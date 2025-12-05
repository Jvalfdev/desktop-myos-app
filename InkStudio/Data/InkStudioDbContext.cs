using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using InkStudio.Models;

namespace InkStudio.Data;

public class InkStudioDbContext : DbContext
{
    // ══════════════════════════════════════════════════════════════
    // TABLAS
    // ══════════════════════════════════════════════════════════════
    
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<Trabajo> Trabajos => Set<Trabajo>();
    public DbSet<Consentimiento> Consentimientos => Set<Consentimiento>();
    public DbSet<Configuracion> Configuracion => Set<Configuracion>();

    // ══════════════════════════════════════════════════════════════
    // CONFIGURACIÓN DE CONEXIÓN
    // ══════════════════════════════════════════════════════════════
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var appData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(appData, "InkStudio");
        var dbPath = Path.Combine(folder, "data.db");
        
        Directory.CreateDirectory(folder);
        options.UseSqlite($"Data Source={dbPath}");
    }

    // ══════════════════════════════════════════════════════════════
    // CONFIGURACIÓN DE MODELOS
    // ══════════════════════════════════════════════════════════════
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasIndex(e => e.Telefono).IsUnique();
            entity.HasIndex(e => new { e.Nombre, e.Apellidos });
        });

        // Cita
        modelBuilder.Entity<Cita>(entity =>
        {
            entity.HasIndex(e => e.Fecha);
            entity.HasIndex(e => e.Estado);
            
            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Citas)
                  .HasForeignKey(e => e.ClienteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Trabajo
        modelBuilder.Entity<Trabajo>(entity =>
        {
            entity.HasIndex(e => e.Fecha);
            
            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Trabajos)
                  .HasForeignKey(e => e.ClienteId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Cita)
                  .WithOne(c => c.Trabajo)
                  .HasForeignKey<Trabajo>(e => e.CitaId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Consentimiento
        modelBuilder.Entity<Consentimiento>(entity =>
        {
            entity.HasIndex(e => e.Tipo);
            
            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Consentimientos)
                  .HasForeignKey(e => e.ClienteId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Trabajo)
                  .WithOne(t => t.Consentimiento)
                  .HasForeignKey<Consentimiento>(e => e.TrabajoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuración - Seed inicial
        modelBuilder.Entity<Configuracion>().HasData(new Configuracion
        {
            Id = 1,
            NombreEstudio = "InkStudio",
            TemaOscuro = true,
            IdiomaApp = "es"
        });
    }
}

