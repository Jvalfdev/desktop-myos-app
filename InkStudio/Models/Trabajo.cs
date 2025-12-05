using System;
using System.Collections.Generic;
using System.Text.Json;

namespace InkStudio.Models;

/// <summary>
/// Entidad que representa un trabajo realizado (tatuaje o piercing).
/// Almacena detalles técnicos, precio y fotos del resultado.
/// </summary>
/// <remarks>
/// Un trabajo está vinculado a un cliente y opcionalmente a una cita.
/// Las fotos se almacenan como rutas en formato JSON.
/// </remarks>
public class Trabajo
{
    #region Identificación

    /// <summary>
    /// Identificador único del trabajo (clave primaria).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID del cliente asociado (clave foránea).
    /// </summary>
    public int ClienteId { get; set; }

    /// <summary>
    /// ID de la cita asociada (opcional).
    /// </summary>
    public int? CitaId { get; set; }

    #endregion

    #region Detalles del Trabajo

    /// <summary>
    /// Tipo de trabajo (Tatuaje o Piercing).
    /// </summary>
    public TipoTrabajo Tipo { get; set; } = TipoTrabajo.Tatuaje;

    /// <summary>
    /// Descripción del trabajo realizado.
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Zona del cuerpo donde se realizó el trabajo.
    /// </summary>
    public string ZonaCuerpo { get; set; } = string.Empty;

    /// <summary>
    /// Estilo del tatuaje (opcional, ej: "Realismo", "Old School").
    /// </summary>
    public string? Estilo { get; set; }

    /// <summary>
    /// Tamaño del trabajo (opcional, ej: "10x15cm").
    /// </summary>
    public string? Tamano { get; set; }

    /// <summary>
    /// Indica si el trabajo tiene colores (para tatuajes).
    /// </summary>
    public bool Colores { get; set; } = false;

    #endregion

    #region Precio y Tiempo

    /// <summary>
    /// Precio cobrado por el trabajo.
    /// </summary>
    public decimal Precio { get; set; }

    /// <summary>
    /// Fecha en que se realizó el trabajo.
    /// </summary>
    public DateTime Fecha { get; set; } = DateTime.Now;

    /// <summary>
    /// Duración real del trabajo en minutos.
    /// </summary>
    public int DuracionMinutos { get; set; }

    #endregion

    #region Fotos y Notas

    /// <summary>
    /// Rutas de las fotos en formato JSON.
    /// Usar la propiedad <see cref="Fotos"/> para acceder.
    /// </summary>
    public string? FotosJson { get; set; }

    /// <summary>
    /// Notas adicionales sobre el trabajo (opcional).
    /// </summary>
    public string? Notas { get; set; }

    #endregion

    #region Auditoría

    /// <summary>
    /// Fecha y hora de creación del registro.
    /// </summary>
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    #endregion

    #region Navegación (Relaciones)

    /// <summary>
    /// Cliente al que se realizó el trabajo.
    /// </summary>
    public Cliente Cliente { get; set; } = null!;

    /// <summary>
    /// Cita de la que surgió el trabajo (si existe).
    /// </summary>
    public Cita? Cita { get; set; }

    /// <summary>
    /// Consentimiento asociado al trabajo (si existe).
    /// </summary>
    public Consentimiento? Consentimiento { get; set; }

    #endregion

    #region Propiedades Calculadas

    /// <summary>
    /// Lista de rutas de fotos del trabajo.
    /// Deserializa/serializa automáticamente desde/hacia FotosJson.
    /// </summary>
    public List<string> Fotos
    {
        get => string.IsNullOrEmpty(FotosJson)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(FotosJson) ?? new();
        set => FotosJson = JsonSerializer.Serialize(value);
    }

    /// <summary>
    /// Emoji representativo del tipo de trabajo.
    /// </summary>
    public string IconoTipo => Tipo switch
    {
        TipoTrabajo.Tatuaje => "🎨",
        TipoTrabajo.Piercing => "💎",
        _ => "💼"
    };

    #endregion
}
