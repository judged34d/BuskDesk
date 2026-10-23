using Avalonia.Controls;
using Avalonia.Input;
using BuskDesk.ViewModels;

namespace BuskDesk.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;

        if (e.Key is Key.Left or Key.Right or Key.Up or Key.Down)
        {
            var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
            vm.HandleMoveKeys(e.Key, shift);
            if (vm.IsMove)
            {
                e.Handled = true;
            }
        }
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
