using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManager.Models;
using FleetManager.Services;
using FleetManager.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.ViewModels;

public class GestionUtilisateursViewModel : BaseViewModel
{
    private readonly NavigationService _nav;
    private readonly AppState _state;

    public ObservableCollection<User> Users { get; set; } = new();
    public ObservableCollection<Role> Roles { get; set; } = new();

    private bool _isFormOpen;
    private string _error = "";
    private string _nom = "";
    private string _prenom = "";
    private string _email = "";
    private string _password = "";
    private Role? _selectedRole;
    private int _currentUserId = 0;

    public bool IsFormOpen { get => _isFormOpen; set { _isFormOpen = value; OnPropertyChanged(); } }
    public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }
    public string Nom { get => _nom; set { _nom = value; OnPropertyChanged(); } }
    public string Prenom { get => _prenom; set { _prenom = value; OnPropertyChanged(); } }
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }
    public Role? SelectedRole { get => _selectedRole; set { _selectedRole = value; OnPropertyChanged(); } }

    public ICommand GoBackCommand { get; }
    public ICommand OpenAddCommand { get; }
    public ICommand OpenEditCommand { get; }
    public ICommand CloseFormCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand GoToRegister { get; }

    public GestionUtilisateursViewModel(NavigationService nav, AppState state)
    {
        _nav = nav;
        _state = state;

        GoBackCommand = new RelayCommand(() => _nav.GoToHome());
        OpenAddCommand = new RelayCommand(PrepareAjout);
        OpenEditCommand = new RelayCommand<User>(PrepareEdit);
        CloseFormCommand = new RelayCommand(() => IsFormOpen = false);
        SaveCommand = new RelayCommand(async () => await SaveUser());
        DeleteCommand = new RelayCommand<User>(async (u) => await DeleteUser(u));
        GoToRegister = new RelayCommand(() => _nav.GoToRegister());

        _ = ChargerDonnees();
    }

    private async Task ChargerDonnees()
    {
        using var ctx = new FleetDbContext();
        
        var rolesDb = await ctx.Roles.ToListAsync();
        Roles.Clear();
        foreach (var r in rolesDb) Roles.Add(r);

        // 🔥 On inclut RoleInfo pour avoir le nom
        var usersDb = await ctx.Users.Include(u => u.RoleInfo).ToListAsync();
        Users.Clear();
        foreach (var u in usersDb) Users.Add(u);
    }

    private void PrepareAjout()
    {
        _currentUserId = 0;
        Nom = ""; Prenom = ""; Email = ""; Password = ""; 
        // On cherche l'ID 1 (Utilisateur par défaut)
        SelectedRole = Roles.FirstOrDefault(r => r.Id_role == 1); 
        Error = ""; IsFormOpen = true;
    }

    private void PrepareEdit(User u)
    {
        // Sécurité : on ne touche pas au Super Admin (ID 1)
        if (u.Id == 1 && _state.CurrentUser.Id != 1) return;

        _currentUserId = u.Id;
        Nom = u.Nom; Prenom = u.Prenom; Email = u.Email; 
        Password = ""; 
        // On retrouve le rôle via l'int 'Role' de l'utilisateur
        SelectedRole = Roles.FirstOrDefault(r => r.Id_role == u.Role);
        Error = ""; IsFormOpen = true;
    }

    private async Task SaveUser()
    {
        if (string.IsNullOrWhiteSpace(Nom) || string.IsNullOrWhiteSpace(Email) || SelectedRole == null) { Error = "Champs manquants."; return; }

        try
        {
            using var ctx = new FleetDbContext();

            if (_currentUserId == 0) // AJOUT
            {
                if (string.IsNullOrWhiteSpace(Password)) { Error = "Mdp requis."; return; }

                var newUser = new User
                {
                    Nom = Nom, Prenom = Prenom, Email = Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password),
                    Role = SelectedRole.Id_role // On sauvegarde l'ID (int)
                };
                ctx.Users.Add(newUser);
            }
            else // MODIF
            {
                var u = await ctx.Users.FindAsync(_currentUserId);
                if (u != null)
                {
                    u.Nom = Nom; u.Prenom = Prenom; u.Email = Email;
                    u.Role = SelectedRole.Id_role; // On sauvegarde l'ID (int)

                    if (!string.IsNullOrWhiteSpace(Password))
                        u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
                }
            }
            await ctx.SaveChangesAsync();
            IsFormOpen = false;
            await ChargerDonnees();
        }
        catch (Exception ex) { Error = ex.Message; }
    }

    private async Task DeleteUser(User u)
    {
        if (u.Id == 1) return;
        if (u.Id == _state.CurrentUser.Id) return;

        try { using var ctx = new FleetDbContext(); ctx.Users.Remove(u); await ctx.SaveChangesAsync(); Users.Remove(u); }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
    }
}