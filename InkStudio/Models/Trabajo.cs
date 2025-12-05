using System;
using System.Collections.Generic;
using System.Text.Json;

namespace InkStudio.Models;

public class Trabajo
{
    public int Id { get; set; }
    
    public int ClienteId { get; set; }
    public int? CitaId { get; set; }
    public TipoTrabajo Tipo { get; set; } = TipoTrabajo.Tatuaje;
    public string Descripcion { get; set; } = string.Empty;
    public string ZonaCuerpo { get; set; } = string.Empty;
    public string? Estilo { get; set; }
    public string? Tamano { get; set; }
    public bool Colores { get; set; } = false;
    public decimal Precio { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public int DuracionMinutos { get; set; }
    public string? FotosJson { get; set; }
    public string? Notas { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    // Navegación
    public Cliente Cliente { get; set; } = null!;
    public Cita? Cita { get; set; }
    public Consentimiento? Consentimiento { get; set; }
    
    // Propiedades calculadas
    public List<string> Fotos
    {
        get => string.IsNullOrEmpty(FotosJson) 
            ? new List<string>() 
            : JsonSerializer.Deserialize<List<string>>(FotosJson) ?? new();
        set => FotosJson = JsonSerializer.Serialize(value);
    }
    
    public string IconoTipo => Tipo switch
    {
        TipoTrabajo.Tatuaje => "🎨",
        TipoTrabajo.Piercing => "💎",
        _ => "💼"
    };
}

