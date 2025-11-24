using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FleetManager.Models;

[Table("parametres")]
public class Parametre
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("cle")]
    public string Cle { get; set; } = "";

    [Column("valeur")]
    [Precision(10, 2)]
    public decimal Valeur { get; set; }
}