using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace InkStudio.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // ══════════════════════════════════════════════════════════════
    // VIEWMODELS DE CADA SECCIÓN
    // ══════════════════════════════════════════════════════════════
    
    public DashboardViewModel DashboardVM { get; } = new();
    
    // TODO: Añadir estos ViewModels cuando se creen las vistas
    // public ClientesViewModel ClientesVM { get; } = new();
    // public AgendaViewModel AgendaVM { get; } = new();
    // public TrabajosViewModel TrabajosVM { get; } = new();
    // public ConfiguracionViewModel ConfiguracionVM { get; } = new();

    // ══════════════════════════════════════════════════════════════
    // NAVEGACIÓN
    // ══════════════════════════════════════════════════════════════
    
    [ObservableProperty]
    private string _vistaActual = "Dashboard";

    [RelayCommand]
    private void IrADashboard()
    {
        VistaActual = "Dashboard";
    }

    [RelayCommand]
    private void IrAClientes()
    {
        VistaActual = "Clientes";
        // TODO: Implementar navegación real
    }

    [RelayCommand]
    private void IrAAgenda()
    {
        VistaActual = "Agenda";
        // TODO: Implementar navegación real
    }

    [RelayCommand]
    private void IrATrabajos()
    {
        VistaActual = "Trabajos";
        // TODO: Implementar navegación real
    }

    [RelayCommand]
    private void IrAConfiguracion()
    {
        VistaActual = "Configuracion";
        // TODO: Implementar navegación real
    }
}
