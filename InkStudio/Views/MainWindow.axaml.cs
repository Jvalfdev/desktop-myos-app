using Avalonia.Controls;

namespace InkStudio.Views;

/// <summary>
/// Ventana principal de la aplicación InkStudio CRM.
/// </summary>
/// <remarks>
/// Contiene:
/// - Barra de navegación lateral
/// - Área de contenido principal (cambia según la sección)
/// 
/// Las vistas disponibles son:
/// - Dashboard (inicio)
/// - Clientes (gestión de clientes)
/// - Agenda (calendario de citas)
/// - Trabajos (galería de trabajos)
/// - Configuración (ajustes)
/// </remarks>
public partial class MainWindow : Window
{
    /// <summary>
    /// Inicializa la ventana principal.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }
}
