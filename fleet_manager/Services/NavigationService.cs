using System.Windows.Input;
using FleetManager.ViewModels;

namespace FleetManager.Services;

public class NavigationService
{
    private readonly MainWindowViewModel _main;

    // 🔥 Commandes globales (utilisables partout)
    public ICommand LogoutCommand { get; }
    public ICommand GoHomeCommand { get; }
    public ICommand GoVehiculesCommand { get; }

    public NavigationService(MainWindowViewModel main)
    {
        _main = main;

        // Initialisation des commandes globales
        LogoutCommand = new RelayCommand(Logout);
        GoHomeCommand = new RelayCommand(GoToHome);
        GoVehiculesCommand = new RelayCommand(GoToVehicules);
    }

    // =========================================================
    //                     NAVIGATION
    // =========================================================

    public void Logout()
    {
        _main.State.CurrentUser = null;
        GoToLogin();
    }

    public void GoToLogin()
    {
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