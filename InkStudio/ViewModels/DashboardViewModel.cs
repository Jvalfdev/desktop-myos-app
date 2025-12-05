using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using InkStudio.Data;
using InkStudio.Models;

namespace InkStudio.ViewModels;

/// <summary>
/// ViewModel para la pantalla principal (Dashboard).
/// Muestra resumen de citas del día, estadísticas y alertas.
/// </summary>
/// <remarks>
/// El Dashboard es la primera pantalla que ve el usuario.
/// Proporciona una vista rápida del estado del negocio.
/// </remarks>
public partial class DashboardViewModel : ViewModelBase
{
    #region Campos Privados

    /// <summary>
    /// Contexto de base de datos.
    /// </summary>
    private readonly InkStudioDbContext _db = new();

    #endregion

    #region Propiedades - Citas del Día

    /// <summary>
    /// Lista de citas programadas para hoy.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Cita> _citasHoy = new();

    #endregion

    #region Propiedades - Estadísticas

    /// <summary>
    /// Número total de clientes activos.
    /// </summary>
    [ObservableProperty]
    private int _totalClientes;

    /// <summary>
    /// Número de citas para el día de hoy.
    /// </summary>
    [ObservableProperty]
    private int _citasHoyCount;

    /// <summary>
    /// Total de ingresos de la semana actual.
    /// </summary>
    [ObservableProperty]
    private decimal _ingresosSemana;

    /// <summary>
    /// Número de citas pendientes de confirmar (próximos 7 días).
    /// </summary>
    [ObservableProperty]
    private int _citasPendientesConfirmar;

    #endregion

    #region Propiedades - Alertas

    /// <summary>
    /// Lista de alertas y notificaciones importantes.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _alertas = new();

    #endregion

    #region Propiedades - Interfaz de Usuario

    /// <summary>
    /// Saludo que cambia según la hora del día.
    /// </summary>
    [ObservableProperty]
    private string _saludo = "Buenos días";

    /// <summary>
    /// Fecha actual formateada para mostrar en la UI.
    /// </summary>
    [ObservableProperty]
    private string _fechaHoy = DateTime.Now.ToString("dddd, d MMMM yyyy");

    /// <summary>
    /// Nombre del estudio (desde configuración).
    /// </summary>
    [ObservableProperty]
    private string _nombreEstudio = "InkStudio";

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa el ViewModel del Dashboard.
    /// </summary>
    public DashboardViewModel()
    {
        ActualizarSaludo();
    }

    #endregion

    #region Comandos

    /// <summary>
    /// Carga todos los datos del Dashboard.
    /// Se ejecuta al mostrar la vista.
    /// </summary>
    [RelayCommand]
    private async Task CargarDatos()
    {
        await CargarCitasHoy();
        await CargarEstadisticas();
        await CargarAlertas();
        await CargarConfiguracion();
    }

    /// <summary>
    /// Marca una cita como confirmada.
    /// </summary>
    /// <param name="cita">Cita a confirmar.</param>
    [RelayCommand]
    private async Task MarcarCitaConfirmada(Cita cita)
    {
        if (cita.Estado == EstadoCita.Pendiente)
        {
            cita.Estado = EstadoCita.Confirmada;
            await _db.SaveChangesAsync();
            await CargarDatos();
        }
    }

    /// <summary>
    /// Marca una cita como completada.
    /// </summary>
    /// <param name="cita">Cita a completar.</param>
    [RelayCommand]
    private async Task MarcarCitaCompletada(Cita cita)
    {
        if (cita.Estado == EstadoCita.Confirmada || cita.Estado == EstadoCita.EnProceso)
        {
            cita.Estado = EstadoCita.Completada;
            await _db.SaveChangesAsync();
            await CargarDatos();
        }
    }

    #endregion

    #region Métodos Privados

    /// <summary>
    /// Actualiza el saludo según la hora del día.
    /// Mañana (< 12), Tarde (12-20), Noche (> 20).
    /// </summary>
    private void ActualizarSaludo()
    {
        var hora = DateTime.Now.Hour;
        Saludo = hora switch
        {
            < 12 => "Buenos días 👋",
            < 20 => "Buenas tardes 👋",
            _ => "Buenas noches 👋"
        };
    }

    /// <summary>
    /// Carga las citas del día actual.
    /// </summary>
    /// <remarks>
    /// El ordenamiento se hace en memoria porque SQLite
    /// no soporta OrderBy con TimeSpan.
    /// </remarks>
    private async Task CargarCitasHoy()
    {
        var hoy = DateTime.Today;
        var citas = await _db.Citas
            .Include(c => c.Cliente)
            .Where(c => c.Fecha.Date == hoy)
            .ToListAsync();

        // Ordenar en memoria (SQLite no soporta OrderBy con TimeSpan)
        var citasOrdenadas = citas.OrderBy(c => c.HoraInicio).ToList();

        CitasHoy = new ObservableCollection<Cita>(citasOrdenadas);
        CitasHoyCount = citasOrdenadas.Count;
    }

    /// <summary>
    /// Carga las estadísticas generales del negocio.
    /// </summary>
    private async Task CargarEstadisticas()
    {
        // Total de clientes activos
        TotalClientes = await _db.Clientes.CountAsync(c => c.Activo);

        // Citas pendientes de confirmar (próximos 7 días)
        var enUnaSemana = DateTime.Today.AddDays(7);
        CitasPendientesConfirmar = await _db.Citas
            .CountAsync(c => c.Estado == EstadoCita.Pendiente &&
                            c.Fecha >= DateTime.Today &&
                            c.Fecha <= enUnaSemana);

        // Ingresos de la semana (Lunes a Domingo)
        var inicioSemana = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
        var finSemana = inicioSemana.AddDays(7);
        IngresosSemana = await _db.Trabajos
            .Where(t => t.Fecha >= inicioSemana && t.Fecha < finSemana)
            .SumAsync(t => t.Precio);
    }

    /// <summary>
    /// Carga alertas y notificaciones importantes.
    /// </summary>
    private async Task CargarAlertas()
    {
        var alertas = new ObservableCollection<string>();

        // Alerta: Citas sin confirmar para mañana
        var manana = DateTime.Today.AddDays(1);
        var citasManana = await _db.Citas
            .CountAsync(c => c.Fecha.Date == manana && c.Estado == EstadoCita.Pendiente);

        if (citasManana > 0)
        {
            alertas.Add($"📅 {citasManana} cita(s) sin confirmar para mañana");
        }

        // Alerta: Clientes sin consentimiento RGPD firmado
        var sinRgpd = await _db.Clientes
            .CountAsync(c => c.Activo && !c.Consentimientos.Any(
                con => con.Tipo == TipoConsentimiento.RGPD && con.Firmado));

        if (sinRgpd > 0)
        {
            alertas.Add($"📝 {sinRgpd} cliente(s) sin consentimiento RGPD");
        }

        Alertas = alertas;
    }

    /// <summary>
    /// Carga la configuración del estudio.
    /// </summary>
    private async Task CargarConfiguracion()
    {
        var config = await _db.Configuracion.FirstOrDefaultAsync();
        if (config != null)
        {
            NombreEstudio = config.NombreEstudio;
        }
    }

    #endregion
}
