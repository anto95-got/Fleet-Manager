
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetManager.Models;
[Table("vehicule")]

public class Vehicule
{   
    [Key]
    [Column("id_vehicule")]
    public int Id { get; set; }

    [Column("imatricule")] public string Imatricule { get; set; } = "";
    [Column("marque")] public string Marque { get; set; } = "";
    [Column("modele")] public string Modele { get; set; } = "";
    [Column("annee")] public int Annee { get; set; } 
    [Column("kilometrage")] public int Kilometrage { get; set; } 
    [Column("status")] public string Status { get; set; } = "";



}