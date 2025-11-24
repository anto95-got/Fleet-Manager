using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetManager.Models;

[Table("user")]
public class User
{
    [Key]
    [Column("id_user")]
    public int Id { get; set; }

    [Column("nom")]
    public string Nom { get; set; } = "";

    [Column("prenom")]
    public string Prenom { get; set; } = "";

    [Column("email")]
    public string Email { get; set; } = "";

    [Column("password")]
    public string PasswordHash { get; set; } = "";

    // Ton champ existant (l'ID)
    [Column("id_role")] 
    public int Role { get; set; } = 1;

    // 🔥 AJOUT UNIQUE : L'objet pour pouvoir afficher le nom
    // Cela dit à C# : "Utilise l'int 'Role' ci-dessus pour trouver l'objet RoleInfo"
    [ForeignKey("Role")]
    public Role? RoleInfo { get; set; }
}