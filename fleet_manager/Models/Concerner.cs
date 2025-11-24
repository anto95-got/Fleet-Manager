using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.Models;

[Table("concerner")] 
public class PleinCarburant
{
    [Key]
    [Column("id_concerner")]
    public int Id { get; set; }

    [Column("date_plein")]
    public DateTime DatePlein { get; set; }

    [Column("litres")]
    [Precision(6, 2)]
    public decimal Litres { get; set; }

    // --- NOUVELLES COLONNES ---

    [Column("Prix_plein")]
    [Precision(10, 2)] // DECIMAL(10,2) en base
    public decimal PrixPlein { get; set; } = 0;

    [Column("Entretien_prix")]
    [Precision(10, 2)]
    public decimal EntretienPrix { get; set; } = 0;

    [Column("Entretien_date")]
    public DateTime? EntretienDate { get; set; } = DateTime.Now;

    [Column("entretien_description")]
    public string EntretienDescription { get; set; } = "Aucun entretien";

    // --- RELATIONS ---

    [Column("id_vehicule")]
    public int VehiculeId { get; set; }
    
    [ForeignKey("VehiculeId")]
    public Vehicule? Vehicule { get; set; }

    [Column("id_suivi")] 
    public int SuiviId { get; set; }
    
    [ForeignKey("SuiviId")]
    public Suivi? Suivi { get; set; }
}