using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManager.Services;
using FleetManager.Data;
using FleetManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.ViewModels;

public class ModificationVehiculeViewModel : BaseViewModel
{
    private readonly NavigationService _nav;
    private readonly AppState _state;

    private string _imatricule = "";
    private string _marque = "";
    private string _modele = "";
    private int _annee ;
    private int _kilometrage ; 
    private string _statut = "";
    private string _error = "";

    public string imatricule { get => _imatricule; set { _imatricule = value; OnPropertyChanged(); } }
    public string marque { get => _marque; set { _marque = value; OnPropertyChanged(); } }
    public string modele { get => _modele; set { _modele = value; OnPropertyChanged(); } }
    public int annee { get => _annee; set { _annee = value; OnPropertyChanged(); } }
    public int kilometrage { get => _kilometrage; set { _kilometrage = value; OnPropertyChanged(); } }
    public string statut { get => _statut; set { _statut = value; OnPropertyChanged(); } }
    public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }

    private bool _isFormOpen ;
    public bool IsFormOpen
    {
        get => _isFormOpen;
        set
        {
            _isFormOpen = value;
            OnPropertyChanged();
        }
    }
    public ICommand AjouterCommand { get; }
    public ICommand GoToHome { get; }
    
    public ICommand OpenFormCommand { get; }
    public ICommand CloseFormCommand { get; }
    public ICommand SubmitFormCommand { get; }
    
    
    
    
    public ModificationVehiculeViewModel(NavigationService nav, AppState state)
    {
        _nav = nav;
        _state = state;
        
       // AjouterCommand = new Command(AjouterVehicule);
       GoToHome = new RelayCommand(() => _nav.GoToHome());
       // AjouterCommand = new RelayCommand(async () => await AjouterVehicule());
       AjouterCommand = new RelayCommand(SubmitFormCommand);
       OpenFormCommand = new RelayCommand(OpenFormCommand);
       CloseFormCommand = new RelayCommand(CloseFormCommand);
       

    }
    
    private void OpenForm()  => IsFormOpen = true;
    private void CloseForm() => IsFormOpen = false;

    private void SaveForm()
    {
        Error = "";

        if (string.IsNullOrWhiteSpace(imatricule) ||
            string.IsNullOrWhiteSpace(marque) ||
            string.IsNullOrWhiteSpace(modele) ||
            string.IsNullOrWhiteSpace())
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
            _nav.GoToHome(); 
        }
        catch (Exception ex)
        {
            Error = "Erreur : " + ex.Message;
        }
    }
    
    
}