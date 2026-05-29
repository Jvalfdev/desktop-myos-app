using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Ataena.Models;
using Ataena.ViewModels;

namespace Ataena.Views;

/// <summary>
/// Vista para la gestión de trabajos (tatuajes y piercings).
/// </summary>
public partial class TrabajosView : UserControl
{
    private TrabajosViewModel? _trabajosVm;
    private bool _permitirAbrirDesplegableCliente;

    public TrabajosView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;

        FormularioTrabajoOverlay?.AddHandler(
            InputElement.PointerWheelChangedEvent,
            OnFormularioTrabajoPointerWheel,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
            handledEventsToo: true);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_trabajosVm != null)
            _trabajosVm.PropertyChanged -= TrabajosVmOnPropertyChanged;

        _trabajosVm = DataContext as TrabajosViewModel;
        if (_trabajosVm != null)
            _trabajosVm.PropertyChanged += TrabajosVmOnPropertyChanged;
    }

    private void TrabajosVmOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(TrabajosViewModel.MostrarFormulario) ||
            _trabajosVm is not { MostrarFormulario: true })
        {
            return;
        }

        Dispatcher.UIThread.Post(PrepararModalTrabajoAlAbrir, DispatcherPriority.Loaded);
    }

    private void PrepararModalTrabajoAlAbrir()
    {
        if (_trabajosVm?.MostrarFormulario != true)
            return;

        _permitirAbrirDesplegableCliente = false;
        ClienteTrabajoComboBox!.IsDropDownOpen = false;

        TopLevel.GetTopLevel(this)?.FocusManager?.ClearFocus();

        // Tras el layout: evitar desplegable abierto al cargar cliente en edición
        Dispatcher.UIThread.Post(() =>
        {
            ClienteTrabajoComboBox.IsDropDownOpen = false;
            _permitirAbrirDesplegableCliente = true;
        }, DispatcherPriority.Background);
    }

    private void OnFormularioTrabajoOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_trabajosVm?.MostrarFormulario != true)
            return;

        // Clic en el fondo oscuro: activar la capa del modal (sin robar clics a botones/campos)
        if (ReferenceEquals(e.Source, FormularioTrabajoOverlay))
            FormularioTrabajoScrollViewer?.Focus();
    }

    private void OnFormularioTrabajoPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        if (_trabajosVm?.MostrarFormulario != true || FormularioTrabajoScrollViewer is not { } scroll)
            return;

        if (ClienteTrabajoComboBox?.IsDropDownOpen == true)
            return;

        var maxY = Math.Max(0, scroll.Extent.Height - scroll.Viewport.Height);
        var step = e.Delta.Y * 48;
        var newY = Math.Clamp(scroll.Offset.Y - step, 0, maxY);

        scroll.Offset = new Vector(scroll.Offset.X, newY);
        e.Handled = true;
    }

    private void OnClienteTrabajoComboDropDownOpened(object? sender, EventArgs e)
    {
        if (_permitirAbrirDesplegableCliente || sender is not ComboBox combo)
            return;

        combo.IsDropDownOpen = false;
    }

    /// <summary>
    /// Se ejecuta cuando se carga el control.
    /// </summary>
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is TrabajosViewModel vm)
        {
            _ = vm.CargarTrabajosCommand.ExecuteAsync(null);
            _ = vm.CargarClientesCommand.ExecuteAsync(null);
        }
    }

    /// <summary>
    /// Maneja el clic en una tarjeta de trabajo.
    /// </summary>
    private void OnTrabajoClick(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.Tag is Trabajo trabajo)
        {
            if (DataContext is TrabajosViewModel vm)
            {
                vm.TrabajoSeleccionado = trabajo;
                vm.EditarTrabajoCommand.Execute(null);
            }
        }
    }

    /// <summary>
    /// Maneja el clic en el botón de firmar consentimiento desde el DataTemplate.
    /// </summary>
    private void OnFirmarConsentimientoClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Trabajo trabajo)
        {
            if (DataContext is TrabajosViewModel vm)
            {
                vm.FirmarConsentimientoTrabajoCommand.Execute(trabajo);
            }
        }
    }

    /// <summary>
    /// Abre el desplegable solo si el usuario escribe en el buscador (no al cargar datos en edición).
    /// </summary>
    private void OnClienteBusquedaTrabajoTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (ClienteTrabajoComboBox == null)
            return;

        var q = ClienteBusquedaTrabajoTextBox?.Text?.Trim();
        if (string.IsNullOrEmpty(q))
        {
            ClienteTrabajoComboBox.IsDropDownOpen = false;
            return;
        }

        if (ClienteBusquedaTrabajoTextBox?.IsFocused != true)
        {
            ClienteTrabajoComboBox.IsDropDownOpen = false;
            return;
        }

        if (DataContext is not TrabajosViewModel vm || vm.ClientesFiltradosFormulario.Count == 0)
        {
            ClienteTrabajoComboBox.IsDropDownOpen = false;
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (ClienteTrabajoComboBox is null ||
                vm.ClientesFiltradosFormulario.Count == 0 ||
                ClienteBusquedaTrabajoTextBox?.IsFocused != true)
            {
                return;
            }

            _permitirAbrirDesplegableCliente = true;
            ClienteTrabajoComboBox.IsDropDownOpen = true;
        }, DispatcherPriority.Input);
    }
}
