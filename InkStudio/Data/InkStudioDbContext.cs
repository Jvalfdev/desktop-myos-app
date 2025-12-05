using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using InkStudio.Models;

namespace InkStudio.Data;

/// <summary>
/// Contexto de base de datos para InkStudio CRM.
/// Usa SQLite como motor de base de datos.
/// </summary>
/// <remarks>
/// La base de datos se almacena en:
/// %LOCALAPPDATA%\InkStudio\data.db
/// </remarks>
public class InkStudioDbContext : DbContext
{
    #region DbSets (Tablas)

    /// <summary>
    /// Tabla de clientes del estudio.
    /// </summary>
    public DbSet<Cliente> Clientes => Set<Cliente>();

    /// <summary>
    /// Tabla de citas agendadas.
    /// </summary>
    public DbSet<Cita> Citas => Set<Cita>();

    /// <summary>
    /// Tabla de trabajos realizados.
    /// </summary>
    public DbSet<Trabajo> Trabajos => Set<Trabajo>();

    /// <summary>
    /// Tabla de consentimientos firmados.
    /// </summary>
    public DbSet<Consentimiento> Consentimientos => Set<Consentimiento>();

    /// <summary>
    /// Tabla de configuración (singleton, solo 1 registro).
    /// </summary>
    public DbSet<Configuracion> Configuracion => Set<Configuracion>();

    #endregion

    #region Configuración de Conexión

    /// <summary>
    /// Configura la conexión a la base de datos SQLite.
    /// </summary>
    /// <param name="options">Builder de opciones de configuración.</param>
    /// <remarks>
    /// Crea automáticamente la carpeta si no existe.
    /// Ruta: %LOCALAPPDATA%\InkStudio\data.db
    /// </remarks>
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var appData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Combine(appData, "InkStudio");
        var dbPath = Path.Combine(folder, "data.db");

        // Crear carpeta si no existe
        Directory.CreateDirectory(folder);

        options.UseSqlite($"Data Source={dbPath}");
    }

    #endregion

    #region Configuración de Modelos

    /// <summary>
    /// Configura las relaciones y restricciones del modelo de datos.
    /// </summary>
    /// <param name="modelBuilder">Builder para configurar el modelo.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigurarCliente(modelBuilder);
        ConfigurarCita(modelBuilder);
        ConfigurarTrabajo(modelBuilder);
        ConfigurarConsentimiento(modelBuilder);
        ConfigurarDatosIniciales(modelBuilder);
    }

    /// <summary>
    /// Configura la entidad Cliente.
    /// </summary>
    /// <param name="modelBuilder">Builder del modelo.</param>
    private static void ConfigurarCliente(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            // Teléfono único
            entity.HasIndex(e => e.Telefono).IsUnique();

            // Índice para búsquedas por nombre
            entity.HasIndex(e => new { e.Nombre, e.Apellidos });
        });
    }

    /// <summary>
    /// Configura la entidad Cita y sus relaciones.
    /// </summary>
    /// <param name="modelBuilder">Builder del modelo.</param>
    private static void ConfigurarCita(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cita>(entity =>
        {
            // Índices para consultas frecuentes
            entity.HasIndex(e => e.Fecha);
            entity.HasIndex(e => e.Estado);

            // Relación: Cliente -> Citas (1:N)
            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Citas)
                  .HasForeignKey(e => e.ClienteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configura la entidad Trabajo y sus relaciones.
    /// </summary>
    /// <param name="modelBuilder">Builder del modelo.</param>
    private static void ConfigurarTrabajo(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Trabajo>(entity =>
        {
            // Índice por fecha
            entity.HasIndex(e => e.Fecha);

            // Relación: Cliente -> Trabajos (1:N)
            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Trabajos)
                  .HasForeignKey(e => e.ClienteId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Relación: Cita -> Trabajo (1:1 opcional)
            entity.HasOne(e => e.Cita)
                  .WithOne(c => c.Trabajo)
                  .HasForeignKey<Trabajo>(e => e.CitaId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// Configura la entidad Consentimiento y sus relaciones.
    /// </summary>
    /// <param name="modelBuilder">Builder del modelo.</param>
    private static void ConfigurarConsentimiento(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Consentimiento>(entity =>
        {
            // Índice por tipo
            entity.HasIndex(e => e.Tipo);

            // Relación: Cliente -> Consentimientos (1:N)
            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Consentimientos)
                  .HasForeignKey(e => e.ClienteId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Relación: Trabajo -> Consentimiento (1:1 opcional)
            entity.HasOne(e => e.Trabajo)
                  .WithOne(t => t.Consentimiento)
                  .HasForeignKey<Consentimiento>(e => e.TrabajoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// Configura los datos iniciales (seed) de la base de datos.
    /// </summary>
    /// <param name="modelBuilder">Builder del modelo.</param>
    private static void ConfigurarDatosIniciales(ModelBuilder modelBuilder)
    {
        // Configuración inicial del estudio
        modelBuilder.Entity<Configuracion>().HasData(new Configuracion
        {
            Id = 1,
            NombreEstudio = "InkStudio",
            TemaOscuro = true,
            IdiomaApp = "es"
        });
    }

    #endregion
}
