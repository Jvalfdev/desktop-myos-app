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

public partial class DashboardViewModel : ViewModelBase
{
    private readonly InkStudioDbContext _db = new();

    // ══════════════════════════════════════════════════════════════
    // PROPIEDADES - Citas de hoy
    // ══════════════════════════════════════════════════════════════
    
    [ObservableProperty]
    private ObservableCollection<Cita> _citasHoy = new();

    // ══════════════════════════════════════════════════════════════
    // PROPIEDADES - Estadísticas
    // ══════════════════════════════════════════════════════════════
    
    [ObservableProperty]
    private int _totalClientes;

    [ObservableProperty]
    private int _citasHoyCount;

    [ObservableProperty]
    private decimal _ingresosSemana;

    [ObservableProperty]
    private int _citasPendientesConfirmar;

    // ══════════════════════════════════════════════════════════════
    // PROPIEDADES - Alertas
    // ══════════════════════════════════════════════════════════════
    
    [ObservableProperty]
    private ObservableCollection<string> _alertas = new();

    // ══════════════════════════════════════════════════════════════
    // PROPIEDADES - UI
    // ══════════════════════════════════════════════════════════════
    
    [ObservableProperty]
    private string _saludo = "Buenos días";

    [ObservableProperty]
    private string _fechaHoy = DateTime.Now.ToString("dddd, d MMMM yyyy");

    [ObservableProperty]
    private string _nombreEstudio = "InkStudio";

    // ══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ══════════════════════════════════════════════════════════════
    
    public DashboardViewModel()
    {
        ActualizarSaludo();
    }

    // ══════════════════════════════════════════════════════════════
    // COMANDOS
    // ══════════════════════════════════════════════════════════════
    
    [RelayCommand]
    private async Task CargarDatos()
    {
        await CargarCitasHoy();
        await CargarEstadisticas();
        await CargarAlertas();
        await CargarConfiguracion();
    }

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

    // ══════════════════════════════════════════════════════════════
    // MÉTODOS PRIVADOS
    // ══════════════════════════════════════════════════════════════
    
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

    private async Task CargarEstadisticas()
    {
        // Total de clientes
        TotalClientes = await _db.Clientes.CountAsync(c => c.Activo);

        // Citas pendientes de confirmar (próximos 7 días)
        var enUnaSemana = DateTime.Today.AddDays(7);
        CitasPendientesConfirmar = await _db.Citas
            .CountAsync(c => c.Estado == EstadoCita.Pendiente && 
                            c.Fecha >= DateTime.Today && 
                            c.Fecha <= enUnaSemana);

        // Ingresos de la semana
        var inicioSemana = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
        var finSemana = inicioSemana.AddDays(7);
        IngresosSemana = await _db.Trabajos
            .Where(t => t.Fecha >= inicioSemana && t.Fecha < finSemana)
            .SumAsync(t => t.Precio);
    }

    private async Task CargarAlertas()
    {
        var alertas = new ObservableCollection<string>();

        // Citas sin confirmar para mañana
        var manana = DateTime.Today.AddDays(1);
        var citasManana = await _db.Citas
            .CountAsync(c => c.Fecha.Date == manana && c.Estado == EstadoCita.Pendiente);
        
        if (citasManana > 0)
        {
            alertas.Add($"📅 {citasManana} cita(s) sin confirmar para mañana");
        }

        // Clientes sin consentimiento RGPD
        var sinRgpd = await _db.Clientes
            .CountAsync(c => c.Activo && !c.Consentimientos.Any(con => con.Tipo == TipoConsentimiento.RGPD && con.Firmado));
        
        if (sinRgpd > 0)
        {
            alertas.Add($"📝 {sinRgpd} cliente(s) sin consentimiento RGPD");
        }

        Alertas = alertas;
    }

    private async Task CargarConfiguracion()
    {
        var config = await _db.Configuracion.FirstOrDefaultAsync();
        if (config != null)
        {
            NombreEstudio = config.NombreEstudio;
        }
    }
}

