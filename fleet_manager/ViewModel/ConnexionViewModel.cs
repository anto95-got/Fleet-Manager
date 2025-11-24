using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManager.Services;
using FleetManager.Data;
using Microsoft.EntityFrameworkCore;
using FleetManager.Models; // Assure-toi d'avoir ce using

namespace FleetManager.ViewModels;

public class ConnexionViewModel : BaseViewModel
{
    private readonly NavigationService _nav;
    private readonly AppState _state;

    // --- Champs Connexion ---
    private string _email = "";
    private string _password = "";
    private string _error = "";
    
    // --- Champs Nouveau Mot de Passe (Popup) ---
    private bool _isChangePasswordOpen;
    private string _newPassword = "";
    private string _confirmNewPassword = "";
    private string _popupError = "";

    // Propriétés Connexion
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }
    public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }

    // Propriétés Popup
    public bool IsChangePasswordOpen 
    { 
        get => _isChangePasswordOpen; 
        set { _isChangePasswordOpen = value; OnPropertyChanged(); } 
    }
    public string NewPassword { get => _newPassword; set { _newPassword = value; OnPropertyChanged(); } }
    public string ConfirmNewPassword { get => _confirmNewPassword; set { _confirmNewPassword = value; OnPropertyChanged(); } }
    public string PopupError { get => _popupError; set { _popupError = value; OnPropertyChanged(); } }

    // Commandes
    public ICommand LoginCommand { get; }
    public ICommand GoRegisterCommand { get; }
    public ICommand SubmitNewPasswordCommand { get; } // Nouvelle commande

    public ConnexionViewModel(NavigationService nav, AppState state)
    {
        _nav = nav;
        _state = state;

        LoginCommand = new RelayCommand(LoginAsync);
        GoRegisterCommand = new RelayCommand(() => _nav.GoToRegister());
        SubmitNewPasswordCommand = new RelayCommand(ChangePasswordAsync);
    }

    private async Task LoginAsync()
    {
        Error = "";

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        { Error = "Email et mot de passe requis."; return; }

        try
        {   
            using var ctx = new FleetDbContext(); // Utilise le constructeur par défaut si configuré
            var emailNorm = Email.Trim().ToLower();

            var user = await ctx.Users.FirstOrDefaultAsync(u => u.Email == emailNorm);
            
            if (user is null || !BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
            { Error = "Identifiants invalides."; return; }

            // Stocker l'utilisateur dans l'état
            _state.CurrentUser = user;

            // --- VÉRIFICATION DU MOT DE PASSE ---
            if (user.MustChangePassword)
            {
                // Si TRUE : On ouvre le popup et on arrête la navigation
                IsChangePasswordOpen = true;
                PopupError = "Vous devez modifier votre mot de passe pour la première connexion.";
                return; 
            }

            // Si FALSE : On va à l'accueil directement
            _nav.GoToHome();
            
        }
        catch (Exception ex)
        {
            Error = "Erreur : " + ex.Message;
        }
    }

    // Nouvelle fonction pour enregistrer le nouveau mot de passe
    private async Task ChangePasswordAsync()
    {
        PopupError = "";

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            PopupError = "Le nouveau mot de passe est vide."; return;
        }

        if (NewPassword != ConfirmNewPassword)
        {
            PopupError = "Les mots de passe ne correspondent pas."; return;
        }

        if (NewPassword == Password)
        {
            PopupError = "Le nouveau mot de passe doit être différent de l'ancien."; return;
        }

        try
        {
            using var ctx = new FleetDbContext();
            // On recharge l'utilisateur depuis la BDD pour être sûr
            var userToUpdate = await ctx.Users.FindAsync(_state.CurrentUser.Id);

            if (userToUpdate != null)
            {
                // 1. Hasher le nouveau mot de passe
                userToUpdate.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                
                // 2. Désactiver l'obligation de changement
                userToUpdate.MustChangePassword = false;

                // 3. Sauvegarder
                await ctx.SaveChangesAsync();

                // 4. Mettre à jour l'état local
                _state.CurrentUser = userToUpdate;

                // 5. Fermer popup et aller à l'accueil
                IsChangePasswordOpen = false;
                _nav.GoToHome();
            }
        }
        catch (Exception ex)
        {
            PopupError = "Erreur SQL : " + ex.Message;
        }
    }
}