using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

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
    public int? KmArrivee { get; set; }

    [Column("destination")]
    public string? Destination { get; set; }

    [Column("commentaire")]
    public string? Commentaire { get; set; }

    [Column("Status")] 
    public bool Status { get; set; } 

    // --- NOUVELLE COLONNE ---
    
    [Column("Prix")]
    [Precision(10, 2)] // DECIMAL(10,2) pour stocker le calcul du trigger
    public decimal Prix { get; set; }

    // --- RELATIONS ---
    
    [Column("id_user")]
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User? User { get; set; }

    [Column("id_vehicule")]
    public int VehiculeId { get; set; }
    
    [ForeignKey("VehiculeId")]
    public Vehicule? Vehicule { get; set; }
    
    public ICollection<PleinCarburant> Pleins { get; set; } = new List<PleinCarburant>();
}