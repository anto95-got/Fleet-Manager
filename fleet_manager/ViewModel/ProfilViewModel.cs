using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManager.Models;
using FleetManager.Services;
using FleetManager.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.ViewModels;

public class ProfilViewModel : BaseViewModel
{
    private readonly NavigationService _nav;
    private readonly AppState _state;

    private string _nom;
    private string _prenom;
    private string _email;
    private string _newPassword = "";
    private string _confirmPassword = "";
    private string _message = "";
    private string _messageColor = "Black";

    public string Nom { get => _nom; set { _nom = value; OnPropertyChanged(); } }
    public string Prenom { get => _prenom; set { _prenom = value; OnPropertyChanged(); } }
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    
    public string NewPassword { get => _newPassword; set { _newPassword = value; OnPropertyChanged(); } }
    public string ConfirmPassword { get => _confirmPassword; set { _confirmPassword = value; OnPropertyChanged(); } }
    
    public string Message { get => _message; set { _message = value; OnPropertyChanged(); } }
    public string MessageColor { get => _messageColor; set { _messageColor = value; OnPropertyChanged(); } }

    public ICommand SaveCommand { get; }
    public ICommand GoBackCommand { get; }

    public ProfilViewModel(NavigationService nav, AppState state)
    {
        _nav = nav;
        _state = state;

        // Charger les infos actuelles
        if (_state.CurrentUser != null)
        {
            Nom = _state.CurrentUser.Nom;
            Prenom = _state.CurrentUser.Prenom;
            Email = _state.CurrentUser.Email;
        }

        GoBackCommand = new RelayCommand(() => _nav.GoToHome());
        SaveCommand = new RelayCommand(async () => await Sauvegarder());
    }

    private async Task Sauvegarder()
    {
        Message = "";
        if (string.IsNullOrWhiteSpace(Nom) || string.IsNullOrWhiteSpace(Email))
        {
            Message = "Nom et Email obligatoires."; MessageColor = "Red"; return;
        }

        // Vérif mot de passe si l'utilisateur veut le changer
        bool changePass = !string.IsNullOrWhiteSpace(NewPassword);
        if (changePass && NewPassword != ConfirmPassword)
        {
            Message = "Les mots de passe ne correspondent pas."; MessageColor = "Red"; return;
        }

        try
        {
            using var ctx = new FleetDbContext();
            var userDb = await ctx.Users.FindAsync(_state.CurrentUser.Id);
            
            if (userDb != null)
            {
                userDb.Nom = Nom;
                userDb.Prenom = Prenom;
                userDb.Email = Email;

                if (changePass)
                {
                    // Hachage du mot de passe (Sécurité)
                    userDb.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                }

                await ctx.SaveChangesAsync();
                
                // Mettre à jour l'état local
                _state.CurrentUser.Nom = Nom;
                _state.CurrentUser.Prenom = Prenom;
                _state.CurrentUser.Email = Email;

                Message = "Profil mis à jour avec succès !"; 
                MessageColor = "Green";
                NewPassword = ""; ConfirmPassword = ""; // Reset champs mdp
            }
        }
        catch (Exception ex)
        {
            Message = "Erreur : " + ex.Message; MessageColor = "Red";
        }
    }
}