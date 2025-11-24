using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FleetManager.Models;
using FleetManager.Data;
using FleetManager.Services;
using Microsoft.EntityFrameworkCore;
using Avalonia.Data.Converters;
using System.Globalization;

namespace FleetManager.ViewModels;

public class HistoriqueVehiculeViewModel : BaseViewModel
{
    private readonly NavigationService _nav;
    private readonly AppState _state;
    private readonly int _suiviId;
    private int _vehiculeId; // Nécessaire pour vérifier les autres suivis du même véhicule

    // Variable pour savoir où retourner (Vehicule ou Suivi)
    private readonly bool _vientDeLaListeDesVehicules;

    // --- Champs Données ---
    private DateTimeOffset? _dateSuivi;
    private string _destination = "";
    private string _kmDepart = "";
    private string _kmArrivee = "";
    
    // --- Champs Statut & UI ---
    private bool _isOpen;
    private string _statusLabel = "Chargement...";
    private string _statusFeedback = "";
    private string _error = "";

    // --- Propriétés ---
    public DateTimeOffset? DateSuivi { get => _dateSuivi; set { _dateSuivi = value; OnPropertyChanged(); } }
    public string Destination { get => _destination; set { _destination = value; OnPropertyChanged(); } }
    public string KmDepart { get => _kmDepart; set { _kmDepart = value; OnPropertyChanged(); } }
    public string KmArrivee { get => _kmArrivee; set { _kmArrivee = value; OnPropertyChanged(); } }
    
