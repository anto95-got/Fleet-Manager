using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // Nécessaire pour [Precision]

namespace FleetManager.Models;

// J'ai renommé "Concerner" en "PleinCarburant" pour plus de clarté
// Mappé à la table "concerner" (si c'est son nom en BDD)
[Table("concerner")] 
public class PleinCarburant // ou gardez "Concerner" si vous préférez
{
    [Key]
    [Column("id_concerner")] // Nom de la colonne clé primaire
    public int Id { get; set; }

    [Column("date_plein")]
    public DateTime DatePlein { get; set; }

    [Column("litres")]
    [Precision(6, 2)] // Gardé, c'est parfait pour les décimaux
    public decimal Litres { get; set; }

    // --- Relation vers Vehicule (1 plein -> 1 véhicule) ---

    [Column("id_vehicule")] // Le nom de votre colonne de clé étrangère
    public int VehiculeId { get; set; }
    
    [ForeignKey("VehiculeId")]
    public Vehicule Vehicule { get; set; }

    // --- Relation vers Suivi (1 plein -> 1 trajet) ---
    // (Cette relation est optionnelle, vous pouvez la supprimer si un plein
    // n'est pas toujours lié à un trajet spécifique)

    [Column("id_suivi")] // Le nom de votre colonne de clé étrangère
    public int SuiviId { get; set; }
    
    [ForeignKey("SuiviId")]
    public Suivi Suivi { get; set; }
}