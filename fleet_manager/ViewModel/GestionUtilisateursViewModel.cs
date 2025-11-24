using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManager.Models;
using FleetManager.Services; // Ton EmailService est ici
using FleetManager.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.ViewModels;

public class GestionUtilisateursViewModel : BaseViewModel
{
    private readonly NavigationService _nav;
    private readonly AppState _state;

    // --- CHAMPS ---
    private int _currentUserId = 0;
    private bool _isFormOpen;
    private string _error = "";

    // Champs du formulaire
    private string _nom = "";
    private string _prenom = "";
    private string _email = "";
    private string _password = "";
    private Role? _selectedRole;

    // --- PROPRIÉTÉS ---
    public string Nom { get => _nom; set { _nom = value; OnPropertyChanged(); } }
    public string Prenom { get => _prenom; set { _prenom = value; OnPropertyChanged(); } }
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
    public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }
    public Role? SelectedRole { get => _selectedRole; set { _selectedRole = value; OnPropertyChanged(); } }
    public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }

    public bool IsFormOpen
    {
        get => _isFormOpen;
        set { _isFormOpen = value; OnPropertyChanged(); }
    }

    // Permission : SuperAdmin si Role == 3
    public bool IsSuperAdmin => _state.CurrentUser?.Role == 3;

    // Popup Suppression
    private bool _isDeleteDialogOpen;
    private User? _userASupprimer;

    public bool IsDeleteDialogOpen
    {
        get => _isDeleteDialogOpen;
        set { _isDeleteDialogOpen = value; OnPropertyChanged(); }
    }

    // Listes
    public ObservableCollection<User> Users { get; } = new();
    public ObservableCollection<Role> Roles { get; } = new();

    // --- COMMANDES ---
    public ICommand GoToHome { get; }
    public ICommand Logout { get; }

    public ICommand OpenAddFormCommand { get; }
    public ICommand OpenEditFormCommand { get; }
    public ICommand CloseFormCommand { get; }
    public ICommand SubmitFormCommand { get; }

    public ICommand AskDeleteCommand { get; }
    public ICommand ConfirmDeleteCommand { get; }
    public ICommand CancelDeleteCommand { get; }

    public GestionUtilisateursViewModel(NavigationService nav, AppState state)
    {
        _nav = nav;
        _state = state;

        GoToHome = new RelayCommand(() => _nav.GoToHome());
        Logout = new RelayCommand(() => _nav.Logout());

        OpenAddFormCommand = new RelayCommand(PrepareAjout);
        OpenEditFormCommand = new RelayCommand<User?>(PrepareModification);
        CloseFormCommand = new RelayCommand(() => IsFormOpen = false);
        SubmitFormCommand = new RelayCommand(async () => await SaveForm());

        AskDeleteCommand = new RelayCommand<User?>(DemanderSuppression);
        ConfirmDeleteCommand = new RelayCommand(async () => await ValiderSuppression());
        CancelDeleteCommand = new RelayCommand(() => IsDeleteDialogOpen = false);

        _ = ChargerDonnees();
    }
    
    
    
    // Est-ce qu'on est en train de modifier un utilisateur existant ?
