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

namespace FleetManager.ViewModels;

public class ModificationVehiculeViewModel : BaseViewModel
{
    private readonly NavigationService _nav;
    private readonly AppState _state;

    // --- 1. GESTION DU FORMULAIRE (AJOUT / MODIF) ---
    private int _currentId = 0; 
    private bool _isFormOpen;
    private string _error = "";

    // Champs textes
    private string _imatricule = "";
    private string _marque = "";
    private string _modele = "";
    private string _annee = "";
    private string _kilometrage = "";

    public string Imatricule  { get => _imatricule;  set { _imatricule  = value; OnPropertyChanged(); } }
    public string Marque      { get => _marque;      set { _marque      = value; OnPropertyChanged(); } }
    public string Modele      { get => _modele;      set { _modele      = value; OnPropertyChanged(); } }
    public string Annee       { get => _annee;       set { _annee       = value; OnPropertyChanged(); } }
    public string Kilometrage { get => _kilometrage; set { _kilometrage = value; OnPropertyChanged(); } }
    public string Error       { get => _error;       set { _error       = value; OnPropertyChanged(); } }

    public bool IsFormOpen
    {
        get => _isFormOpen;
        set { _isFormOpen = value; OnPropertyChanged(); }
    }

    // --- 2. GESTION DU POPUP SUPPRESSION (NOUVEAU 🔥) ---
    private bool _isDeleteDialogOpen;
    private Vehicule? _vehiculeASupprimer; // Stocke temporairement le véhicule

    public bool IsDeleteDialogOpen
    {
        get => _isDeleteDialogOpen;
        set { _isDeleteDialogOpen = value; OnPropertyChanged(); }
    }

    // --- LISTE DES VÉHICULES ---
    public ObservableCollection<Vehicule> MesVehicules { get; set; }

    // --- COMMANDES ---
    public ICommand GoToHome { get; }
    
    // Commandes Formulaire Ajout/Modif
    public ICommand OpenAddFormCommand { get; }    // Ouvre pour ajouter
    public ICommand OpenEditFormCommand { get; }   // Ouvre pour modifier
    public ICommand CloseFormCommand { get; }      // Ferme le form
    public ICommand SubmitFormCommand { get; }     // Valide (Save)

    // Commandes Suppression (Sécurisée)
    public ICommand AskDeleteCommand { get; }      // 1. Demande "Etes-vous sûr ?"
    public ICommand ConfirmDeleteCommand { get; }  // 2. "Oui, supprimer"
    public ICommand CancelDeleteCommand { get; }   // 3. "Non, annuler"

    // Commande Suivi
    public ICommand OpenSuiviCommand { get; }      // Ouvre le formulaire de suivi
    

    public ModificationVehiculeViewModel(NavigationService nav, AppState state)
    {
        _nav = nav;
        _state = state;
        MesVehicules = new ObservableCollection<Vehicule>();

        // Navigation simple
        GoToHome = new RelayCommand(() => _nav.GoToHome());

        // Gestion Formulaire Véhicule
        OpenAddFormCommand  = new RelayCommand(PrepareAjout);
        OpenEditFormCommand = new RelayCommand<object>(PrepareModification);
        CloseFormCommand    = new RelayCommand(() => IsFormOpen = false);
        SubmitFormCommand   = new RelayCommand(async () => await SaveForm());

        // Gestion Suppression (En 2 étapes)
        AskDeleteCommand     = new RelayCommand<object>(DemanderSuppression);
        ConfirmDeleteCommand = new RelayCommand(async () => await ValiderSuppression());
        CancelDeleteCommand  = new RelayCommand(() => IsDeleteDialogOpen = false);

        // Gestion Suivi
        OpenSuiviCommand = new RelayCommand<object>(AllerVersSuivi);

        // Chargement initial
        _ = ChargerVehicules();
    }

    // ---------------------------------------------------------
    // 🚗 MÉTHODES CHARGEMENT
    // ---------------------------------------------------------
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

    // ---------------------------------------------------------
    // ➕ / ✏️ GESTION AJOUT ET MODIFICATION
    // ---------------------------------------------------------
    private void PrepareAjout()
    {
        _currentId = 0; // Nouveau
        Imatricule = ""; Marque = ""; Modele = ""; Annee = ""; Kilometrage = "";
        Error = "";
        IsFormOpen = true;
    }

