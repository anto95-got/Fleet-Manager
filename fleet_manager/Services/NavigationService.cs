using System.Windows.Input;
using FleetManager.Models;
using FleetManager.ViewModels;

namespace FleetManager.Services;

public class NavigationService
{
    private readonly MainWindowViewModel _main;

    // 🔥 Commandes globales (utilisables partout)
    public ICommand LogoutCommand { get; }
    public ICommand GoHomeCommand { get; }
    public ICommand GoVehiculesCommand { get; }

    public ICommand GoSuivieCommand { get; }

    public NavigationService(MainWindowViewModel main)
    {
        _main = main;

        // Initialisation des commandes globales
        LogoutCommand = new RelayCommand(Logout);
        GoHomeCommand = new RelayCommand(GoToHome);
        GoVehiculesCommand = new RelayCommand(GoToVehicules);
        GoSuivieCommand = new RelayCommand(GoToSuivie);
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

    public void GoToSuivie()
    {
        if (_main.State.CurrentUser == null)
        {
            GoToLogin();
            return;
        }
        _main.CurrentView = new SuivieViewModel(this, _main.State);
    }

    public void GoToRegister()
    {    
        if (_main.State.CurrentUser?.Role is not  (2 or 3) )
        {
            GoToHome();
            return;
        }
        _main.CurrentView = new InscriptionViewModel(this, _main.State);

        
    }
    public void GoToProfil() => _main.CurrentView = new ProfilViewModel(this, _main.State);
    public void GoToAdmin() => _main.CurrentView = new GestionUtilisateursViewModel(this, _main.State);
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
    
    // Ancienne méthode (pour info, on garde si tu t'en sers ailleurs)
   

    // --- NOUVELLE MÉTHODE AJOUTÉE POUR TA DEMANDE ---
    // Ajout du paramètre 'vientDeVehicule'
    public void GoToHistoriqueDetail(Suivi s, bool vientDeVehicule)
    {
        if (_main.State.CurrentUser == null)
        {
            GoToLogin();
            return;
        }
    
        // On transmet l'info au ViewModel
        _main.CurrentView = new HistoriqueVehiculeViewModel(this, _main.State, s, vientDeVehicule);
    }
}