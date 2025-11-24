using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace FleetManager.Models;

[Table("Suivie")] 
public class Suivi
{
    [Key]
    [Column("id_suivi")]
    public int Id { get; set; }

    [Column("date_suivi")]
    public DateTime DateSuivi { get; set; }

    [Column("km_depart")]
    public int KmDepart { get; set; }

    [Column("km_arrivee")]
    public int? KmArrivee { get; set; } // Nullable car au départ on ne sait pas

    [Column("destination")]
    public string? Destination { get; set; }

    [Column("commentaire")]
    public string? Commentaire { get; set; }

    // 1 = Ouvert (En cours), 0 = Fermé
    // EntityFramework convertit automatiquement tinyint(1) en bool
    [Column("Status")] 
    public bool Status { get; set; } 

    // --- RELATIONS ---
    
    [Column("id_user")]
    public int UserId { get; set; }
    
    // Pas obligatoire de charger le User pour l'affichage, mais bonne pratique
    [ForeignKey("UserId")]
    public User? User { get; set; }

    [Column("id_vehicule")]
    public int VehiculeId { get; set; }
    
    // 🔥 C'est GRÂCE À ÇA qu'on aura la plaque et la marque !
    [ForeignKey("VehiculeId")]
    public Vehicule? Vehicule { get; set; }
    
    public ICollection<PleinCarburant> Pleins { get; set; } = new List<PleinCarburant>();

}