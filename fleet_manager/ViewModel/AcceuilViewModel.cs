using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManager.Models;
using FleetManager.Services;
using FleetManager.Data;
using Microsoft.EntityFrameworkCore;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FleetManager.ViewModels;

public class AcceuilViewModel : BaseViewModel
{
    private readonly NavigationService _nav;
    private readonly AppState _state;
    private readonly FleetDbContext _db;

    public bool IsAdmin => _state.CurrentUser?.Role is 2 or 3 ; 
    public string WelcomeText => _state.CurrentUser == null ? "Bienvenue" : $"Bienvenue {_state.CurrentUser.Prenom} {_state.CurrentUser.Nom}";

    // --- FILTRES ---
    public List<string> FilterOptions { get; } = new() { "Tout", "Cette Année", "Ce Mois", "Cette Semaine" };
    
    private string _selectedFilter = "Tout";
    public string SelectedFilter
    {
        get => _selectedFilter;
        set { _selectedFilter = value; OnPropertyChanged(); _ = ReloadDataWithFilter(); }
    }

    // --- KPIs ---
    private string _nombreDeVehicules = "0";
    public string NombreDeVehicules { get => _nombreDeVehicules; set { _nombreDeVehicules = value; OnPropertyChanged(); } }

    private string _nombreDeSuivis = "0";
    public string NombreDeSuivis { get => _nombreDeSuivis; set { _nombreDeSuivis = value; OnPropertyChanged(); } }

    // --- GRAPHIQUES EXISTANTS ---
    public ISeries[] StatusSeries { get; set; } = { };
    public ISeries[] ActivitySeries { get; set; } = { };
    public Axis[] ActivityXAxes { get; set; } = { };

    // 🔥 NOUVEAU GRAPHIQUE : DÉPENSES ---
    public ISeries[] ExpensesSeries { get; set; } = { };
    public Axis[] ExpensesXAxes { get; set; } = { };

    // --- COMMANDES ---
    public ICommand LogoutCommand { get; }
    public ICommand GoVehiculesCommand { get; }
    public ICommand GoToHome { get; }
    public ICommand GoSuivieCommand { get; }
    public ICommand GoToRegisterCommand { get; }
    public ICommand GoToProfilCommand { get; }
    public ICommand GoToAdminCommand { get; }

    public AcceuilViewModel(NavigationService nav, AppState state, FleetDbContext db)
    {
        _nav = nav;
        _state = state;
        _db = db;

        LogoutCommand = new RelayCommand(async () => { _state.CurrentUser = null; _nav.GoToLogin(); });
        GoVehiculesCommand = new RelayCommand(() => _nav.GoToVehicules());
        GoToHome = new RelayCommand(() => _nav.GoToHome());
        GoSuivieCommand = new RelayCommand(() => _nav.GoToSuivie());
        GoToRegisterCommand = new RelayCommand(() => _nav.GoToRegister());
        GoToProfilCommand = new RelayCommand(() => _nav.GoToProfil());
        GoToAdminCommand = new RelayCommand(() => _nav.GoToAdmin());

        _ = ReloadDataWithFilter();
    }

