using System;
using Microsoft.EntityFrameworkCore;
using FleetManager.Models;

namespace FleetManager.Data;

public class FleetDbContext : DbContext
{
    public FleetDbContext() { }
    public FleetDbContext(DbContextOptions<FleetDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseMySql(
                "server=localhost;database=fleet_manager;user=root;password=;",
                new MySqlServerVersion(new Version(8, 0, 36))
            );
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}