using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InkStudio.Models;

namespace InkStudio.ViewModels;

/// <summary>
/// ViewModel para el control de calendario visual usando el control Calendar nativo de Avalonia.
/// Muestra un mes completo con los días y las citas marcadas.
/// </summary>
public partial class CalendarViewModel : ViewModelBase
{
    #region Propiedades

    /// <summary>
    /// Fecha del inicio de la semana actual mostrada (lunes de la semana).
    /// </summary>
    [ObservableProperty]
    private DateTime _semanaActual;

    /// <summary>
    /// Fecha del mes actual mostrado (para compatibilidad con vista mensual).
    /// </summary>
    public DateTime MesActual
    {
        get => SemanaActual;
        set
        {
            var daysFromMonday = ((int)value.DayOfWeek + 6) % 7;
            SemanaActual = value.AddDays(-daysFromMonday);
        }
    }

    /// <summary>
    /// Se ejecuta cuando cambia la semana actual.
    /// </summary>
    partial void OnSemanaActualChanged(DateTime value)
    {
        OnPropertyChanged(nameof(SemanaActualTexto));
    }

    /// <summary>
    /// Texto que muestra el rango de la semana actual.
    /// </summary>
    public string SemanaActualTexto
    {
        get
        {
            var inicio = SemanaActual;
            var fin = inicio.AddDays(6);
            if (inicio.Month == fin.Month)
            {
                return $"{inicio:dd} - {fin:dd MMMM yyyy}";
            }
            return $"{inicio:dd MMM} - {fin:dd MMM yyyy}";
        }
    }

    /// <summary>
    /// Fecha seleccionada en el calendario.
    /// </summary>
    [ObservableProperty]
    private DateTime? _fechaSeleccionada;

    /// <summary>
    /// Lista de citas para el mes actual.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Cita> _citas = new();

    /// <summary>
    /// Diccionario de fechas con número de citas para personalización visual.
    /// </summary>
    public Dictionary<DateTime, int> CitasPorFecha { get; private set; } = new();

    #endregion

    #region Comandos

    /// <summary>
    /// Navega a la semana anterior.
    /// </summary>
    [RelayCommand]
    private void SemanaAnterior()
    {
        SemanaActual = SemanaActual.AddDays(-7);
        ActualizarCitasPorFecha();
    }

    /// <summary>
    /// Navega a la semana siguiente.
    /// </summary>
    [RelayCommand]
    private void SemanaSiguiente()
    {
        SemanaActual = SemanaActual.AddDays(7);
        ActualizarCitasPorFecha();
    }

    /// <summary>
    /// Vuelve a la semana actual.
    /// </summary>
    [RelayCommand]
    private void IrASemanaActual()
    {
        var today = DateTime.Today;
        var daysFromMonday = ((int)today.DayOfWeek + 6) % 7; // Convertir Domingo=0 a Lunes=0
        SemanaActual = today.AddDays(-daysFromMonday);
        ActualizarCitasPorFecha();
    }

    /// <summary>
    /// Navega al mes anterior (para compatibilidad con vista mensual).
    /// </summary>
    [RelayCommand]
    private void MesAnterior()
    {
        SemanaActual = SemanaActual.AddMonths(-1);
        ActualizarCitasPorFecha();
    }

    /// <summary>
    /// Navega al mes siguiente (para compatibilidad con vista mensual).
    /// </summary>
    [RelayCommand]
    private void MesSiguiente()
    {
        SemanaActual = SemanaActual.AddMonths(1);
        ActualizarCitasPorFecha();
    }

    /// <summary>
    /// Vuelve al mes actual (para compatibilidad con vista mensual).
    /// </summary>
    [RelayCommand]
    private void IrAMesActual()
    {
        var today = DateTime.Today;
        var daysFromMonday = ((int)today.DayOfWeek + 6) % 7;
        SemanaActual = today.AddDays(-daysFromMonday);
        ActualizarCitasPorFecha();
    }

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Carga el calendario para la semana especificada.
    /// </summary>
    /// <param name="fecha">Fecha de la semana a mostrar.</param>
    /// <param name="citas">Lista de citas para la semana.</param>
    public void CargarSemana(DateTimeOffset fecha, IEnumerable<Cita> citas)
    {
        var fechaDateTime = fecha.DateTime;
        var daysFromMonday = ((int)fechaDateTime.DayOfWeek + 6) % 7; // Convertir Domingo=0 a Lunes=0
        SemanaActual = fechaDateTime.AddDays(-daysFromMonday).Date;
        Citas = new ObservableCollection<Cita>(citas);
        ActualizarCitasPorFecha();
    }

    /// <summary>
    /// Carga el calendario para el mes especificado (compatibilidad).
    /// </summary>
    /// <param name="mes">Mes a mostrar.</param>
    /// <param name="citas">Lista de citas para el mes.</param>
    public void CargarMes(DateTimeOffset mes, IEnumerable<Cita> citas)
    {
        var fechaDateTime = mes.DateTime;
        var daysFromMonday = ((int)fechaDateTime.DayOfWeek + 6) % 7;
        SemanaActual = fechaDateTime.AddDays(-daysFromMonday).Date;
        Citas = new ObservableCollection<Cita>(citas);
        ActualizarCitasPorFecha();
    }

    /// <summary>
    /// Obtiene el número de citas para una fecha específica.
    /// </summary>
    public int ObtenerNumeroCitas(DateTime fecha)
    {
        return CitasPorFecha.TryGetValue(fecha.Date, out var count) ? count : 0;
    }

    /// <summary>
    /// Indica si una fecha tiene citas.
    /// </summary>
    public bool TieneCitas(DateTime fecha)
    {
        return CitasPorFecha.ContainsKey(fecha.Date);
    }

    #endregion

    #region Métodos Privados

    /// <summary>
    /// Actualiza el diccionario de citas por fecha.
    /// </summary>
    private void ActualizarCitasPorFecha()
    {
        CitasPorFecha.Clear();
        foreach (var cita in Citas)
        {
            var fecha = cita.Fecha.Date;
            if (CitasPorFecha.ContainsKey(fecha))
            {
                CitasPorFecha[fecha]++;
            }
            else
            {
                CitasPorFecha[fecha] = 1;
            }
        }
        OnPropertyChanged(nameof(CitasPorFecha));
    }

    /// <summary>
    /// Se ejecuta cuando cambian las citas.
    /// </summary>
    partial void OnCitasChanged(ObservableCollection<Cita> value)
    {
        ActualizarCitasPorFecha();
    }

    #endregion
}

