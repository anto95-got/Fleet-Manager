using System;
using Microsoft.EntityFrameworkCore;
using FleetManager.Models;

namespace FleetManager.Data;

public class FleetDbContext : DbContext
{
    public FleetDbContext() { }
    public FleetDbContext(DbContextOptions options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    
    public DbSet<Vehicule> Vehicules{ get; set; }
    
    public DbSet<Suivi> Suivis { get; set; }
    public DbSet<Role> Roles { get; set; }
    
    public DbSet<Parametre> Parametres { get; set; }

    
    public DbSet<PleinCarburant> PleinsCarburants { get; set; } 
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseMySql(
                "server=localhost;port=8889;database=fleet-manager;uid=root;pwd=root;",
                new MySqlServerVersion(new Version(8, 0, 36))
            );
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuration User (index unique sur Email)
        modelBuilder.Entity<User>()
            .ToTable("user")
            .HasIndex(u => u.Email)
            .IsUnique();

        // ⬇️ AJOUTE CETTE CONFIGURATION POUR LA RELATION User -> Role
        modelBuilder.Entity<User>()
            .HasOne(u => u.RoleInfo)
            .WithMany()
            .HasForeignKey(u => u.Role)
            .HasPrincipalKey(r => r.Id_role)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}