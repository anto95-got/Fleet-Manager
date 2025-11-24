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

    // Champs formulaire (Plus de password ici)
    private string _nom = "";
    private string _prenom = "";
    private string _email = "";
    private string _error = "";
    private string _successMessage = ""; // Pour confirmer à l'admin

    public string Nom { get => _nom; set { _nom = value; OnPropertyChanged(); } }
    public string Prenom { get => _prenom; set { _prenom = value; OnPropertyChanged(); } }
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }
    public string SuccessMessage { get => _successMessage; set { _successMessage = value; OnPropertyChanged(); } }

    public ICommand RegisterCommand { get; }
    public ICommand GoBackCommand { get; } // Retour au tableau de bord (pas login)
    

    public InscriptionViewModel(NavigationService nav, AppState state)
    {
        _nav = nav;
        _state = state;

        // VÉRIFICATION DE SÉCURITÉ AU CHARGEMENT
        // Si l'utilisateur n'est pas connecté ou n'est pas Admin/SuperAdmin
        if (_state.CurrentUser == null || _state.CurrentUser.Role == 1) // 1 = Utilisateur simple
        {
            // On le renvoie à l'accueil ou au login
            _nav.GoToHome();
        }

        RegisterCommand = new RelayCommand(RegisterAsync);
        // Le bouton retour renvoie à la gestion des utilisateurs ou home
        GoBackCommand = new RelayCommand(() => _nav.GoToAdmin()); 
    }

    private async Task RegisterAsync()
    {
        Error = "";
        SuccessMessage = "";

        // 1. Validation basique
        if (string.IsNullOrWhiteSpace(Nom) || string.IsNullOrWhiteSpace(Prenom) || string.IsNullOrWhiteSpace(Email))
        {
            Error = "Nom, Prénom et Email sont obligatoires.";
            return;
        }

        try
        {
            using var ctx = new FleetDbContext();
            var emailNorm = Email.Trim().ToLower();

            // 2. Vérifier si l'email existe déjà
            var exists = await ctx.Users.AnyAsync(u => u.Email == emailNorm);
            if (exists)
            {
                Error = "Cet email est déjà utilisé par un autre collaborateur.";
                return;
            }

            // 3. GÉNÉRATION DU MOT DE PASSE ALÉATOIRE
            string randomPassword = GenererMotDePasse(10);

            // 4. ENVOI DU MAIL (Avant la sauvegarde BDD pour être sûr que ça part)
            try 
            {
                // De-commente la ligne ci-dessous quand tu as configuré EmailService
                // EmailService.EnvoyerMotDePasse(emailNorm, Nom, randomPassword);
                
                // Pour le test tant que tu n'as pas de serveur SMTP, on l'affiche dans la console ou en Messagebox
               // Console.WriteLine($"[EMAIL SIMULÉ] Pour: {emailNorm} | Pass: {randomPassword}");
            }
            catch (Exception emailEx)
            {
                Error = "Impossible d'envoyer l'email. L'utilisateur n'a pas été créé. " + emailEx.Message;
                return; // On arrête tout, on ne crée pas le user si l'email ne part pas
            }

            // 5. Hashage et Sauvegarde
            var hash = BCrypt.Net.BCrypt.HashPassword(randomPassword);

            var user = new User
            {
                Nom = Nom.Trim(),
                Prenom = Prenom.Trim(),
                Email = emailNorm,
                PasswordHash = hash,
                Role = 1 // Par défaut on crée un Utilisateur simple (modifier si besoin)
            };

            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            // 6. Succès et Nettoyage
            SuccessMessage = $"Utilisateur créé avec succès ! Le mot de passe a été envoyé à {emailNorm}.";
            
            // On vide les champs pour permettre d'en créer un autre
            Nom = "";
            Prenom = "";
            Email = "";
            
            // On ne change PAS de vue (_nav.GoToHome) car l'admin veut peut-être en créer d'autres.
        }
        catch (Exception ex)
        {
            Error = "Erreur BDD : " + ex.Message;
        }
    }

    // Méthode utilitaire pour générer un mot de passe
    private string GenererMotDePasse(int longueur)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, longueur)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}