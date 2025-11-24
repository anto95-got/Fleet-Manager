using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManager.Services;
using FleetManager.Data;
using FleetManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace FleetManager.ViewModels;

public class ModificationVehiculeViewModel : BaseViewModel
{
    private readonly NavigationService _nav;
    private readonly AppState _state;

    // --- CHAMPS ---
    private int _currentId = 0; 
    private bool _isFormOpen;
    private string _error = "";

    // Champs du formulaire
    private string _imatricule = "";
    private string _marque = "";
    private string _modele = "";
    private string _annee = "";
    private string _kilometrage = "";
    private string _statut = "";
    private string _capacite = "50";
    
    // Modification : On utilise string pour faciliter la saisie et la validation
    private string _prixVehicule = "0"; 

    // --- PROPRIÉTÉS ---
    public string Imatricule  { get => _imatricule;  set { _imatricule  = value; OnPropertyChanged(); } }
    public string Marque      { get => _marque;      set { _marque      = value; OnPropertyChanged(); } }
    public string Modele      { get => _modele;      set { _modele      = value; OnPropertyChanged(); } }
    public string Annee       { get => _annee;       set { _annee       = value; OnPropertyChanged(); } }
    public string Kilometrage { get => _kilometrage; set { _kilometrage = value; OnPropertyChanged(); } }
    public string Capacite    { get => _capacite;    set { _capacite    = value; OnPropertyChanged(); } }
    
    public string PrixVehicule { get => _prixVehicule; set { _prixVehicule = value; OnPropertyChanged(); } }

    public string Status      { get => _statut;      set { _statut      = value; OnPropertyChanged(); } }
    public string Error       { get => _error;       set { _error       = value; OnPropertyChanged(); } }

    public bool IsFormOpen
    {
        get => _isFormOpen;
        set { _isFormOpen = value; OnPropertyChanged(); }
    }

    // Popup Suppression
    private bool _isDeleteDialogOpen;
    private Vehicule? _vehiculeASupprimer;

    public bool IsDeleteDialogOpen
    {
        get => _isDeleteDialogOpen;
        set { _isDeleteDialogOpen = value; OnPropertyChanged(); }
    }

    // Liste des véhicules
    public ObservableCollection<Vehicule> MesVehicules { get; set; } = new();

    // --- LISTE SUIVIS (POPUP) ---
    private bool _isSuiviListOpen;
    public bool IsSuiviListOpen
    {
        get => _isSuiviListOpen;
        set { _isSuiviListOpen = value; OnPropertyChanged(); }
    }
    public ObservableCollection<Suivi> ListeSuivisDuVehicule { get; set; } = new();


    // --- COMMANDES ---
    public ICommand GoToHome { get; }
    
    public ICommand OpenAddFormCommand { get; }
    public ICommand OpenEditFormCommand { get; }
    public ICommand CloseFormCommand { get; }
    public ICommand SubmitFormCommand { get; }

    public ICommand AskDeleteCommand { get; }
    public ICommand ConfirmDeleteCommand { get; }
    public ICommand CancelDeleteCommand { get; }

    public ICommand OpenSuiviListCommand { get; }
    public ICommand CloseSuiviListCommand { get; }
    public ICommand GoToHistoriqueDetailCommand { get; }


    public ModificationVehiculeViewModel(NavigationService nav, AppState state)
    {
        _nav = nav;
        _state = state;

        GoToHome = new RelayCommand(() => _nav.GoToHome());

        OpenAddFormCommand  = new RelayCommand(PrepareAjout);
        OpenEditFormCommand = new RelayCommand<object>(PrepareModification);
        CloseFormCommand    = new RelayCommand(() => IsFormOpen = false);
        SubmitFormCommand   = new RelayCommand(async () => await SaveForm());

        AskDeleteCommand     = new RelayCommand<object>(DemanderSuppression);
        ConfirmDeleteCommand = new RelayCommand(async () => await ValiderSuppression());
        CancelDeleteCommand  = new RelayCommand(() => IsDeleteDialogOpen = false);

        OpenSuiviListCommand = new RelayCommand<Vehicule>(async (v) => await ChargerEtOuvrirSuivis(v));
        CloseSuiviListCommand = new RelayCommand(() => IsSuiviListOpen = false);
        GoToHistoriqueDetailCommand = new RelayCommand<Suivi>(NaviguerVersEditionSuivi);

        _ = ChargerVehicules();
    }

    // --- LOGIQUE POPUP SUIVI ---
    private async Task ChargerEtOuvrirSuivis(Vehicule v)
    {
        if (v == null) return;
        try
        {
            using var ctx = new FleetDbContext();
            var suivis = await ctx.Suivis.Where(s => s.VehiculeId == v.Id).OrderByDescending(s => s.DateSuivi).ToListAsync();
            ListeSuivisDuVehicule.Clear();
            foreach (var s in suivis) { s.Vehicule = v; ListeSuivisDuVehicule.Add(s); }
            IsSuiviListOpen = true;
        }
        catch (Exception ex) { Error = "Erreur suivi: " + ex.Message; }
    }

    private void NaviguerVersEditionSuivi(Suivi s)
    {
        if (s != null)
        {
            IsSuiviListOpen = false;
            // True car on vient de la page Véhicule
            _nav.GoToHistoriqueDetail(s, true); 
        }
    }

    // --- LOGIQUE VEHICULE ---

    private async Task ChargerVehicules()
    {
        try
        {
            using var ctx = new FleetDbContext();
            var list = await ctx.Vehicules.ToListAsync();
            MesVehicules.Clear();
            foreach (var v in list) MesVehicules.Add(v);
        }
        catch (Exception ex)
        {
            Error = "Erreur chargement : " + ex.Message;
        }
    }

