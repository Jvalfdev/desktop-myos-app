using System;
using System.Diagnostics;
using System.IO;
using Serilog;

namespace Ataena.Services;

/// <summary>
/// Abre archivos con el programa predeterminado del sistema (visor de imágenes, PDF, etc.).
/// </summary>
public static class ArchivoSistemaService
{
    /// <summary>
    /// Abre un archivo en el visor predeterminado de Windows.
    /// </summary>
    public static void AbrirEnVisorPredeterminado(string ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta) || !File.Exists(ruta))
            throw new FileNotFoundException("No se encontró el archivo.", ruta);

        var rutaAbsoluta = Path.GetFullPath(ruta);

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = rutaAbsoluta,
                UseShellExecute = true,
                Verb = "open"
            });
            return;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "No se pudo abrir con ShellExecute directo: {Ruta}", rutaAbsoluta);
        }

        if (!OperatingSystem.IsWindows())
            throw new InvalidOperationException("No hay visor predeterminado disponible en este sistema.");

        // Fallback fiable en Windows cuando falla la asociación directa
        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c start \"\" \"{rutaAbsoluta}\"",
            CreateNoWindow = true,
            UseShellExecute = false
        });
    }
}
