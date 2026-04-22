using App.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuarios>()
            .HasIndex(u => u.Nome)
            .IsUnique();

        modelBuilder.Entity<Usuarios>()
            .HasIndex(u => u.NumeroTelefone)
            .IsUnique();

        modelBuilder.Entity<Clientes>()
            .HasOne(c => c.Usuario)
            .WithOne(u => u.Cliente)
            .HasForeignKey<Clientes>(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<FolgasFeriados>()
            .HasOne(x => x.Parametros)
            .WithMany(x => x.FolgasFeriados)
            .HasForeignKey(x => x.ParametrosId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FolgasFeriados>()
            .HasIndex(x => new { x.ParametrosId, x.Data })
            .IsUnique();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Usuarios> Usuarios { get; set; }
    public DbSet<Parametros> Parametros { get; set; }
    public DbSet<Agendamentos> Agendamentos { get; set; }
    public DbSet<Servicos> Servicos { get; set; }
    public DbSet<Clientes> Clientes { get; set; }
    public DbSet<FolgasFeriados> FolgasFeriados { get; set; }
}