    private void PrepareAjout()
    {
        _currentId = 0; 
        Imatricule = ""; Marque = ""; Modele = ""; Annee = ""; Kilometrage = "";
        Capacite = "50"; 
        PrixVehicule = "0"; // Reset
        Error = "";
        IsFormOpen = true;
    }

    private void PrepareModification(object param)
    {
        if (param is Vehicule v)
        {
            _currentId = v.Id; 
            Imatricule = v.Imatricule;
            Marque = v.Marque;
            Modele = v.Modele;
            Annee = v.Annee.ToString();
            Kilometrage = v.Kilometrage.ToString();
            Capacite = v.Capacite.ToString();
            PrixVehicule = v.PrixVehicule.ToString("F0"); // Charge le prix (sans décimale inutile si c'est 20000)
            
            Error = "";
            IsFormOpen = true;
        }
    }

    private async Task SaveForm()
    {
        Error = "";

        if (string.IsNullOrWhiteSpace(Imatricule) || string.IsNullOrWhiteSpace(Marque) ||
            string.IsNullOrWhiteSpace(Modele) || string.IsNullOrWhiteSpace(Annee) ||
            string.IsNullOrWhiteSpace(Kilometrage) || string.IsNullOrWhiteSpace(Capacite) || 
            string.IsNullOrWhiteSpace(PrixVehicule))
        {
            Error = "Tous les champs sont obligatoires.";
            return;
        }

        // Conversions et Validations
        if (!int.TryParse(Annee, out var anneeInt) || anneeInt <= 0) { Error = "Année invalide."; return; }
        if (!int.TryParse(Kilometrage, out var kmInt) || kmInt < 0) { Error = "Kilométrage invalide."; return; }
        if (!int.TryParse(Capacite, out var capaInt) || capaInt <= 0) { Error = "Capacité invalide."; return; }

        // Validation du PRIX (Ne doit pas être négatif)
        if (!decimal.TryParse(PrixVehicule, out var prixDec) || prixDec < 0) 
        { 
            Error = "Le prix du véhicule ne peut pas être négatif."; 
            return; 
        }

        if (ContientInjection(Imatricule) || ContientInjection(Marque) || ContientInjection(Modele)) { Error = "Caractères interdits."; return; }

        string imatriculeNorm = Nettoyer(Imatricule).ToLower();
        string marqueNorm = Nettoyer(Marque);
        string modeleNorm = Nettoyer(Modele);

        try
        {
            using var ctx = new FleetDbContext();

            if (_currentId == 0) // AJOUT
            {
                if (await ctx.Vehicules.AnyAsync(v => v.Imatricule == imatriculeNorm))
                {
                    Error = "Ce véhicule existe déjà."; return;
                }
                var v = new Vehicule { 
                    Imatricule = imatriculeNorm, 
                    Marque = marqueNorm, 
                    Modele = modeleNorm, 
                    Annee = anneeInt, 
                    Kilometrage = kmInt, 
                    Capacite = capaInt,
                    PrixVehicule = prixDec, // Insertion du prix validé
                    Status = "en magasin" 
                };
                ctx.Vehicules.Add(v);
            }
            else // MODIFICATION
            {
                var v = await ctx.Vehicules.FindAsync(_currentId);
                if (v != null)
                {
                    v.Imatricule = imatriculeNorm; 
                    v.Marque = marqueNorm; 
                    v.Modele = modeleNorm; 
                    v.Annee = anneeInt; 
                    v.Kilometrage = kmInt;
                    v.Capacite = capaInt;
                    v.PrixVehicule = prixDec; // Mise à jour du prix
                }
            }
            await ctx.SaveChangesAsync();
            IsFormOpen = false;
            await ChargerVehicules();
        }
        catch (Exception ex)
        {
            Error = "Erreur BDD : " + (ex.InnerException?.Message ?? ex.Message);
        }
    }

    // ... (Le reste suppression, helpers et converters reste inchangé) ...

    private void DemanderSuppression(object param)
    {
        if (param is Vehicule v) { _vehiculeASupprimer = v; IsDeleteDialogOpen = true; }
    }

    private async Task ValiderSuppression()
    {
        if (_vehiculeASupprimer != null)
        {
            try {
                using var ctx = new FleetDbContext();
                ctx.Vehicules.Remove(_vehiculeASupprimer);
                await ctx.SaveChangesAsync();
                MesVehicules.Remove(_vehiculeASupprimer);
            }
            catch (Exception ex) { Console.WriteLine("Erreur suppression : " + ex.Message); }
            finally { IsDeleteDialogOpen = false; _vehiculeASupprimer = null; }
        }
    }

    private bool ContientInjection(string input) => !string.IsNullOrWhiteSpace(input) && Regex.IsMatch(input.ToLower(), @"(<script|</script>|--|'|""|;|/\*|\*/|drop|delete|insert|update|select|create|alter|union)");
    private string Nettoyer(string input) => string.IsNullOrWhiteSpace(input) ? "" : input.Trim().Replace("<", "").Replace(">", "").Replace("'", "").Replace("\"", "");
}

// ... Converters inchangés ...
public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string s = (value as string)?.ToLower().Trim() ?? "";
        if (s == "en location") return SolidColorBrush.Parse("#FF4444");
        if (s == "panne") return SolidColorBrush.Parse("Orange");
        return SolidColorBrush.Parse("#4CAF50"); 
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class KiloFormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int valInt) return valInt.ToString("N0", new CultureInfo("fr-FR"));
        return value;
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}