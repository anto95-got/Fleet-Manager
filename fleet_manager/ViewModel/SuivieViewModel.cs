using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManager.Models;
using FleetManager.Services;
using FleetManager.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace FleetManager.ViewModels;

public class SuivieViewModel : BaseViewModel
{
    private readonly NavigationService _nav;
    private readonly AppState _state;

    public ObservableCollection<Suivi> MesSuivis { get; set; } = new();
    public ObservableCollection<Vehicule> ListeVehicules { get; set; } = new();

    // --- POPUPS ---
    private bool _isFormOpen;
    private bool _isCloseFormOpen;
    private bool _isFuelFormOpen;
    private bool _isDeleteDialogOpen;
    private string _error = "";

    // --- CHAMPS ---
    private DateTimeOffset? _newDate = DateTimeOffset.Now;
    private string _newKmDepart = "";
    private string _newDestination = "";
    private Vehicule? _selectedVehiculeForAdd;

    private Suivi? _suiviAction;
    private string _closeKmArrivee = "";
    private string _closeLitres = ""; 
    private string _addFuelLitres = ""; 

    // --- PROPRIÉTÉS ---
    public bool IsFormOpen { get => _isFormOpen; set { _isFormOpen = value; OnPropertyChanged(); } }
    public bool IsCloseFormOpen { get => _isCloseFormOpen; set { _isCloseFormOpen = value; OnPropertyChanged(); } }
    public bool IsFuelFormOpen { get => _isFuelFormOpen; set { _isFuelFormOpen = value; OnPropertyChanged(); } }
    public bool IsDeleteDialogOpen { get => _isDeleteDialogOpen; set { _isDeleteDialogOpen = value; OnPropertyChanged(); } }
    public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }

    public DateTimeOffset? NewDate { get => _newDate; set { _newDate = value; OnPropertyChanged(); } }
    public string NewKmDepart { get => _newKmDepart; set { _newKmDepart = value; OnPropertyChanged(); } }
    public string NewDestination { get => _newDestination; set { _newDestination = value; OnPropertyChanged(); } }
    public Vehicule? SelectedVehiculeForAdd 
    { 
        get => _selectedVehiculeForAdd; 
        set { _selectedVehiculeForAdd = value; OnPropertyChanged(); if(value!=null) NewKmDepart = value.Kilometrage.ToString(); } 
    }

    public string CloseKmArrivee { get => _closeKmArrivee; set { _closeKmArrivee = value; OnPropertyChanged(); } }
    public string CloseLitres { get => _closeLitres; set { _closeLitres = value; OnPropertyChanged(); } }
    public string AddFuelLitres { get => _addFuelLitres; set { _addFuelLitres = value; OnPropertyChanged(); } }

    // --- COMMANDES ---
    public ICommand GoToHome { get; }
    public ICommand OpenAddFormCommand { get; }
    public ICommand CloseFormCommand { get; }
    public ICommand SubmitFormCommand { get; }
    public ICommand AskDeleteCommand { get; }
    public ICommand ConfirmDeleteCommand { get; }
    public ICommand CancelDeleteCommand { get; }
    public ICommand OpenClotureFormCommand { get; }
    public ICommand SubmitClotureCommand { get; }
    public ICommand CancelClotureCommand { get; }
    public ICommand OpenFuelFormCommand { get; }
    public ICommand SubmitFuelCommand { get; }
    public ICommand CancelFuelCommand { get; }

    // 🔥 NOUVELLE COMMANDE : Navigation vers le détail
    public ICommand GoToDetailCommand { get; }

    public SuivieViewModel(NavigationService nav, AppState state)
    {
        _nav = nav;
        _state = state;
        GoToHome = new RelayCommand(() => _nav.GoToHome());

        OpenAddFormCommand = new RelayCommand(PrepareAjout);
        CloseFormCommand = new RelayCommand(() => IsFormOpen = false);
        SubmitFormCommand = new RelayCommand(async () => await SaveSuivi());

        OpenClotureFormCommand = new RelayCommand<Suivi>(PrepareCloture);
        SubmitClotureCommand = new RelayCommand(async () => await ValiderCloture());
        CancelClotureCommand = new RelayCommand(() => IsCloseFormOpen = false);

        OpenFuelFormCommand = new RelayCommand<Suivi>(PrepareFuel);
        SubmitFuelCommand = new RelayCommand(async () => await SaveFuel());
        CancelFuelCommand = new RelayCommand(() => IsFuelFormOpen = false);

        AskDeleteCommand = new RelayCommand<Suivi>(DemanderSuppression);
        ConfirmDeleteCommand = new RelayCommand(async () => await ValiderSuppression());
        CancelDeleteCommand = new RelayCommand(() => IsDeleteDialogOpen = false);

        // 🔥 Initialisation de la commande de navigation
        GoToDetailCommand = new RelayCommand<Suivi>(s => _nav.GoToHistoriqueDetail(s));

        _ = ChargerDonnees();
    }

    private async Task ChargerDonnees()
    {
        try {
            using var ctx = new FleetDbContext();
            var suivis = await ctx.Suivis.Include(s => s.Vehicule).Include(s => s.Pleins).OrderByDescending(s => s.DateSuivi).ToListAsync();
            MesSuivis.Clear();
            foreach (var s in suivis) MesSuivis.Add(s);

            var vehicules = await ctx.Vehicules.ToListAsync();
            ListeVehicules.Clear();
            foreach (var v in vehicules) ListeVehicules.Add(v);
        } catch (Exception ex) { Error = ex.Message; }
    }

    private void PrepareAjout() { Error = ""; NewDate = DateTimeOffset.Now; NewKmDepart = ""; NewDestination = ""; SelectedVehiculeForAdd = null; IsFormOpen = true; }
    
    private async Task SaveSuivi() {
        Error = ""; if (SelectedVehiculeForAdd == null || string.IsNullOrWhiteSpace(NewKmDepart)) { Error = "Infos manquantes"; return; }
        if (!int.TryParse(NewKmDepart, out int km)) { Error = "Km invalide"; return; }
        try {
            using var ctx = new FleetDbContext();
            var s = new Suivi { DateSuivi = NewDate?.DateTime ?? DateTime.Now, KmDepart = km, Destination = NewDestination, VehiculeId = SelectedVehiculeForAdd.Id, UserId = _state.CurrentUser?.Id ?? 1, Status = true };
            ctx.Suivis.Add(s); await ctx.SaveChangesAsync(); IsFormOpen = false; await ChargerDonnees();
        } catch (Exception ex) { Error = ex.Message; }
    }

    private void PrepareCloture(Suivi s) { _suiviAction = s; CloseKmArrivee = ""; CloseLitres = ""; Error = ""; IsCloseFormOpen = true; }
    
    private async Task ValiderCloture() {
        Error = ""; if (_suiviAction == null) return;
        if (!int.TryParse(CloseKmArrivee, out int kmArr)) { Error = "Km invalide"; return; }
        if (kmArr < _suiviAction.KmDepart) { Error = "Km arrivée < Km départ"; return; }

        try {
            using var ctx = new FleetDbContext();
            var dbS = await ctx.Suivis.Include(s => s.Vehicule).FirstOrDefaultAsync(x => x.Id == _suiviAction.Id);
            if (dbS != null) {
                dbS.KmArrivee = kmArr;
                dbS.Status = false; 

                if (decimal.TryParse(CloseLitres, out decimal litres) && litres > 0)
                {
                    var plein = new PleinCarburant { 
                        DatePlein = DateTime.Now, 
                        Litres = litres, 
                        VehiculeId = dbS.VehiculeId, 
                        SuiviId = dbS.Id 
                    };
                    ctx.PleinsCarburants.Add(plein);
                }
                await ctx.SaveChangesAsync();
            }
            IsCloseFormOpen = false;
            await ChargerDonnees();
        } catch (Exception ex) { Error = ex.Message; }
    }

    private void PrepareFuel(Suivi s) { _suiviAction = s; AddFuelLitres = ""; Error = ""; IsFuelFormOpen = true; }
    
    private async Task SaveFuel() {
        Error = ""; if (_suiviAction == null) return;
        if (!decimal.TryParse(AddFuelLitres, out decimal lit) || lit <= 0) { Error = "Litres invalides"; return; }
        try {
            using var ctx = new FleetDbContext();
            var plein = new PleinCarburant { DatePlein = DateTime.Now, Litres = lit, VehiculeId = _suiviAction.VehiculeId, SuiviId = _suiviAction.Id };
            ctx.PleinsCarburants.Add(plein);
            await ctx.SaveChangesAsync();
            IsFuelFormOpen = false;
            await ChargerDonnees();
        } catch (Exception ex) { Error = ex.Message; }
    }

    private void DemanderSuppression(Suivi s) { _suiviAction = s; IsDeleteDialogOpen = true; }
    private async Task ValiderSuppression() {
        if (_suiviAction == null) return;
        try { using var ctx = new FleetDbContext(); ctx.Suivis.Remove(_suiviAction); await ctx.SaveChangesAsync(); MesSuivis.Remove(_suiviAction); IsDeleteDialogOpen = false; } catch (Exception ex) { Error = ex.Message; }
    }
}

// CONVERTISSEURS (Standards)
public class SuiviStatusColorConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => (value is bool b && b) ? SolidColorBrush.Parse("#4CAF50") : SolidColorBrush.Parse("#9E9E9E");
    public object? ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotImplementedException();
}
public class SuiviStatusTextConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value is bool b && b) ? "EN COURS" : "TERMINÉ";
    public object? ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotImplementedException();
}