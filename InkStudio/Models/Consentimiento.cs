using System;

namespace InkStudio.Models;

public class Consentimiento
{
    public int Id { get; set; }
    
    public int ClienteId { get; set; }
    public int? TrabajoId { get; set; }
    public TipoConsentimiento Tipo { get; set; }
    public DateTime FechaFirma { get; set; } = DateTime.Now;
    public string? RutaDocumento { get; set; }
    public bool Firmado { get; set; } = false;
    public string? Notas { get; set; }
    
    // Navegación
    public Cliente Cliente { get; set; } = null!;
    public Trabajo? Trabajo { get; set; }
    
    // Propiedades calculadas
    public bool TieneDocumento => !string.IsNullOrEmpty(RutaDocumento);
    
    public string NombreTipo => Tipo switch
    {
        TipoConsentimiento.RGPD => "RGPD - Protección de datos",
        TipoConsentimiento.Imagenes => "Uso de imágenes",
        TipoConsentimiento.Trabajo => "Consentimiento de trabajo",
        _ => "Desconocido"
    };
}

