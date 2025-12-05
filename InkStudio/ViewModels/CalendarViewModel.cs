using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InkStudio.Models;

namespace InkStudio.ViewModels;

/// <summary>
/// ViewModel para el control de calendario visual.
/// Muestra un mes completo con los días y las citas marcadas.
/// </summary>
public partial class CalendarViewModel : ViewModelBase
{
    #region Propiedades

    /// <summary>
    /// Fecha del mes actual mostrado.
    /// </summary>
    [ObservableProperty]
    private DateTimeOffset _mesActual = DateTimeOffset.Now.Date;

    /// <summary>
    /// Días del mes para mostrar en el calendario.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DiaCalendario> _dias = new();

    /// <summary>
    /// Lista de citas para el mes actual.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Cita> _citas = new();

    /// <summary>
    /// Día seleccionado en el calendario.
    /// </summary>
    [ObservableProperty]
    private DiaCalendario? _diaSeleccionado;

    #endregion

    #region Comandos

    /// <summary>
    /// Navega al mes anterior.
    /// </summary>
    [RelayCommand]
    private void MesAnterior()
    {
        MesActual = MesActual.AddMonths(-1);
        GenerarDiasDelMes();
    }

    /// <summary>
    /// Navega al mes siguiente.
    /// </summary>
    [RelayCommand]
    private void MesSiguiente()
    {
        MesActual = MesActual.AddMonths(1);
        GenerarDiasDelMes();
    }

    /// <summary>
    /// Vuelve al mes actual.
    /// </summary>
    [RelayCommand]
    private void IrAMesActual()
    {
        MesActual = DateTimeOffset.Now.Date;
        var primerDia = new DateTimeOffset(MesActual.Year, MesActual.Month, 1, 0, 0, 0, MesActual.Offset);
        MesActual = primerDia;
        GenerarDiasDelMes();
    }

    #endregion

    #region Métodos Públicos

    /// <summary>
    /// Carga el calendario para el mes especificado.
    /// </summary>
    /// <param name="mes">Mes a mostrar.</param>
    /// <param name="citas">Lista de citas para el mes.</param>
    public void CargarMes(DateTimeOffset mes, IEnumerable<Cita> citas)
    {
        MesActual = new DateTimeOffset(mes.Year, mes.Month, 1, 0, 0, 0, mes.Offset);
        Citas = new ObservableCollection<Cita>(citas);

        GenerarDiasDelMes();
    }

    #endregion

    #region Métodos Privados

    /// <summary>
    /// Genera los días del mes para mostrar en el calendario.
    /// </summary>
    private void GenerarDiasDelMes()
    {
        Dias.Clear();

        var primerDia = new DateTime(MesActual.Year, MesActual.Month, 1);
        var ultimoDia = primerDia.AddMonths(1).AddDays(-1);

        // Días de la semana (L, M, X, J, V, S, D)
        var diasSemana = new[] { "L", "M", "X", "J", "V", "S", "D" };

        // Añadir encabezados de días de la semana
        for (int i = 0; i < 7; i++)
        {
            Dias.Add(new DiaCalendario
            {
                EsEncabezado = true,
                Texto = diasSemana[i],
                EsDomingo = i == 6
            });
        }

        // Calcular el primer día de la semana del mes (0 = Domingo, 1 = Lunes, etc.)
        // Convertir a Lunes = 0
        var primerDiaSemana = ((int)primerDia.DayOfWeek + 6) % 7;

        // Añadir días vacíos antes del primer día del mes
        for (int i = 0; i < primerDiaSemana; i++)
        {
            Dias.Add(new DiaCalendario { EsVacio = true });
        }

        // Añadir todos los días del mes
        for (int dia = 1; dia <= ultimoDia.Day; dia++)
        {
            var fecha = new DateTime(MesActual.Year, MesActual.Month, dia);
            var fechaOffset = new DateTimeOffset(fecha, MesActual.Offset);
            var esHoy = fecha.Date == DateTime.Today;
            var esDomingo = fecha.DayOfWeek == DayOfWeek.Sunday;

            // Contar citas del día
            var citasDelDia = Citas.Where(c => c.Fecha.Date == fecha.Date).ToList();
            var tieneCitas = citasDelDia.Any();

            Dias.Add(new DiaCalendario
            {
                Fecha = fechaOffset,
                NumeroDia = dia,
                EsHoy = esHoy,
                EsDomingo = esDomingo,
                TieneCitas = tieneCitas,
                NumeroCitas = citasDelDia.Count,
                Citas = citasDelDia
            });
        }

        // Añadir días vacíos al final para completar la cuadrícula (6 filas = 42 días)
        var diasTotales = Dias.Count(d => !d.EsEncabezado && !d.EsVacio);
        var diasVaciosNecesarios = 42 - (primerDiaSemana + diasTotales);
        for (int i = 0; i < diasVaciosNecesarios; i++)
        {
            Dias.Add(new DiaCalendario { EsVacio = true });
        }
    }

    #endregion
}

/// <summary>
/// Representa un día en el calendario.
/// </summary>
public class DiaCalendario
{
    /// <summary>
    /// Indica si es un encabezado de día de la semana.
    /// </summary>
    public bool EsEncabezado { get; set; }

    /// <summary>
    /// Indica si es un día vacío (fuera del mes).
    /// </summary>
    public bool EsVacio { get; set; }

    /// <summary>
    /// Indica si es un día del mes (no encabezado ni vacío).
    /// </summary>
    public bool EsDiaDelMes => !EsEncabezado && !EsVacio;

    /// <summary>
    /// Texto del encabezado (L, M, X, etc.).
    /// </summary>
    public string? Texto { get; set; }

    /// <summary>
    /// Fecha del día.
    /// </summary>
    public DateTimeOffset? Fecha { get; set; }

    /// <summary>
    /// Número del día.
    /// </summary>
    public int NumeroDia { get; set; }

    /// <summary>
    /// Indica si es el día de hoy.
    /// </summary>
    public bool EsHoy { get; set; }

    /// <summary>
    /// Indica si es domingo.
    /// </summary>
    public bool EsDomingo { get; set; }

    /// <summary>
    /// Indica si tiene citas.
    /// </summary>
    public bool TieneCitas { get; set; }

    /// <summary>
    /// Número de citas del día.
    /// </summary>
    public int NumeroCitas { get; set; }

    /// <summary>
    /// Lista de citas del día.
    /// </summary>
    public List<Cita> Citas { get; set; } = new();
}

