using System;
using System.Collections.Generic;

namespace InkStudio.Models;

public class Cliente
{
    public int Id { get; set; }
    
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Alergias { get; set; }
    public string? Notas { get; set; }
    public bool EsVip { get; set; } = false;
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    
    // Navegación
    public List<Cita> Citas { get; set; } = new();
    public List<Trabajo> Trabajos { get; set; } = new();
    public List<Consentimiento> Consentimientos { get; set; } = new();
    
    // Propiedades calculadas
    public string NombreCompleto => $"{Nombre} {Apellidos}";
    
    public int? Edad => FechaNacimiento.HasValue 
        ? (int)((DateTime.Today - FechaNacimiento.Value).TotalDays / 365.25)
        : null;
}

