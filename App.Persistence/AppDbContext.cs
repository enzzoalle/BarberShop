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
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Nome)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.NumeroTelefone)
            .IsUnique();

        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Usuario)
            .WithOne(u => u.Cliente)
            .HasForeignKey<Cliente>(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Usuario> Usuario { get; set; }
    public DbSet<Empresa> Empresa { get; set; }
    public DbSet<Agendamento> Agendamento { get; set; }
    public DbSet<Servico> Servico { get; set; }
    public DbSet<Cliente> Cliente { get; set; }
}