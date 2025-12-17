using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using InkStudio.Models;
using InkStudio.ViewModels;

namespace InkStudio.Views;

/// <summary>
/// Vista de calendario visual para mostrar el mes con citas usando CalendarControl.Avalonia de NuGet.
/// Muestra las citas en el calendario semanal y permite hacer clic en ellas para editarlas.
/// </summary>
public partial class CalendarView : UserControl
{
    /// <summary>
    /// Inicializa el componente.
    /// </summary>
    public CalendarView()
    {
        InitializeComponent();
        
        // Suscribirse a eventos del CalendarControl para detectar clics en citas
        Loaded += OnLoaded;
    }
    
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Interceptar eventos de clic en el CalendarControl para detectar clics en citas
        if (CalendarControl != null)
        {
            CalendarControl.PointerPressed += OnCalendarControlPointerPressed;
        }
    }
    
    /// <summary>
    /// Maneja el clic en el CalendarControl para detectar si se hizo clic en una cita.
    /// </summary>
    private void OnCalendarControlPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Buscar el AppointmentControl que fue clickeado
        var source = e.Source;
        if (source == null) return;
        
        // Buscar el AppointmentControl en el árbol visual
        var appointmentControl = FindAppointmentControl(source);
        if (appointmentControl != null)
        {
            // Obtener la cita del DataContext del AppointmentControl
            var cita = appointmentControl.DataContext as Cita;
            if (cita != null)
            {
                // Buscar el AgendaViewModel en el árbol visual
                var agendaVM = FindAgendaViewModel();
                if (agendaVM != null)
                {
                    // Establecer la cita seleccionada y abrir el formulario de edición
                    agendaVM.CitaSeleccionada = cita;
                    agendaVM.EditarCitaCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }
    }
    
    /// <summary>
    /// Busca un AppointmentControl en el árbol visual a partir de un elemento.
    /// </summary>
    private Control? FindAppointmentControl(object? element)
    {
        if (element == null) return null;
        
        // Verificar si el elemento es un AppointmentControl
        var typeName = element.GetType().Name;
        if (typeName.Contains("Appointment", StringComparison.OrdinalIgnoreCase))
        {
            return element as Control;
        }
        
        // Si no, buscar en el árbol visual hacia arriba
        if (element is Control control)
        {
            var parent = control.Parent;
            while (parent != null)
            {
                var parentTypeName = parent.GetType().Name;
                if (parentTypeName.Contains("Appointment", StringComparison.OrdinalIgnoreCase))
                {
                    return parent as Control;
                }
                parent = parent.Parent;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Busca el AgendaViewModel en el árbol visual.
    /// </summary>
    private AgendaViewModel? FindAgendaViewModel()
    {
        // Buscar el AgendaView padre en el árbol visual
        var parent = this.Parent;
        while (parent != null)
        {
            if (parent is AgendaView agendaView && agendaView.DataContext is AgendaViewModel vm)
            {
                return vm;
            }
            parent = parent.Parent;
        }
        
        return null;
    }
}
