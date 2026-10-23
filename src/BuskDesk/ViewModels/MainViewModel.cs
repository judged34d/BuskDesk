using System.Collections.ObjectModel;
using Avalonia.Threading;
using BuskDesk.Core;
using BuskDesk.Model;
using BuskDesk.Models;
using BuskDesk.Output;
using BuskDesk.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BuskDesk.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ShowController _show;
    private readonly DispatcherTimer _statusTimer;

    public MainViewModel(ShowController show)
    {
        _show = show;
        _show.Changed += () => Dispatcher.UIThread.Post(RefreshFromController);

        for (var i = 1; i <= 8; i++)
        {
            var addr = i;
            PresetA.Add(new FaderChannelViewModel
            {
                Address = addr,
                Label = $"A{i}",
                Role = $"Ch {i}",
                ValueChanged = (a, v) => _show.SetPresetA(a, v)
            });
            PresetB.Add(new FaderChannelViewModel
            {
                Address = addr,
                Label = $"B{i}",
                Role = $"Ch {i}",
                ValueChanged = (a, v) => _show.SetPresetB(a, v)
            });
        }

        for (var i = 1; i <= 16; i++)
        {
            var addr = i;
            ProbeChannels.Add(new FaderChannelViewModel
            {
                Address = addr,
                Label = $"{i:D3}",
                Role = ChannelRole.Raw.DisplayName(),
                ValueChanged = OnProbeValueChanged
            });
        }

        for (var i = 0; i < 12; i++)
        {
            var slot = new SubmasterViewModel { Index = i, Name = $"SM {i + 1}" };
            slot.LevelChanged = s => _show.SetSubmasterLevel(s.Index, (byte)Math.Clamp((int)s.Level, 0, 255));
            Submasters.Add(slot);
        }

        OutputOptions = new ObservableCollection<OutputOption>(OutputCatalog.Options);
        SelectedOutput = OutputOptions[0];
        AvailableRoles = Enum.GetValues<ChannelRole>().ToList();

        _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _statusTimer.Tick += (_, _) => RefreshStatus();
        _statusTimer.Start();
    }

    public string AppName => "Busk Desk";
    public string VersionLabel => "v0.2 · Functional";

    public ObservableCollection<FaderChannelViewModel> PresetA { get; } = [];
    public ObservableCollection<FaderChannelViewModel> PresetB { get; } = [];
    public ObservableCollection<FaderChannelViewModel> ProbeChannels { get; } = [];
    public ObservableCollection<SubmasterViewModel> Submasters { get; } = [];
    public ObservableCollection<SceneLook> Scenes { get; } = [];
    public ObservableCollection<PatchedFixture> Patch { get; } = [];
    public ObservableCollection<FixtureDefinition> Definitions { get; } = [];
    public ObservableCollection<OutputOption> OutputOptions { get; }
    public IReadOnlyList<ChannelRole> AvailableRoles { get; }
    public ObservableCollection<string> SerialPorts { get; } = [];

    [ObservableProperty] private AppSection _currentSection = AppSection.Desk;
    [ObservableProperty] private double _grandMaster = 255;
    [ObservableProperty] private double _crossfade;
    [ObservableProperty] private bool _blackout;
    [ObservableProperty] private string _statusLine = "Starting…";
    [ObservableProperty] private string _outputStatus = "—";
    [ObservableProperty] private string _framesLabel = "0 fps";
    [ObservableProperty] private OutputOption? _selectedOutput;
    [ObservableProperty] private string _artNetHost = "2.0.0.1";
    [ObservableProperty] private string _serialPort = "";
    [ObservableProperty] private string _newSceneName = "Neue Szene";
    [ObservableProperty] private SceneLook? _selectedScene;
    [ObservableProperty] private FixtureDefinition? _selectedDefinition;
    [ObservableProperty] private int _patchStartAddress = 1;
    [ObservableProperty] private string _patchName = "";
    [ObservableProperty] private int _probeStartAddress = 1;
    [ObservableProperty] private int _probeChannelCount = 12;
    [ObservableProperty] private string _fixtureManufacturer = "U'King";
    [ObservableProperty] private string _fixtureModel = "Moving Head";
    [ObservableProperty] private string _fixtureMode = "12ch";
    [ObservableProperty] private ChannelRole _selectedProbeRole = ChannelRole.Raw;
    [ObservableProperty] private FaderChannelViewModel? _selectedProbeChannel;
    [ObservableProperty] private string _toast = "";

    public bool IsDesk => CurrentSection == AppSection.Desk;
    public bool IsScenes => CurrentSection == AppSection.Scenes;
    public bool IsPatch => CurrentSection == AppSection.Patch;
    public bool IsProbe => CurrentSection == AppSection.Probe;
    public bool IsOutput => CurrentSection == AppSection.Output;
    public bool IsAssistant => CurrentSection == AppSection.Assistant;
    public bool IsTracks => CurrentSection == AppSection.Tracks;

    partial void OnCurrentSectionChanged(AppSection value)
    {
        OnPropertyChanged(nameof(IsDesk));
        OnPropertyChanged(nameof(IsScenes));
        OnPropertyChanged(nameof(IsPatch));
        OnPropertyChanged(nameof(IsProbe));
        OnPropertyChanged(nameof(IsOutput));
        OnPropertyChanged(nameof(IsAssistant));
        OnPropertyChanged(nameof(IsTracks));
    }

    partial void OnGrandMasterChanged(double value) =>
        _show.Engine.SetGrandMaster((byte)Math.Clamp((int)Math.Round(value), 0, 255));

    partial void OnCrossfadeChanged(double value) => _show.SetCrossfade(value);

    partial void OnBlackoutChanged(bool value) => _show.Engine.SetBlackout(value);

    partial void OnProbeStartAddressChanged(int value) => RelabelProbe();

    partial void OnProbeChannelCountChanged(int value)
    {
        value = Math.Clamp(value, 1, 32);
        if (value != ProbeChannelCount)
        {
            ProbeChannelCount = value;
            return;
        }

        while (ProbeChannels.Count < value)
        {
            var addr = ProbeStartAddress + ProbeChannels.Count;
            ProbeChannels.Add(new FaderChannelViewModel
            {
                Address = addr,
                Label = $"{addr:D3}",
                Role = ChannelRole.Raw.DisplayName(),
                ValueChanged = OnProbeValueChanged
            });
        }

        while (ProbeChannels.Count > value)
        {
            ProbeChannels.RemoveAt(ProbeChannels.Count - 1);
        }

        RelabelProbe();
    }

    public void RefreshFromController()
    {
        Scenes.Clear();
        foreach (var s in _show.Scenes) Scenes.Add(s);

        Patch.Clear();
        foreach (var p in _show.Patch) Patch.Add(p);

        Definitions.Clear();
        foreach (var d in _show.Definitions) Definitions.Add(d);
        SelectedDefinition ??= Definitions.FirstOrDefault(d => d.Id.Contains("uking", StringComparison.OrdinalIgnoreCase))
                               ?? Definitions.FirstOrDefault();

        for (var i = 0; i < Submasters.Count; i++)
        {
            Submasters[i].Name = _show.Submasters[i].Name;
            Submasters[i].Level = _show.Submasters[i].Level;
            Submasters[i].HasContent = _show.Submasters[i].HasContent;
        }

        ArtNetHost = _show.Settings.ArtNetHost;
        SerialPort = _show.Settings.SerialPort;
        SelectedOutput = OutputOptions.FirstOrDefault(o => o.Id == _show.Settings.SelectedOutputId)
                         ?? OutputOptions[0];

        SyncDeskAndProbeFromPatch();
        RefreshStatus();
        RefreshSerialPorts();
    }

    private void SyncDeskAndProbeFromPatch()
    {
        var first = _show.Patch.FirstOrDefault(p => p.Definition is not null);
        if (first?.Definition is null)
        {
            return;
        }

        var def = first.Definition;
        ProbeStartAddress = first.StartAddress;
        ProbeChannelCount = Math.Clamp(def.ChannelCount, 1, 32);

        for (var i = 0; i < ProbeChannels.Count && i < def.Channels.Count; i++)
        {
            var ch = def.Channels[i];
            ProbeChannels[i].Address = first.StartAddress + ch.Offset;
            ProbeChannels[i].Label = $"{ProbeChannels[i].Address:D3}";
            ProbeChannels[i].Role = string.IsNullOrWhiteSpace(ch.Name) ? ch.Role.DisplayName() : ch.Name;
        }

        var deskCount = Math.Min(PresetA.Count, def.Channels.Count);
        for (var i = 0; i < deskCount; i++)
        {
            var ch = def.Channels[i];
            var addr = first.StartAddress + ch.Offset;
            PresetA[i].Address = addr;
            PresetA[i].Role = ch.Name;
            PresetA[i].Label = $"A{i + 1}";
            PresetA[i].ValueChanged = (a, v) => _show.SetPresetA(a, v);

            PresetB[i].Address = addr;
            PresetB[i].Role = ch.Name;
            PresetB[i].Label = $"B{i + 1}";
            PresetB[i].ValueChanged = (a, v) => _show.SetPresetB(a, v);
        }
    }

    private void RefreshStatus()
    {
        var device = _show.Engine.OutputDevice;
        OutputStatus = device.StatusText;
        FramesLabel = $"{_show.Engine.FramesSent} frames";
        var err = _show.Engine.LastError;
        StatusLine = string.IsNullOrEmpty(err)
            ? $"{device.DisplayName} · {device.StatusText}"
            : $"Fehler: {err}";
    }

    private void RefreshSerialPorts()
    {
        SerialPorts.Clear();
        foreach (var p in EnttecProOutput.ListPorts())
        {
            SerialPorts.Add(p);
        }
    }

    private void OnProbeValueChanged(int absoluteAddress, byte value)
    {
        _show.Engine.SetProbeOverride(absoluteAddress, value, enabled: true);
        var vm = ProbeChannels.FirstOrDefault(c => c.Address == absoluteAddress);
        if (vm is not null)
        {
            vm.IsProbeOverride = true;
        }
    }

    private void RelabelProbe()
    {
        for (var i = 0; i < ProbeChannels.Count; i++)
        {
            var addr = ProbeStartAddress + i;
            ProbeChannels[i].Address = addr;
            ProbeChannels[i].Label = $"{addr:D3}";
            ProbeChannels[i].ValueChanged = OnProbeValueChanged;
        }
    }

    [RelayCommand] private void GoDesk() => CurrentSection = AppSection.Desk;
    [RelayCommand] private void GoScenes() => CurrentSection = AppSection.Scenes;
    [RelayCommand] private void GoPatch() => CurrentSection = AppSection.Patch;
    [RelayCommand] private void GoProbe() => CurrentSection = AppSection.Probe;
    [RelayCommand] private void GoOutput() => CurrentSection = AppSection.Output;
    [RelayCommand] private void GoAssistant() => CurrentSection = AppSection.Assistant;
    [RelayCommand] private void GoTracks() => CurrentSection = AppSection.Tracks;

    [RelayCommand]
    private void ToggleBlackout() => Blackout = !Blackout;

    [RelayCommand]
    private void ClearProbe()
    {
        _show.Engine.ClearProbeOverrides();
        foreach (var ch in ProbeChannels)
        {
            ch.IsProbeOverride = false;
            ch.Value = 0;
        }

        Toast = "Sonde gelöst — Overrides aus";
    }

    [RelayCommand]
    private void AssignProbeRole()
    {
        if (SelectedProbeChannel is null) return;
        SelectedProbeChannel.Role = SelectedProbeRole.DisplayName();
        Toast = $"{SelectedProbeChannel.Label} → {SelectedProbeRole.DisplayName()}";
    }

    [RelayCommand]
    private void SaveFixtureFromProbe()
    {
        var channels = ProbeChannels.Select(ch =>
        {
            var role = AvailableRoles.FirstOrDefault(r => r.DisplayName() == ch.Role, ChannelRole.Raw);
            return (ch.Role, role, (byte)Math.Clamp((int)ch.Value, 0, 255));
        }).ToList();

        var def = _show.SaveProbeAsFixture(
            FixtureManufacturer,
            FixtureModel,
            FixtureMode,
            ProbeChannelCount,
            channels);

        Toast = $"Fixture gespeichert: {def.DisplayName}";
        RefreshFromController();
    }

    [RelayCommand]
    private void AddPatch()
    {
        if (SelectedDefinition is null) return;
        _show.AddToPatch(
            SelectedDefinition,
            Math.Clamp(PatchStartAddress, 1, 512),
            string.IsNullOrWhiteSpace(PatchName) ? SelectedDefinition.Model : PatchName);
        PatchStartAddress = Math.Min(512, PatchStartAddress + SelectedDefinition.ChannelCount);
        Toast = "Gerät gepatcht";
        RefreshFromController();
    }

    [RelayCommand]
    private void RemovePatch(PatchedFixture? fixture)
    {
        if (fixture is null) return;
        _show.RemoveFromPatch(fixture);
        Toast = "Aus Patch entfernt";
        RefreshFromController();
    }

    [RelayCommand]
    private void CaptureScene()
    {
        var name = string.IsNullOrWhiteSpace(NewSceneName) ? $"Szene {Scenes.Count + 1}" : NewSceneName;
        _show.CaptureScene(name);
        NewSceneName = $"Szene {Scenes.Count + 1}";
        Toast = $"Szene „{name}“ gespeichert";
        RefreshFromController();
    }

    [RelayCommand]
    private void RecallScene(SceneLook? scene)
    {
        if (scene is null) return;
        _show.RecallScene(scene);
        SelectedScene = scene;
        Toast = $"GO · {scene.Name}";
    }

    [RelayCommand]
    private void DeleteScene(SceneLook? scene)
    {
        if (scene is null) return;
        _show.DeleteScene(scene);
        Toast = "Szene gelöscht";
        RefreshFromController();
    }

    [RelayCommand]
    private void StoreSubmaster(SubmasterViewModel? slot)
    {
        if (slot is null) return;
        _show.StoreSubmaster(slot.Index);
        slot.HasContent = true;
        slot.Name = _show.Submasters[slot.Index].Name;
        Toast = $"{slot.Name} gespeichert";
    }

    [RelayCommand]
    private void SetSubmasterLevel(SubmasterViewModel? slot)
    {
        if (slot is null) return;
        _show.SetSubmasterLevel(slot.Index, (byte)Math.Clamp((int)slot.Level, 0, 255));
    }

    [RelayCommand]
    private async Task ApplyOutputAsync()
    {
        if (SelectedOutput is null) return;
        _show.Settings.ArtNetHost = ArtNetHost;
        _show.Settings.SerialPort = SerialPort;
        try
        {
            await _show.ApplyOutputAsync(SelectedOutput.Id);
            Toast = $"Ausgang: {SelectedOutput.DisplayName}";
        }
        catch (Exception ex)
        {
            Toast = ex.Message;
        }

        RefreshStatus();
    }

    [RelayCommand]
    private void RefreshPorts() => RefreshSerialPorts();
}

public partial class SubmasterViewModel : ObservableObject
{
    public int Index { get; init; }
    public Action<SubmasterViewModel>? LevelChanged { get; set; }

    [ObservableProperty] private string _name = "SM";
    [ObservableProperty] private double _level;
    [ObservableProperty] private bool _hasContent;

    partial void OnLevelChanged(double value) => LevelChanged?.Invoke(this);
}
