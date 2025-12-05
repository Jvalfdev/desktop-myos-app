namespace InkStudio.Models;

/// <summary>
/// Tipo de cita que se puede agendar en el estudio.
/// </summary>
public enum TipoCita
{
    /// <summary>
    /// Cita para realizar un tatuaje.
    /// </summary>
    Tatuaje = 0,

    /// <summary>
    /// Cita para realizar un piercing.
    /// </summary>
    Piercing = 1,

    /// <summary>
    /// Consulta inicial o de diseño (sin trabajo).
    /// </summary>
    Consulta = 2,

    /// <summary>
    /// Retoque de un trabajo existente.
    /// </summary>
    Retoque = 3
}

/// <summary>
/// Estado del ciclo de vida de una cita.
/// </summary>
public enum EstadoCita
{
    /// <summary>
    /// Cita creada, pendiente de confirmación.
    /// </summary>
    Pendiente = 0,

    /// <summary>
    /// Cita confirmada por el cliente.
    /// </summary>
    Confirmada = 1,

    /// <summary>
    /// El trabajo está en curso.
    /// </summary>
    EnProceso = 2,

    /// <summary>
    /// Cita completada satisfactoriamente.
    /// </summary>
    Completada = 3,

    /// <summary>
    /// Cita cancelada (por cliente o estudio).
    /// </summary>
    Cancelada = 4,

    /// <summary>
    /// El cliente no se presentó (No Show).
    /// </summary>
    NoShow = 5
}

/// <summary>
/// Tipo de trabajo realizado en el estudio.
/// </summary>
public enum TipoTrabajo
{
    /// <summary>
    /// Trabajo de tatuaje.
    /// </summary>
    Tatuaje = 0,

    /// <summary>
    /// Trabajo de piercing.
    /// </summary>
    Piercing = 1
}

/// <summary>
/// Tipo de consentimiento que debe firmar el cliente.
/// </summary>
/// <remarks>
/// - RGPD: Obligatorio para cumplir con la protección de datos.
/// - Imagenes: Opcional, para usar fotos en redes sociales.
/// - Trabajo: Obligatorio antes de cada tatuaje/piercing.
/// </remarks>
public enum TipoConsentimiento
{
    /// <summary>
    /// Consentimiento RGPD (protección de datos personales).
    /// Se firma una vez por cliente.
    /// </summary>
    RGPD = 0,

    /// <summary>
    /// Consentimiento de uso de imágenes en redes sociales.
    /// Se firma una vez por cliente.
    /// </summary>
    Imagenes = 1,

    /// <summary>
    /// Consentimiento específico para un trabajo.
    /// Se firma antes de cada tatuaje/piercing.
    /// </summary>
    Trabajo = 2
}
