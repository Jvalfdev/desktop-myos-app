using Avalonia.Controls;
using Avalonia.Interactivity;
using InkStudio.ViewModels;

namespace InkStudio.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel vm)
        {
            await vm.CargarDatosCommand.ExecuteAsync(null);
        }
    }
}

