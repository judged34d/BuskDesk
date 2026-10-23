using CommunityToolkit.Mvvm.ComponentModel;

namespace BuskDesk.ViewModels;

public partial class FaderChannelViewModel : ObservableObject
{
    [ObservableProperty] private string _label = "";
    [ObservableProperty] private string _role = "";
    [ObservableProperty] private double _value;
    [ObservableProperty] private bool _isHot;
    [ObservableProperty] private bool _isProbeOverride;

    public int Address { get; set; }
    public Action<int, byte>? ValueChanged { get; set; }

    partial void OnValueChanged(double value)
    {
        var byteValue = (byte)Math.Clamp((int)Math.Round(value), 0, 255);
        IsHot = byteValue > 0;
        ValueChanged?.Invoke(Address, byteValue);
    }
}
