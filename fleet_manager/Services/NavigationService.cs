using FleetManager.ViewModels;

namespace FleetManager.Services;

public class NavigationService
{
    private readonly MainWindowViewModel _main;

    public NavigationService(MainWindowViewModel main) => _main = main;

    public void GoToLogin()
        => _main.CurrentView = new ConnexionViewModel(this, _main.State);

    public void GoToRegister()
        => _main.CurrentView = new InscriptionViewModel(this, _main.State);

    // À remplacer par ton vrai Dashboard plus tard
    public void GoToDashboard()
        => _main.CurrentView = new ConnexionViewModel(this, _main.State);
}