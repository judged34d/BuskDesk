using Avalonia.Controls;
using Avalonia.Input;
using BuskDesk.ViewModels;

namespace BuskDesk.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ProbeChannel_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border { DataContext: FaderChannelViewModel channel }
            && DataContext is MainViewModel vm)
        {
            vm.SelectedProbeChannel = channel;
        }
    }
}
