using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetManager.Models;

[Table("vehicule")]
public class Vehicule
{   
    [Key] [Column("id_vehicule")] public int Id { get; set; }
    [Column("immatriculation")] public string Imatricule { get; set; } = "";
    [Column("marque")] public string Marque { get; set; } = "";
    [Column("modele")] public string Modele { get; set; } = "";
    [Column("annee")] public int Annee { get; set; } 
    [Column("kilometrage")] public int Kilometrage { get; set; } 
    [Column("statut")] public string Status { get; set; } = "";

    // On garde juste la capacité MAX (ex: 50L) pour info
    [Column("capacite")] 
    public int Capacite { get; set; } = 50; 
    
}