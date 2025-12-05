namespace InkStudio.Models;

/// <summary>
/// Entidad singleton que almacena la configuración del estudio.
/// Solo existe un registro (Id = 1) en la base de datos.
/// </summary>
/// <remarks>
/// Incluye datos del estudio, configuración SMTP para emails
/// y preferencias de la aplicación.
/// </remarks>
public class Configuracion
{
    #region Identificación

    /// <summary>
    /// Identificador único. Siempre es 1 (singleton).
    /// </summary>
    public int Id { get; set; } = 1;

    #endregion

    #region Datos del Estudio

    /// <summary>
    /// Nombre del estudio de tatuajes.
    /// </summary>
    public string NombreEstudio { get; set; } = "Mi Estudio";

    /// <summary>
    /// Dirección física del estudio (opcional).
    /// </summary>
    public string? Direccion { get; set; }

    /// <summary>
    /// Teléfono de contacto del estudio (opcional).
    /// </summary>
    public string? Telefono { get; set; }

    /// <summary>
    /// Email de contacto del estudio (opcional).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Ruta al archivo del logo del estudio (opcional).
    /// </summary>
    public string? LogoPath { get; set; }

    #endregion

    #region Configuración SMTP (Email)

    /// <summary>
    /// Servidor SMTP para envío de emails.
    /// Ej: "smtp.gmail.com"
    /// </summary>
    public string? SmtpServidor { get; set; }

    /// <summary>
    /// Puerto del servidor SMTP.
    /// Valor por defecto: 587 (TLS).
    /// </summary>
    public int SmtpPuerto { get; set; } = 587;

    /// <summary>
    /// Usuario para autenticación SMTP.
    /// </summary>
    public string? SmtpUsuario { get; set; }

    /// <summary>
    /// Contraseña para autenticación SMTP.
    /// </summary>
    /// <remarks>
    /// TODO: En producción debería estar cifrada.
    /// </remarks>
    public string? SmtpPassword { get; set; }

    /// <summary>
    /// Indica si usar SSL/TLS para la conexión SMTP.
    /// </summary>
    public bool SmtpUsarSsl { get; set; } = true;

    #endregion

    #region Preferencias de la Aplicación

    /// <summary>
    /// Indica si usar tema oscuro en la UI.
    /// </summary>
    public bool TemaOscuro { get; set; } = true;

    /// <summary>
    /// Código de idioma de la aplicación.
    /// Valor por defecto: "es" (español).
    /// </summary>
    public string IdiomaApp { get; set; } = "es";

    #endregion

    #region Propiedades Calculadas

    /// <summary>
    /// Indica si el SMTP está configurado correctamente.
    /// Requiere servidor y usuario como mínimo.
    /// </summary>
    public bool SmtpConfigurado => !string.IsNullOrEmpty(SmtpServidor) &&
                                   !string.IsNullOrEmpty(SmtpUsuario);

    #endregion
}