// (Si _currentUserId est 0, c'est un ajout).
    public bool IsEditingExistingUser => _currentUserId != 0;

    // Le champ mot de passe n'est visible que si :
    // 1. C'est le SuperAdmin
    // 2. ET on est en train de MODIFIER (pas ajouter)
    public bool ShowPasswordField => IsSuperAdmin && IsEditingExistingUser;

    private async Task ChargerDonnees()
    {
        try
        {
            await using var ctx = new FleetDbContext();

            // 1. Rôles
            var rolesDb = await ctx.Roles.ToListAsync();
            Roles.Clear();
            foreach (var r in rolesDb) Roles.Add(r);

            // 2. Utilisateurs
            var usersDb = await ctx.Users.Include(u => u.RoleInfo).ToListAsync();
            Users.Clear();
            
            if (usersDb.Count == 0) Error = "Aucun utilisateur trouvé.";

            foreach (var u in usersDb) Users.Add(u);
        }
        catch (Exception ex)
        {
            Error = "Erreur chargement : " + ex.Message;
        }
    }

    private void PrepareAjout()
    {
        _currentUserId = 0;
        Nom = ""; Prenom = ""; Email = ""; Password = "";
        // Sélectionne Role ID 1 (Utilisateur) par défaut
        SelectedRole = Roles.FirstOrDefault(r => r.Id_role == 1);
        Error = "";
        IsFormOpen = true;
        OnPropertyChanged(nameof(ShowPasswordField)); // <--- AJOUTE CECI
    }

    private void PrepareModification(User? u)
    {
        if (u == null) return;

        // SÉCURITÉ : Admin (2) ne modifie pas SuperAdmin (3)
        if (u.Role == 3 && !IsSuperAdmin) 
        {
            Error = "Vous ne pouvez pas modifier un SuperAdmin.";
            return;
        }

        _currentUserId = u.Id;
        Nom = u.Nom;
        Prenom = u.Prenom;
        Email = u.Email;
        Password = ""; 
        OnPropertyChanged(nameof(ShowPasswordField)); // <--- AJOUTE CECI
        
        // Mapping du Role
        SelectedRole = Roles.FirstOrDefault(r => r.Id_role == u.Role);

        Error = "";
        IsFormOpen = true;
    }

    private async Task SaveForm()
    {
        Error = "";

        // On ne vérifie pas le mot de passe ici car il est généré auto à l'ajout
        if (string.IsNullOrWhiteSpace(Nom) || string.IsNullOrWhiteSpace(Email) || SelectedRole == null)
        {
            Error = "Champs requis.";
            return;
        }

        try
        {
            await using var ctx = new FleetDbContext();

            // === AJOUT ===
            if (_currentUserId == 0) 
            {
                // Vérif email unique
                bool emailExiste = await ctx.Users.AnyAsync(u => u.Email == Email);
                if (emailExiste) { Error = "Cet email est déjà utilisé."; return; }

                // 1. Génération du mot de passe aléatoire
                string passwordClair = GenererMotDePasse(10);
                
                // 2. Envoi de l'email via ta classe Statique
                try 
                {
                    // 👇 C'EST ICI LE CHANGEMENT : Appel direct à la classe statique
                    EmailService.EnvoyerIdentifiants(Email, Nom, passwordClair);
                }
                catch (Exception exMail)
                {
                    Error = exMail.Message; // Le message d'erreur vient déjà de ton service
                    return; // On annule tout si l'email ne part pas
                }

                // 3. Création User
                var newUser = new User
                {
                    Nom = Nom,
                    Prenom = Prenom,
                    Email = Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordClair),
                    Role = SelectedRole.Id_role, // J'ai gardé ta logique
                    MustChangePassword = true
                };
                ctx.Users.Add(newUser);
            }
            // === MODIFICATION ===
            else 
            {
                var u = await ctx.Users.FindAsync(_currentUserId);
                if (u != null)
                {
                    u.Nom = Nom;
                    u.Prenom = Prenom;
                    u.Email = Email;

                    // Seul SuperAdmin change le rôle
                    if (IsSuperAdmin)
                    {
                        u.Role = SelectedRole.Id_role;
                    }

                    // Reset manuel du mot de passe par l'admin
                    if (IsSuperAdmin && !string.IsNullOrWhiteSpace(Password))
                    {
                        u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
                        u.MustChangePassword = true;
                    }
                }
            }
            await ctx.SaveChangesAsync();
            IsFormOpen = false;
            await ChargerDonnees();
        }
        catch (Exception ex)
        {
            Error = "Erreur BDD : " + ex.Message;
        }
    }

    // --- SUPPRESSION ---
    private void DemanderSuppression(User? u)
    {
        if (u == null) return;
        if (!IsSuperAdmin) { Error = "Seul le SuperAdmin peut supprimer."; return; }
        if (_state.CurrentUser != null && u.Id == _state.CurrentUser.Id) { Error = "Impossible de se supprimer."; return; }
        
        if (u.Role == 3) { Error = "Impossible de supprimer un autre SuperAdmin."; return; }

        _userASupprimer = u;
        IsDeleteDialogOpen = true;
    }

    private async Task ValiderSuppression()
    {
        if (_userASupprimer != null)
        {
            try
            {
                await using var ctx = new FleetDbContext();
                ctx.Users.Remove(_userASupprimer);
                await ctx.SaveChangesAsync();
                Users.Remove(_userASupprimer);
            }
            catch (Exception ex)
            {
                Error = "Erreur suppression : " + ex.Message;
            }
            finally
            {
                IsDeleteDialogOpen = false;
                _userASupprimer = null;
            }
        }
    }

    // Générateur de mot de passe
    private string GenererMotDePasse(int longueur)
    {
        const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, longueur)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}