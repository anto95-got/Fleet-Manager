using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManager.Services;
using FleetManager.Data;
using FleetManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.ViewModels;

public class InscriptionViewModel : BaseViewModel
{
    private readonly NavigationService _nav;
    private readonly AppState _state;

    // Champs du formulaire (Pas de mot de passe, c'est auto)
    private string _nom = "";
    private string _prenom = "";
    private string _email = "";
    private string _error = "";
    private string _successMessage = "";

    public string Nom { get => _nom; set { _nom = value; OnPropertyChanged(); } }
    public string Prenom { get => _prenom; set { _prenom = value; OnPropertyChanged(); } }
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }
    public string SuccessMessage { get => _successMessage; set { _successMessage = value; OnPropertyChanged(); } }

    public ICommand RegisterCommand { get; }
    public ICommand GoBackCommand { get; }

    public InscriptionViewModel(NavigationService nav, AppState state)
    {
        _nav = nav;
        _state = state;

        // VÉRIFICATION SÉCURITÉ : Seuls Admin (2) et SuperAdmin (3) peuvent accéder
        if (_state.CurrentUser == null || _state.CurrentUser.Role < 2)
        {
            _nav.GoToHome(); // Ejection
        }

        RegisterCommand = new RelayCommand(RegisterAsync);
        // Le bouton retour renvoie au tableau de bord Admin
        GoBackCommand = new RelayCommand(() => _nav.GoToAdmin());
    }

    private async Task RegisterAsync()
    {
        Error = "";
        SuccessMessage = "";

        // 1. Validation des champs
        if (string.IsNullOrWhiteSpace(Nom) || string.IsNullOrWhiteSpace(Prenom) || string.IsNullOrWhiteSpace(Email))
        {
            Error = "Veuillez remplir le Nom, le Prénom et l'Email.";
            return;
        }

        try
        {
            using var ctx = new FleetDbContext();
            var emailNorm = Email.Trim().ToLower();

            // 2. Vérifier unicité email
            bool existe = await ctx.Users.AnyAsync(u => u.Email == emailNorm);
            if (existe)
            {
                Error = "Cet email est déjà enregistré.";
                return;
            }

            // 3. Générer un mot de passe aléatoire (10 caractères)
            string passwordClair = GenererMotDePasse(10);

            // 4. Tenter l'envoi de l'email AVANT d'enregistrer
            // Si l'envoi échoue, on passe dans le catch et on ne crée pas le user
            try 
            {
                EmailService.EnvoyerIdentifiants(emailNorm, Nom, passwordClair);
            }
            catch (Exception exMail)
            {
                Error = "Impossible d'envoyer l'email. L'inscription est annulée.\n" + exMail.Message;
                return; 
            }

            // 5. Création de l'utilisateur en BDD
            var hash = BCrypt.Net.BCrypt.HashPassword(passwordClair);

            var newUser = new User
            {
                Nom = Nom.Trim(),
                Prenom = Prenom.Trim(),
                Email = emailNorm,
                PasswordHash = hash,
                Role = 1 // Par défaut "Utilisateur" simple
            };

            ctx.Users.Add(newUser);
            await ctx.SaveChangesAsync();

            // 6. Succès
            SuccessMessage = $"Utilisateur créé et email envoyé à {emailNorm}.";
            
            // On vide les champs pour enchainer
            Nom = ""; Prenom = ""; Email = "";
        }
        catch (Exception ex)
        {
            Error = "Erreur technique : " + ex.Message;
        }
    }

    private string GenererMotDePasse(int longueur)
    {
        const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, longueur)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}