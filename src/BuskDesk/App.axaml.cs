using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using BuskDesk.Services;
using BuskDesk.ViewModels;
using BuskDesk.Views;

namespace BuskDesk;

public partial class App : Application
{
    private ShowController? _controller;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _controller = new ShowController();
            var vm = new MainViewModel(_controller);

            desktop.MainWindow = new MainWindow { DataContext = vm };
            desktop.ShutdownRequested += async (_, _) =>
            {
                if (_controller is not null)
                {
                    await _controller.DisposeAsync();
                }
            };

            _ = Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await _controller.InitializeAsync();
                vm.RefreshFromController();
            });
        }

        base.OnFrameworkInitializationCompleted();
    }
}
