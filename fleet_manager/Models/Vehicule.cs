using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.Models;

[Table("vehicule")]
public class Vehicule
{   
    [Key] 
    [Column("id_vehicule")] 
    public int Id { get; set; }

    [Column("immatriculation")] 
    public string Imatricule { get; set; } = "";

    [Column("marque")] 
    public string Marque { get; set; } = "";

    [Column("modele")] 
    public string Modele { get; set; } = "";

    [Column("annee")] 
    public int Annee { get; set; } 

    [Column("kilometrage")] 
    public int Kilometrage { get; set; } 

    [Column("statut")] 
    public string Status { get; set; } = "";

    [Column("capacite")] 
    public int Capacite { get; set; } = 50; 

    // --- NOUVELLES COLONNES ---

    [Column("carburant_actuel")]
    public int CarburantActuel { get; set; } = 50; // Valeur par défaut logique

    [Column("Prix_vehicule")]
    [Precision(20, 0)] // DECIMAL(20,0) selon ta base
    public decimal PrixVehicule { get; set; }
}