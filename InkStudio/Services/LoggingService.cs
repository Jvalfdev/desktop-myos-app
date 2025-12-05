using System;
using System.IO;
using Serilog;
using Serilog.Events;

namespace InkStudio.Services;

/// <summary>
/// Servicio centralizado para logging de la aplicación.
/// Configura Serilog y proporciona métodos de utilidad.
/// </summary>
/// <remarks>
/// Los logs se guardan en: %LOCALAPPDATA%\InkStudio\logs\
/// Formato: inkstudio-YYYYMMDD.log
/// </remarks>
public static class LoggingService
{
    private static bool _inicializado = false;

    /// <summary>
    /// Ruta de la carpeta donde se guardan los logs.
    /// </summary>
    public static string LogsFolder { get; private set; } = string.Empty;

    /// <summary>
    /// Inicializa el sistema de logging.
    /// Debe llamarse al inicio de la aplicación.
    /// </summary>
    public static void Inicializar()
    {
        if (_inicializado)
            return;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        LogsFolder = Path.Combine(appData, "InkStudio", "logs");
        Directory.CreateDirectory(LogsFolder);

        var logFile = Path.Combine(LogsFolder, "inkstudio-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: logFile,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30, // Mantener 30 días de logs
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                shared: true)
            .CreateLogger();

        _inicializado = true;
        Log.Information("═══════════════════════════════════════════════════════");
        Log.Information("InkStudio CRM iniciado - Sistema de logging activado");
        Log.Information("Carpeta de logs: {LogsFolder}", LogsFolder);
        Log.Information("═══════════════════════════════════════════════════════");
    }

    /// <summary>
    /// Cierra el sistema de logging correctamente.
    /// Debe llamarse al cerrar la aplicación.
    /// </summary>
    public static void Cerrar()
    {
        Log.Information("Aplicación cerrada");
        Log.CloseAndFlush();
    }

    /// <summary>
    /// Abre la carpeta de logs en el explorador de archivos.
    /// </summary>
    public static void AbrirCarpetaLogs()
    {
        try
        {
            if (Directory.Exists(LogsFolder))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = LogsFolder,
                    UseShellExecute = true,
                    Verb = "open"
                });
                Log.Information("Carpeta de logs abierta: {LogsFolder}", LogsFolder);
            }
            else
            {
                Log.Warning("La carpeta de logs no existe: {LogsFolder}", LogsFolder);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al abrir la carpeta de logs");
        }
    }

    /// <summary>
    /// Obtiene el archivo de log más reciente.
    /// </summary>
    /// <returns>Ruta del archivo de log más reciente, o null si no existe.</returns>
    public static string? ObtenerLogMasReciente()
    {
        try
        {
            if (!Directory.Exists(LogsFolder))
                return null;

            var archivos = Directory.GetFiles(LogsFolder, "inkstudio-*.log");
            if (archivos.Length == 0)
                return null;

            // Ordenar por fecha de modificación (más reciente primero)
            Array.Sort(archivos, (a, b) => 
                File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));

            return archivos[0];
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al obtener el log más reciente");
            return null;
        }
    }
}

