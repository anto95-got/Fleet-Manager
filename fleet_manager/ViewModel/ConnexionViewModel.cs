using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManager.Services;
using FleetManager.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.ViewModels;

public class ConnexionViewModel : BaseViewModel
{
    private readonly NavigationService _nav;
    private readonly AppState _state;

    private string _email = "";
    private string _password = "";
    private string _error = "";

    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }
    public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }

    public ICommand LoginCommand { get; }
    public ICommand GoRegisterCommand { get; }

    public ConnexionViewModel(NavigationService nav, AppState state)
    {
        _nav = nav;
        _state = state;

        LoginCommand = new RelayCommand(LoginAsync);
        GoRegisterCommand = new RelayCommand(() => _nav.GoToRegister());
    }

    private async Task LoginAsync()
    {
        Error = "";

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        { Error = "Email et mot de passe requis."; return; }

        try
        {   
            using var ctx = new FleetDbContext(new DbContextOptions<FleetDbContext>());
            var emailNorm = Email.Trim().ToLower();

            var user = await ctx.Users.FirstOrDefaultAsync(u => u.Email == emailNorm);
            if (user is null || !BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
            { Error = "Identifiants invalides."; return; }

            _state.CurrentUser = user;
            
            _nav.GoToHome(); // placeholder
            
        }
        catch (Exception ex)
        {
            Error = "Erreur : " + ex.Message;
        }
    }
}