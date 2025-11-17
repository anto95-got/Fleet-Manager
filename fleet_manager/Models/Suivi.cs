using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetManager.Models;

[Table("Suivie")] // Nom de votre table en base de données
public class Suivi
{
    [Key]
    [Column("id_suivi")] // Nom de la colonne clé primaire
    public int Id { get; set; }

    [Column("date_suivi")]
    public DateTime DateSuivi { get; set; }

    [Column("km_depart")]
    public int KmDepart { get; set; }

    [Column("km_arrivee")]
    public int KmArrivee { get; set; }

    [Column("destination")] // J'ai corrigé la faute de frappe "destiantion"
    public string Destination { get; set; } = "";

    [Column("commentaire")]
    public string Commentaire { get; set; } = "";

    // --- Relation vers User (1 trajet -> 1 utilisateur) ---
    
    [Column("id_user")] // Le nom de votre colonne de clé étrangère
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; }

    // --- Relation vers Vehicule (1 trajet -> 1 véhicule) ---

    [Column("id_vehicule")] // Le nom de votre colonne de clé étrangère
    public int VehiculeId { get; set; }
    
    [ForeignKey("VehiculeId")]
    public Vehicule Vehicule { get; set; }
}