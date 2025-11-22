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

    public static NavigationService GlobalNav;

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 👉 utilise le constructeur SANS arguments !
            var mainVm = new MainWindowViewModel();

            // Navigation globale
            GlobalNav = mainVm.Nav;

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainVm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}