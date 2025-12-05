namespace InkStudio.Models;

/// <summary>
/// Tipo de cita
/// </summary>
public enum TipoCita
{
    Tatuaje = 0,
    Piercing = 1,
    Consulta = 2,
    Retoque = 3
}

/// <summary>
/// Estado de una cita
/// </summary>
public enum EstadoCita
{
    Pendiente = 0,
    Confirmada = 1,
    EnProceso = 2,
    Completada = 3,
    Cancelada = 4,
    NoShow = 5
}

/// <summary>
/// Tipo de trabajo realizado
/// </summary>
public enum TipoTrabajo
{
    Tatuaje = 0,
    Piercing = 1
}

/// <summary>
/// Tipo de consentimiento
/// </summary>
public enum TipoConsentimiento
{
    RGPD = 0,
    Imagenes = 1,
    Trabajo = 2
}

