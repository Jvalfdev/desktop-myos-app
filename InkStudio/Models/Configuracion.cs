namespace InkStudio.Models;

public class Configuracion
{
    public int Id { get; set; } = 1;
    
    // Datos del estudio
    public string NombreEstudio { get; set; } = "Mi Estudio";
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? LogoPath { get; set; }
    
    // Configuración SMTP
    public string? SmtpServidor { get; set; }
    public int SmtpPuerto { get; set; } = 587;
    public string? SmtpUsuario { get; set; }
    public string? SmtpPassword { get; set; }
    public bool SmtpUsarSsl { get; set; } = true;
    
    // Preferencias
    public bool TemaOscuro { get; set; } = true;
    public string IdiomaApp { get; set; } = "es";
    
    // Propiedades calculadas
    public bool SmtpConfigurado => !string.IsNullOrEmpty(SmtpServidor) && 
                                   !string.IsNullOrEmpty(SmtpUsuario);
}

