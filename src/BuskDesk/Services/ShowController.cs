using BuskDesk.Core;
using BuskDesk.Model;
using BuskDesk.Output;

namespace BuskDesk.Services;

/// <summary>Application façade over engine, patch, scenes and outputs.</summary>
public sealed class ShowController : IAsyncDisposable
{
    public DmxEngine Engine { get; } = new();
    public OutputSettings Settings { get; private set; } = new();
    public List<FixtureDefinition> Definitions { get; } = [];
    public List<PatchedFixture> Patch { get; } = [];
    public List<SceneLook> Scenes { get; } = [];
    public SubmasterSlot[] Submasters { get; } = Enumerable.Range(1, 12)
        .Select(i => new SubmasterSlot { Name = $"SM {i}" })
        .ToArray();

    public byte[] PresetA { get; } = new byte[DmxUniverse.ChannelCount];
    public byte[] PresetB { get; } = new byte[DmxUniverse.ChannelCount];
    public double Crossfade { get; private set; }

    public event Action? Changed;

    public async Task InitializeAsync()
    {
        Settings = JsonStore.LoadSettings();
        Definitions.Clear();
        Definitions.AddRange(JsonStore.LoadFixtures());

        var map = Definitions.ToDictionary(d => d.Id);
        Patch.Clear();
        Patch.AddRange(JsonStore.LoadPatch(map));
        Scenes.Clear();
        Scenes.AddRange(JsonStore.LoadScenes());

        ApplyIntensityMask();
        await ApplyOutputAsync(Settings.SelectedOutputId).ConfigureAwait(false);
        Engine.Start();
        RebuildDeskMerge();
        Changed?.Invoke();
    }

    public async Task ApplyOutputAsync(string outputId)
    {
        Settings.SelectedOutputId = outputId;
        if (outputId == "artnet-unicast" && string.IsNullOrWhiteSpace(Settings.ArtNetHost))
        {
            Settings.ArtNetHost = "192.168.1.10";
        }

        if (outputId == "air2dmx")
        {
            Settings.ArtNetHost = "2.0.0.1";
            Settings.ArtNetUniverse = 0;
        }

        var device = OutputCatalog.Create(outputId, Settings);
        if (device is ArtNetOutput art && outputId == "artnet-unicast")
        {
            art.TargetHost = Settings.ArtNetHost;
            art.Universe = Settings.ArtNetUniverse;
        }

        if (device is EnttecProOutput pro)
        {
            pro.PortName = Settings.SerialPort;
        }

        await Engine.SetOutputAsync(device).ConfigureAwait(false);
        JsonStore.SaveSettings(Settings);
        Changed?.Invoke();
    }

    public void SetCrossfade(double t)
    {
        Crossfade = Math.Clamp(t, 0, 1);
        RebuildDeskMerge();
    }

    public void SetPresetA(int address1Based, byte value)
    {
        PresetA[address1Based - 1] = value;
        RebuildDeskMerge();
    }

    public void SetPresetB(int address1Based, byte value)
    {
        PresetB[address1Based - 1] = value;
        RebuildDeskMerge();
    }

    public void CaptureScene(string name, int fadeMs = 1000)
    {
        var look = new SceneLook
        {
            Name = name,
            FadeMs = fadeMs,
            Channels = Engine.SnapshotOutput()
        };
        Scenes.Add(look);
        JsonStore.SaveScenes(Scenes);
        Changed?.Invoke();
    }

    public void RecallScene(SceneLook scene)
    {
        Engine.SetSceneSnapshot(scene.Channels);
        Changed?.Invoke();
    }

    public void DeleteScene(SceneLook scene)
    {
        Scenes.Remove(scene);
        JsonStore.SaveScenes(Scenes);
        Engine.ClearScene();
        Changed?.Invoke();
    }

