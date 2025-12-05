using System;

namespace InkStudio.Models;

public class Cita
{
    public int Id { get; set; }
    
    public int ClienteId { get; set; }
    public DateTime Fecha { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public int DuracionMinutos { get; set; } = 60;
    public TipoCita TipoCita { get; set; } = TipoCita.Tatuaje;
    public string? Descripcion { get; set; }
    public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;
    public bool EmailEnviado { get; set; } = false;
    public DateTime? FechaEmailEnviado { get; set; }
    public string? Notas { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    // Navegación
    public Cliente Cliente { get; set; } = null!;
    public Trabajo? Trabajo { get; set; }
    
    // Propiedades calculadas
    public DateTime FechaHoraInicio => Fecha.Date + HoraInicio;
    public DateTime FechaHoraFin => FechaHoraInicio.AddMinutes(DuracionMinutos);
    public bool EsPasada => FechaHoraInicio < DateTime.Now;
    public bool EsHoy => Fecha.Date == DateTime.Today;
    
    public string HoraInicioFormateada => HoraInicio.ToString(@"hh\:mm");
    public string DuracionFormateada => DuracionMinutos >= 60 
        ? $"{DuracionMinutos / 60}h {DuracionMinutos % 60}m".TrimEnd('0', 'm', ' ')
        : $"{DuracionMinutos}m";
    
    public string IconoTipo => TipoCita switch
    {
        TipoCita.Tatuaje => "🎨",
        TipoCita.Piercing => "💎",
        TipoCita.Consulta => "📋",
        TipoCita.Retoque => "🔄",
        _ => "📅"
    };
}

