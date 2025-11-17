using System;
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

    private string _nom = "";
    private string _prenom = "";
    private string _email = "";
    private string _password = "";
    private string _confirmPassword = "";
    private string _error = "";

    public string Nom { get => _nom; set { _nom = value; OnPropertyChanged(); } }
    public string Prenom { get => _prenom; set { _prenom = value; OnPropertyChanged(); } }
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }
    public string ConfirmPassword { get => _confirmPassword; set { _confirmPassword = value; OnPropertyChanged(); } }
    public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }

    public ICommand RegisterCommand { get; }
    public ICommand GoLoginCommand { get; }

    public InscriptionViewModel(NavigationService nav, AppState state)
    {
        _nav = nav;
        _state = state;

        RegisterCommand = new RelayCommand(RegisterAsync);
        GoLoginCommand = new RelayCommand(() => _nav.GoToLogin());
    }

    private async Task RegisterAsync()
    {
        Error = "";

        if (string.IsNullOrWhiteSpace(Nom) ||
            string.IsNullOrWhiteSpace(Prenom) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password))
        { Error = "Tous les champs sont obligatoires."; return; }

        if (Password != ConfirmPassword)
        { Error = "Les mots de passe ne correspondent pas."; return; }

        try
        {
            using var ctx = new FleetDbContext(new DbContextOptions<FleetDbContext>());
            var emailNorm = Email.Trim().ToLower();

            var exists = await ctx.Users.AnyAsync(u => u.Email == emailNorm);
            if (exists) { Error = "Cet email est déjà utilisé."; return; }

            var hash = BCrypt.Net.BCrypt.HashPassword(Password);

            var user = new User
            {
                Nom = Nom.Trim(),
                Prenom = Prenom.Trim(),
                Email = emailNorm,
                PasswordHash = hash
            };

            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            _state.CurrentUser = user;
            _nav.GoToDashboard(); 
        }
        catch (Exception ex)
        {
            Error = "Erreur : " + ex.Message;
        }
    }
}
