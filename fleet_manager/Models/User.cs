namespace FleetManager.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    // si ta colonne en base s'appelle 'password'
    [Column("password")]
    public string PasswordHash { get; set; } = "";
}
