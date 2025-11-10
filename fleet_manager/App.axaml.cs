using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FleetManager.Services;
using FleetManager.ViewModels;
using FleetManager.Views;

namespace FleetManager;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var state = new AppState();
            var mainVm = new MainWindowViewModel(state);
            var nav = new NavigationService(mainVm);
            // page de démarrage :
            nav.GoToRegister();

            desktop.MainWindow = new MainWindow { DataContext = mainVm };
        }

        base.OnFrameworkInitializationCompleted();
    }
}