    public string Error { get => _error; set { _error = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrEmpty(Error);

    // Propriétés pour l'affichage dynamique du statut
    public string StatusLabel { get => _statusLabel; set { _statusLabel = value; OnPropertyChanged(); } }
    public string StatusFeedback { get => _statusFeedback; set { _statusFeedback = value; OnPropertyChanged(); } }

    public bool IsOpen 
    { 
        get => _isOpen; 
        set 
        {
            // LOGIQUE DE SÉCURITÉ :
            // Si on tente d'OUVRIR (passer de false à true)
            if (value == true && !_isOpen)
            {
                if (VerifierSiAutreSuiviOuvert())
                {
                    // Si un autre est déjà ouvert, on bloque
                    Error = "⛔ Impossible d'ouvrir : Un autre trajet est déjà en cours pour ce véhicule !";
                    // On force l'UI à rester décochée (on notifie le changement sans changer la valeur)
                    OnPropertyChanged(nameof(IsOpen)); 
                    return;
                }
            }

            _isOpen = value; 
            OnPropertyChanged();
            
            // Met à jour les textes (Ouvert/Fermé et Feedback)
            MettreAJourTextesStatut();
        } 
    }

    // Champs Table Concerner (Liste)
    public ObservableCollection<PleinCarburant> ListePleins { get; set; } = new();

    // Commandes
    public ICommand GoBackCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand AddPleinCommand { get; }
    public ICommand RemovePleinCommand { get; }

    // Constructeur modifié : on ajoute le booléen pour savoir d'où on vient
    // Par défaut à true (Vehicule) pour la compatibilité
    public HistoriqueVehiculeViewModel(NavigationService nav, AppState state, Suivi suiviSelectionne, bool vientDeLaListeDesVehicules = true)
    {
        _nav = nav;
        _state = state;
        _suiviId = suiviSelectionne.Id;
        _vientDeLaListeDesVehicules = vientDeLaListeDesVehicules;

        // La commande de retour utilise maintenant la logique dynamique
        GoBackCommand = new RelayCommand(RetournerALaSource);
        
        SaveCommand = new RelayCommand(async () => await SauvegarderModifications());
        AddPleinCommand = new RelayCommand(AjouterPleinVide);
        RemovePleinCommand = new RelayCommand<PleinCarburant>(SupprimerPlein);

        _ = ChargerDonneesCompletes();
    }

    // --- LOGIQUE DE NAVIGATION DYNAMIQUE ---
    private void RetournerALaSource()
    {
        if (_vientDeLaListeDesVehicules)
        {
            // Si on vient de ModificationVehiculeView
            _nav.GoToVehicules();
        }
        else
        {
            // Si on vient de SuivieView
            _nav.GoToSuivie();
        }
    }

    private async Task ChargerDonneesCompletes()
    {
        try
        {
            using var ctx = new FleetDbContext();
            var s = await ctx.Suivis.Include(x => x.Pleins).FirstOrDefaultAsync(x => x.Id == _suiviId);

            if (s != null)
            {
                _vehiculeId = s.VehiculeId; // On sauvegarde l'ID véhicule pour la vérif
                DateSuivi = s.DateSuivi;
                Destination = s.Destination ?? "";
                KmDepart = s.KmDepart.ToString();
                KmArrivee = s.KmArrivee.HasValue ? s.KmArrivee.ToString() : "";
                
                // On set directement le field pour éviter de déclencher la vérif au chargement
                _isOpen = s.Status; 
                OnPropertyChanged(nameof(IsOpen));

                // Initialisation des textes
                StatusLabel = _isOpen ? "Statut actuel : OUVERT (En cours)" : "Statut actuel : FERMÉ (Terminé)";
                StatusFeedback = ""; 

                ListePleins.Clear();
                foreach (var p in s.Pleins) ListePleins.Add(p);
            }
        }
        catch (Exception ex) { Error = "Erreur chargement : " + ex.Message; }
    }

    // --- LOGIQUE METIER ---

    private bool VerifierSiAutreSuiviOuvert()
    {
        try 
        {
            using var ctx = new FleetDbContext();
            // Existe-t-il un autre suivi actif (Status=1) pour ce même véhicule, différent de celui-ci ?
            return ctx.Suivis.Any(s => s.VehiculeId == _vehiculeId && s.Status == true && s.Id != _suiviId);
        }
        catch 
        { 
            return false; 
        }
    }

    private void MettreAJourTextesStatut()
    {
        Error = ""; // Reset erreur
        if (IsOpen)
        {
            StatusLabel = "Statut actuel : OUVERT (En cours)";
            StatusFeedback = "✅ Vous avez OUVERT ce suivi.";
        }
        else
        {
            StatusLabel = "Statut actuel : FERMÉ (Terminé)";
            StatusFeedback = "🔒 Vous avez FERMÉ ce suivi.";
        }
    }

    private void AjouterPleinVide()
    {
        ListePleins.Add(new PleinCarburant { 
            Litres = 0, 
            DatePlein = DateTime.Now, 
            SuiviId = _suiviId 
        });
    }

    private void SupprimerPlein(PleinCarburant p)
    {
        if (p != null) ListePleins.Remove(p);
    }

    private async Task SauvegarderModifications()
    {
        Error = "";
        
        if (!int.TryParse(KmDepart, out int kmDep)) { Error = "Km Départ invalide"; return; }
        
        int? kmArr = null;
        if (!string.IsNullOrWhiteSpace(KmArrivee))
        {
            if (int.TryParse(KmArrivee, out int valArr)) kmArr = valArr;
            else { Error = "Km Arrivée invalide"; return; }
        }

        // Vérification de sécurité avant sauvegarde
        if (IsOpen && VerifierSiAutreSuiviOuvert())
        {
            Error = "Impossible de sauvegarder : Un autre trajet est déjà ouvert.";
            return;
        }

        try
        {
            using var ctx = new FleetDbContext();
            
            var dbSuivi = await ctx.Suivis.Include(s => s.Pleins).FirstOrDefaultAsync(s => s.Id == _suiviId);
            if (dbSuivi == null) return;

            // 1. Update SUIVIE
            dbSuivi.DateSuivi = DateSuivi?.DateTime ?? DateTime.Now;
            dbSuivi.Destination = Destination;
            dbSuivi.KmDepart = kmDep;
            dbSuivi.KmArrivee = kmArr;
            dbSuivi.Status = IsOpen;

            // 2. Update CONCERNER (Pleins)
            var idsUI = ListePleins.Where(p => p.Id != 0).Select(p => p.Id).ToList();
            ctx.PleinsCarburants.RemoveRange(dbSuivi.Pleins.Where(p => !idsUI.Contains(p.Id)));

            foreach (var pUI in ListePleins)
            {
                if (pUI.Litres <= 0) continue; 

                if (pUI.Id == 0) // Nouveau
                {
                    ctx.PleinsCarburants.Add(new PleinCarburant {
                        DatePlein = pUI.DatePlein,
                        Litres = pUI.Litres,
                        SuiviId = dbSuivi.Id,
                        VehiculeId = dbSuivi.VehiculeId
                    });
                }
                else // Existant
                {
                    var existing = dbSuivi.Pleins.FirstOrDefault(x => x.Id == pUI.Id);
                    if (existing != null) {
                        existing.Litres = pUI.Litres;
                        existing.DatePlein = pUI.DatePlein;
                    }
                }
            }

            await ctx.SaveChangesAsync();
            
            // Redirection dynamique après sauvegarde
            RetournerALaSource();
        }
        catch (Exception ex) { Error = "Erreur Sauvegarde : " + ex.Message; }
    }
}

// --- CONVERTISSEUR POUR DATE ---
public class DateTimeToOffsetConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt) return new DateTimeOffset(dt);
        return DateTimeOffset.Now;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTimeOffset dto) return dto.DateTime;
        return DateTime.Now;
    }
}