    // Cette méthode est appelée quand tu cliques sur le bouton gris du menu
    private void PrepareModification(object param)
    {
        // On vérifie qu'on a bien reçu un véhicule
        if (param is Vehicule v)
        {
            // 1. On stocke l'ID pour savoir qu'on modifie (pas un ajout)
            _currentId = v.Id; 
        
            // 2. On remplit les champs du formulaire avec les infos du véhicule
            Imatricule = v.Imatricule;
            Marque = v.Marque;
            Modele = v.Modele;
            Annee = v.Annee.ToString();
            Kilometrage = v.Kilometrage.ToString();
        
            // 3. On ouvre le formulaire
            Error = "";
            IsFormOpen = true;
        }
    }

    private async Task SaveForm()
    {
        Error = "";

        // Validations de base
        if (string.IsNullOrWhiteSpace(Imatricule) || string.IsNullOrWhiteSpace(Marque) ||
            string.IsNullOrWhiteSpace(Modele) || string.IsNullOrWhiteSpace(Annee) ||
            string.IsNullOrWhiteSpace(Kilometrage))
        {
            Error = "Tous les champs sont obligatoires.";
            return;
        }
        if (!int.TryParse(Annee, out var anneeInt) || anneeInt <= 0) { Error = "Année invalide."; return; }
        if (!int.TryParse(Kilometrage, out var kmInt) || kmInt < 0) { Error = "Kilométrage invalide."; return; }
        if (ContientInjection(Imatricule) || ContientInjection(Marque) || ContientInjection(Modele))
        {
            Error = "Caractères interdits.";
            return;
        }

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
                var v = new Vehicule { Imatricule = imatriculeNorm, Marque = marqueNorm, Modele = modeleNorm, Annee = anneeInt, Kilometrage = kmInt, Status = "en boutique" };
                ctx.Vehicules.Add(v);
            }
            else // MODIFICATION
            {
                var v = await ctx.Vehicules.FindAsync(_currentId);
                if (v != null)
                {
                    v.Imatricule = imatriculeNorm; v.Marque = marqueNorm; v.Modele = modeleNorm; v.Annee = anneeInt; v.Kilometrage = kmInt;
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

    // ---------------------------------------------------------
    // 🗑️ GESTION SUPPRESSION (SÉCURISÉE)
    // ---------------------------------------------------------
    
    // Étape 1 : On clique sur la poubelle
    private void DemanderSuppression(object param)
    {
        if (param is Vehicule v)
        {
            _vehiculeASupprimer = v; // On le garde en mémoire
            IsDeleteDialogOpen = true; // On ouvre le popup "Êtes-vous sûr ?"
        }
    }

    // Étape 2 : On clique sur "OUI" dans le popup
    private async Task ValiderSuppression()
    {
        if (_vehiculeASupprimer != null)
        {
            try
            {
                using var ctx = new FleetDbContext();
                // Le Attach n'est pas obligatoire si on utilise Remove directement avec l'objet correct,
                // mais par sécurité on peut faire Remove direct.
                ctx.Vehicules.Remove(_vehiculeASupprimer);
                await ctx.SaveChangesAsync();

                MesVehicules.Remove(_vehiculeASupprimer); // Mise à jour visuelle
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur suppression : " + ex.Message);
                // Tu pourrais afficher l'erreur dans une variable ErrorDelete si tu veux
            }
            finally
            {
                // Quoi qu'il arrive, on ferme le popup et on vide la mémoire
                IsDeleteDialogOpen = false;
                _vehiculeASupprimer = null;
            }
        }
    }

    // ---------------------------------------------------------
    // 📍 GESTION SUIVI (NAVIGATION)
    // ---------------------------------------------------------
    private void AllerVersSuivi(object param)
    {
        if (param is Vehicule v)
        {
            // Ici, on passe le véhicule sélectionné à l'état global (AppState)
            // pour que la page "AjoutSuivi" sache quel véhicule est concerné.
            // (Supposons que tu aies ajouté une propriété 'SelectedVehiculeForSuivi' dans AppState)
            
            // _state.CurrentVehiculeSuivi = v; // Exemple
            
            Console.WriteLine($"Navigation vers suivi pour : {v.Marque} {v.Modele}");
            
            // Navigation vers la page de suivi
            // _nav.GoToAjoutSuivi(); // À adapter selon ta méthode de navigation
        }
    }

    // Helpers
    private bool ContientInjection(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        return Regex.IsMatch(input.ToLower(), @"(<script|</script>|--|'|""|;|/\*|\*/|drop|delete|insert|update|select|create|alter|union)");
    }
    private string Nettoyer(string input) => string.IsNullOrWhiteSpace(input) ? "" : input.Trim().Replace("<", "").Replace(">", "").Replace("'", "").Replace("\"", "");
}