    private async Task ReloadDataWithFilter()
    {
        try
        {
            // 1. Filtre de Date
            DateTime? dateDebut = null;
            DateTime now = DateTime.Now;

            switch (SelectedFilter)
            {
                case "Cette Année": dateDebut = new DateTime(now.Year, 1, 1); break;
                case "Ce Mois": dateDebut = new DateTime(now.Year, now.Month, 1); break;
                case "Cette Semaine": dateDebut = now.AddDays(-7); break;
                default: dateDebut = null; break; 
            }

            // 2. KPIs
            var nbVehicules = await _db.Vehicules.CountAsync();
            NombreDeVehicules = nbVehicules.ToString();

            var querySuivis = _db.Suivis.AsQueryable();
            if (dateDebut.HasValue) querySuivis = querySuivis.Where(s => s.DateSuivi >= dateDebut.Value);
            
            var nbSuivis = await querySuivis.CountAsync();
            NombreDeSuivis = nbSuivis.ToString();

            // 3. GRAPHIQUES EXISTANTS
            
            // A. Camembert
            int nbEnMagasin = await _db.Vehicules.CountAsync(v => v.Status == "en magasin");
            int nbEnLocation = await _db.Vehicules.CountAsync(v => v.Status == "en location");

            StatusSeries = new ISeries[]
            {
                new PieSeries<int> { Values = new[] { nbEnMagasin }, Name = "En Magasin", Fill = new SolidColorPaint(SKColors.MediumSeaGreen) },
                new PieSeries<int> { Values = new[] { nbEnLocation }, Name = "En Location", Fill = new SolidColorPaint(SKColors.OrangeRed) }
            };
            OnPropertyChanged(nameof(StatusSeries));

            // B. Barres Activité (Km)
            var queryGraph = _db.Suivis.Where(s => s.Status == false && s.KmArrivee != null);
            if (dateDebut.HasValue) queryGraph = queryGraph.Where(s => s.DateSuivi >= dateDebut.Value);

            var activityData = await queryGraph
                .OrderByDescending(s => s.DateSuivi)
                .Take(10)
                .Select(s => new { s.DateSuivi, Distance = s.KmArrivee - s.KmDepart })
                .ToListAsync();
            
            activityData.Reverse();

            if (activityData.Any())
            {
                ActivitySeries = new ISeries[]
                {
                    new ColumnSeries<int> { Values = activityData.Select(x => (int)x.Distance).ToArray(), Name = "Km", Fill = new SolidColorPaint(SKColors.CornflowerBlue) }
                };
                ActivityXAxes = new Axis[] { new Axis { Labels = activityData.Select(x => x.DateSuivi.ToString("dd/MM")).ToArray() } };
            }
            else
            {
                ActivitySeries = new ISeries[] { };
                ActivityXAxes = new Axis[] { new Axis { Labels = new[] { "-" } } };
            }
            OnPropertyChanged(nameof(ActivitySeries));
            OnPropertyChanged(nameof(ActivityXAxes));

            // 🔥 4. NOUVEAU GRAPHIQUE : DÉPENSES (Argent)
            // On interroge la base pour avoir la somme des prix (calculés par tes triggers)
            
            // a. Coût des trajets (Table Suivie, colonne Prix)
            var queryCoutTrajets = _db.Suivis.AsQueryable();
            if (dateDebut.HasValue) queryCoutTrajets = queryCoutTrajets.Where(s => s.DateSuivi >= dateDebut.Value);
            // Note : on utilise (double) pour LiveCharts
            double totalCoutTrajets = (double)(await queryCoutTrajets.SumAsync(s => s.Prix));

            // b. Coût Carburant & Entretien (Table Concerner)
            var queryPleins = _db.PleinsCarburants.AsQueryable();
            if (dateDebut.HasValue) queryPleins = queryPleins.Where(p => p.DatePlein >= dateDebut.Value);
            
            double totalCarburant = (double)(await queryPleins.SumAsync(p => p.PrixPlein));
            double totalEntretien = (double)(await queryPleins.SumAsync(p => p.EntretienPrix));

            // c. Configuration du graphique
            ExpensesSeries = new ISeries[]
            {
                new ColumnSeries<double> 
                { 
                    Values = new[] { totalCoutTrajets }, 
                    Name = "Coût Location (Km)", 
                    Fill = new SolidColorPaint(SKColors.Purple) 
                },
                new ColumnSeries<double> 
                { 
                    Values = new[] { totalCarburant }, 
                    Name = "Carburant", 
                    Fill = new SolidColorPaint(SKColors.Teal) 
                },
                new ColumnSeries<double> 
                { 
                    Values = new[] { totalEntretien }, 
                    Name = "Entretien", 
                    Fill = new SolidColorPaint(SKColors.Orange) 
                }
            };

            ExpensesXAxes = new Axis[] 
            { 
                new Axis 
                { 
                    Labels = new[] { "Catégories de Dépenses" }, 
                    LabelsRotation = 0 
                } 
            };

            OnPropertyChanged(nameof(ExpensesSeries));
            OnPropertyChanged(nameof(ExpensesXAxes));

        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur Dashboard : " + ex.Message);
        }
    }
}