    public void StoreSubmaster(int index, string? name = null)
    {
        if (index is < 0 or > 11) return;
        Submasters[index].Channels = Engine.SnapshotOutput();
        if (!string.IsNullOrWhiteSpace(name))
        {
            Submasters[index].Name = name!;
        }
        else if (Submasters[index].Name is "—" or null || Submasters[index].Name.StartsWith("SM "))
        {
            Submasters[index].Name = $"Look {index + 1}";
        }

        Changed?.Invoke();
    }

    public void SetSubmasterLevel(int index, byte level)
    {
        if (index is < 0 or > 11) return;
        Submasters[index].Level = level;
        // Active submaster = highest level with content (simple HTP across stored looks)
        byte[]? best = null;
        byte bestLevel = 0;
        foreach (var slot in Submasters)
        {
            if (slot.Channels is null) continue;
            if (slot.Level >= bestLevel)
            {
                bestLevel = slot.Level;
                best = slot.Channels;
            }
        }

        if (best is null || bestLevel == 0)
        {
            Engine.ClearSubmaster();
        }
        else
        {
            Engine.SetSubmasterSnapshot(best, bestLevel);
        }

        Changed?.Invoke();
    }

    public FixtureDefinition SaveProbeAsFixture(
        string manufacturer,
        string model,
        string modeName,
        int channelCount,
        IReadOnlyList<(string name, ChannelRole role, byte value)> channels)
    {
        var def = new FixtureDefinition
        {
            Manufacturer = manufacturer,
            Model = model,
            ModeName = modeName,
            Channels = channels.Select((c, i) => new FixtureChannel
            {
                Offset = i,
                Name = c.name,
                Role = c.role,
                DefaultValue = c.role.IsProtected() ? c.value : (byte)0
            }).ToList()
        };

        // Ensure length
        while (def.Channels.Count < channelCount)
        {
            def.Channels.Add(new FixtureChannel
            {
                Offset = def.Channels.Count,
                Name = $"Ch {def.Channels.Count + 1}",
                Role = ChannelRole.Raw
            });
        }

        Definitions.Add(def);
        JsonStore.SaveFixture(def);
        Changed?.Invoke();
        return def;
    }

    public void AddToPatch(FixtureDefinition definition, int startAddress, string? name = null, string group = "Default")
    {
        var patched = new PatchedFixture
        {
            DefinitionId = definition.Id,
            Definition = definition,
            Name = name ?? definition.Model,
            StartAddress = startAddress,
            Group = group
        };
        Patch.Add(patched);
        JsonStore.SavePatch(Patch);
        ApplyIntensityMask();
        Changed?.Invoke();
    }

    public void RemoveFromPatch(PatchedFixture fixture)
    {
        Patch.Remove(fixture);
        JsonStore.SavePatch(Patch);
        ApplyIntensityMask();
        Changed?.Invoke();
    }

    public void ApplyIntensityMask()
    {
        var intensity = new bool[DmxUniverse.ChannelCount];
        Array.Fill(intensity, true);
        foreach (var p in Patch)
        {
            if (p.Definition is null) continue;
            foreach (var ch in p.Definition.Channels)
            {
                var addr = p.StartAddress + ch.Offset;
                if (addr is < 1 or > 512) continue;
                intensity[addr - 1] = ch.Role.IsIntensityFamily();
            }
        }

        Engine.SetIntensityMask(a => intensity[a - 1]);
    }

    private void RebuildDeskMerge()
    {
        var merged = new byte[DmxUniverse.ChannelCount];
        var t = Crossfade;
        for (var i = 0; i < DmxUniverse.ChannelCount; i++)
        {
            // Classic two-scene: A contributes (1-t), B contributes t, HTP
            var a = (byte)(PresetA[i] * (1.0 - t));
            var b = (byte)(PresetB[i] * t);
            merged[i] = Math.Max(a, b);
        }

        Engine.SetDeskSnapshot(merged);
        Changed?.Invoke();
    }

    public async ValueTask DisposeAsync() => await Engine.DisposeAsync().ConfigureAwait(false);
}
