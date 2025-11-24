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

    // Clé étrangère vers la table Role
    [Column("id_role")]
    [ForeignKey(nameof(RoleInfo))]  // ⬅️ CORRECTION ICI
    public int Role { get; set; } = 1;

    // Propriété de navigation
    public Role? RoleInfo { get; set; }

    [Column("must_change_password")]
    public bool MustChangePassword { get; set; } = true;
}