using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManager.Models;
using FleetManager.Services;
using FleetManager.Data;              // DbContext
using Microsoft.EntityFrameworkCore;   // CountAsync

namespace FleetManager.ViewModels
{
    public class AcceuilViewModel : BaseViewModel
    {
        private readonly NavigationService _nav;
        private readonly AppState _state;
        private readonly FleetDbContext _db;   // DbContext

        // --- Propriétés véhicule ---
        private string _imatricule = "";
        private string _marque = "";
        private string _modele = "";
        private int _annee;
        private int _kilometrage;
        private string _statut = "";
        private string _error = "";

        public string imatricule { get => _imatricule; set { _imatricule = value; OnPropertyChanged(); } }
        public string marque     { get => _marque;     set { _marque     = value; OnPropertyChanged(); } }
        public string modele     { get => _modele;     set { _modele     = value; OnPropertyChanged(); } }
        public int annee         { get => _annee;      set { _annee      = value; OnPropertyChanged(); } }
        public int kilometrage   { get => _kilometrage; set { _kilometrage = value; OnPropertyChanged(); } }
        public string statut     { get => _statut;     set { _statut     = value; OnPropertyChanged(); } }
        public string Error      { get => _error;      set { _error      = value; OnPropertyChanged(); } }

        // --- Propriétés suivi ---
        private int _Id_suivi;
        private DateTime _date_suivi;
        private int _km_depart;
        private int _km_arrivee;
        private string _destination = "";
        private string _commentaire = "";
        private string _error2 = "";

        public int Id               => _Id_suivi;
        public DateTime Date_suivi  { get => _date_suivi;  set { _date_suivi  = value; OnPropertyChanged(); } }
        public int Km_depart        { get => _km_depart;   set { _km_depart   = value; OnPropertyChanged(); } }
        public int Km_arrivee       { get => _km_arrivee;  set { _km_arrivee  = value; OnPropertyChanged(); } }
        public string Destination   { get => _destination; set { _destination = value; OnPropertyChanged(); } }
        public string Commentaire   { get => _commentaire; set { _commentaire = value; OnPropertyChanged(); } }
        public string Error2        { get => _error2;      set { _error2      = value; OnPropertyChanged(); } }

        // --- Propriété pour le nombre de véhicules ---
        private string _nombreDeVehicules = "0";
        public string NombreDeVehicules
        {
            get => _nombreDeVehicules;
            set { _nombreDeVehicules = value; OnPropertyChanged(); }
        }

        // --- Propriété pour le nombre de suivis ---
        private string _nombreDeSuivis = "0";
        public string NombreDeSuivis
        {
            get => _nombreDeSuivis;
            set { _nombreDeSuivis = value; OnPropertyChanged(); }
        }

        // Commandes
        public ICommand LogoutCommand { get; }
        public ICommand GoVehiculesCommand { get; }
        public ICommand GoToHome { get; }

        // Menu
        public ObservableCollection<MenuItem> MenuItems { get; }

        public AcceuilViewModel(NavigationService nav, AppState state, FleetDbContext db)
        {
            _nav   = nav;
            _state = state;
            _db    = db;

            // Commandes
            LogoutCommand      = new RelayCommand(LogoutAsync);
            GoVehiculesCommand = new RelayCommand(GoVehicules);
            GoToHome           = new RelayCommand(() => _nav.GoToHome());

            // Menu
            MenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem { Title = "Gestion des véhicules", Command = GoVehiculesCommand },
                new MenuItem { Title = "Déconnexion",           Command = LogoutCommand }
            };

            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            // On fait les requêtes l'une APRÈS l'autre
            await RafraichirNombreDeVehiculesAsync();
            await RafraichirNombreDeSuivieAsync();
        }

        public string WelcomeText
        {
            get
            {
                if (_state.CurrentUser is null)
                    return "Bienvenue";

                return $"Bienvenue { _state.CurrentUser.Prenom } {_state.CurrentUser.Nom}";
            }
        }

        private async Task LogoutAsync()
        {
            _state.CurrentUser = null;
            _nav.GoToLogin();
        }

        private void GoVehicules()
        {
            _nav.GoToVehicules();
        }

        // 🔁 Méthode qui va chercher le nombre de véhicules dans la BDD
        public async Task RafraichirNombreDeVehiculesAsync()
        {
            try
            {
                var count = await _db.Vehicules.CountAsync();
                NombreDeVehicules = $"{count}";
            }
            catch (Exception ex)
            {
                Error = "Erreur lors du chargement du nombre de véhicules";
                Console.WriteLine(ex);
            }
        }

        public async Task RafraichirNombreDeSuivieAsync()
        {
            try
            {
                var count = await _db.Suivis.CountAsync();
                NombreDeSuivis = $"{count}";
            }
            catch (Exception e)
            {
                Error2 = "Erreur lors du chargement du nombre des suivis";
                Console.WriteLine(e);
            }
        }
    }
}
