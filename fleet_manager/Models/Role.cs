using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetManager.Models;

[Table("role")]
public class Role
{
    [Key]
    [Column("id_role")]
    public int Id_role { get; set; }

    [Column("nom_role")]
    public string Nom_role { get; set; } = "";
}