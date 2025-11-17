using FleetManager.ViewModels;

namespace FleetManager.Services;

public class NavigationService
{
    private readonly MainWindowViewModel _main;

    public NavigationService(MainWindowViewModel main)
    {
        _main = main;
    }

    public void GoToLogin()
    {
        // Si déjà connecté, on redirige vers l'accueil
        if (_main.State.CurrentUser != null)
        {
            GoToHome();
            return;
        }

        _main.CurrentView = new ConnexionViewModel(this, _main.State);
    }

    public void GoToRegister()
    {    
        if (_main.State.CurrentUser != null)
        {
            GoToHome();
            return;
        }
        _main.CurrentView = new InscriptionViewModel(this, _main.State);
    }

    public void GoToHome()
    {   
        if (_main.State.CurrentUser == null)
        {
            GoToLogin();
            return;
        }
        _main.CurrentView = new AcceuilViewModel(this, _main.State, _main.Db);
    }

    public void GoToVehicules()
    {   
        if (_main.State.CurrentUser == null)
        {
            GoToLogin();
            return;
        }
        _main.CurrentView = new ModificationVehiculeViewModel(this, _main.State);
    }
}