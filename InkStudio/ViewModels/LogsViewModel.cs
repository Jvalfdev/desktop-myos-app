using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InkStudio.Services;
using Serilog;

namespace InkStudio.ViewModels;

/// <summary>
/// ViewModel para la visualización y gestión de logs.
/// Permite ver logs recientes y exportarlos.
/// </summary>
public partial class LogsViewModel : ViewModelBase
{
    #region Propiedades

    /// <summary>
    /// Contenido del log actual mostrado.
    /// </summary>
    [ObservableProperty]
    private string _contenidoLog = string.Empty;

    /// <summary>
    /// Lista de archivos de log disponibles.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _archivosLog = new();

    /// <summary>
    /// Archivo de log seleccionado.
    /// </summary>
    [ObservableProperty]
    private string? _archivoSeleccionado;

    /// <summary>
    /// Indica si se está cargando el log.
    /// </summary>
    [ObservableProperty]
    private bool _cargando = false;

    /// <summary>
    /// Mensaje de estado o error.
    /// </summary>
    [ObservableProperty]
    private string _mensaje = string.Empty;

    /// <summary>
    /// Ruta de la carpeta de logs.
    /// </summary>
    [ObservableProperty]
    private string _rutaCarpetaLogs = string.Empty;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa el ViewModel y carga los logs disponibles.
    /// </summary>
    public LogsViewModel()
    {
        RutaCarpetaLogs = LoggingService.LogsFolder;
        CargarArchivosLog();
    }

    #endregion

    #region Comandos

    /// <summary>
    /// Carga la lista de archivos de log disponibles.
    /// </summary>
    [RelayCommand]
    private void CargarArchivosLog()
    {
        try
        {
            ArchivosLog.Clear();
            Mensaje = string.Empty;

            if (!Directory.Exists(LoggingService.LogsFolder))
            {
                Mensaje = "La carpeta de logs no existe";
                return;
            }

            var archivos = Directory.GetFiles(LoggingService.LogsFolder, "inkstudio-*.log")
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .Select(f => Path.GetFileName(f))
                .ToList();

            foreach (var archivo in archivos)
            {
                ArchivosLog.Add(archivo);
            }

            // Seleccionar el más reciente por defecto
            if (archivos.Count > 0)
            {
                ArchivoSeleccionado = archivos[0];
                CargarLogCommand.Execute(null);
            }

            Log.Information("Archivos de log cargados: {Count} archivos", archivos.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al cargar archivos de log");
            Mensaje = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Carga el contenido del log seleccionado.
    /// </summary>
    [RelayCommand]
    private void CargarLog()
    {
        if (string.IsNullOrEmpty(ArchivoSeleccionado))
            return;

        try
        {
            Cargando = true;
            Mensaje = string.Empty;

            var rutaCompleta = Path.Combine(LoggingService.LogsFolder, ArchivoSeleccionado);

            if (!File.Exists(rutaCompleta))
            {
                Mensaje = "El archivo de log no existe";
                return;
            }

            // Leer las últimas 1000 líneas para no sobrecargar la UI
            var lineas = File.ReadAllLines(rutaCompleta);
            var lineasMostrar = lineas.Length > 1000 
                ? lineas.Skip(lineas.Length - 1000).ToArray() 
                : lineas;

            ContenidoLog = string.Join(Environment.NewLine, lineasMostrar);

            if (lineas.Length > 1000)
            {
                Mensaje = $"Mostrando últimas 1000 líneas de {lineas.Length} totales";
            }
            else
            {
                Mensaje = $"Log cargado: {lineas.Length} líneas";
            }

            Log.Debug("Log cargado: {Archivo}, {Lineas} líneas", ArchivoSeleccionado, lineas.Length);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al cargar el log: {Archivo}", ArchivoSeleccionado);
            Mensaje = $"Error al cargar el log: {ex.Message}";
            ContenidoLog = string.Empty;
        }
        finally
        {
            Cargando = false;
        }
    }

    /// <summary>
    /// Copia el contenido del log al portapapeles.
    /// </summary>
    [RelayCommand]
    private async Task CopiarLog()
    {
        try
        {
            if (string.IsNullOrEmpty(ContenidoLog))
            {
                Mensaje = "No hay contenido para copiar";
                return;
            }

            // TODO: Implementar copia al portapapeles en Avalonia
            // Por ahora, guardamos en un archivo temporal
            var tempFile = Path.Combine(Path.GetTempPath(), $"inkstudio-log-{DateTime.Now:yyyyMMddHHmmss}.txt");
            await File.WriteAllTextAsync(tempFile, ContenidoLog);
            
            Mensaje = $"Log copiado a: {tempFile}";
            Log.Information("Log copiado a archivo temporal: {Archivo}", tempFile);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al copiar el log");
            Mensaje = $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Abre la carpeta de logs en el explorador de archivos.
    /// </summary>
    [RelayCommand]
    private void AbrirCarpetaLogs()
    {
        LoggingService.AbrirCarpetaLogs();
        Mensaje = "Carpeta de logs abierta";
    }

    /// <summary>
    /// Exporta el log actual a un archivo seleccionado por el usuario.
    /// </summary>
    [RelayCommand]
    private Task ExportarLog()
    {
        // TODO: Implementar diálogo de guardar archivo en Avalonia
        // Por ahora, usamos la carpeta de documentos
        try
        {
            if (string.IsNullOrEmpty(ArchivoSeleccionado))
            {
                Mensaje = "No hay log seleccionado";
                return Task.CompletedTask;
            }

            var documentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var nombreArchivo = $"inkstudio-log-export-{DateTime.Now:yyyyMMddHHmmss}.txt";
            var rutaDestino = Path.Combine(documentos, nombreArchivo);

            var rutaOrigen = Path.Combine(LoggingService.LogsFolder, ArchivoSeleccionado);
            File.Copy(rutaOrigen, rutaDestino, true);

            Mensaje = $"Log exportado a: {rutaDestino}";
            Log.Information("Log exportado: {Archivo}", rutaDestino);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al exportar el log");
            Mensaje = $"Error: {ex.Message}";
        }
        
        return Task.CompletedTask;
    }

    #endregion
}

