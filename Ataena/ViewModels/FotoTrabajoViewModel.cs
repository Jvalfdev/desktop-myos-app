using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ataena.Data;
using Ataena.Models;
using Ataena.Services;
using Serilog;
using Microsoft.EntityFrameworkCore;

namespace Ataena.ViewModels;

/// <summary>
/// ViewModel para gestionar la captura de fotos (antes/después) de un trabajo mediante QR y móvil.
/// </summary>
public partial class FotoTrabajoViewModel : ViewModelBase
{
    private readonly AtaenaDbContext _db;
    private readonly FirmaWebService _firmaWebService = FirmaWebService.InstanciaCompartida;

    private string? _tokenActual;
    private Trabajo? _trabajo;
    private bool _esAntes;

    /// <summary>
    /// Evento que se dispara cuando se guarda una foto exitosamente.
    /// </summary>
    public event EventHandler<Trabajo>? FotoGuardada;

    public FotoTrabajoViewModel(AtaenaDbContext dbContext)
    {
        _db = dbContext;
    }

    #region Propiedades

    [ObservableProperty]
    private bool _esVisible;

    [ObservableProperty]
    private string _tituloModal = "📸 Foto del trabajo";

    [ObservableProperty]
    private string _estadoConexion = "⏳ Esperando conexión...";

    [ObservableProperty]
    private bool _estaProcesando;

    [ObservableProperty]
    private string _urlFoto = string.Empty;

    [ObservableProperty]
    private Bitmap? _qrCodeImage;

    [ObservableProperty]
    private Bitmap? _previewFotoAntes;

    [ObservableProperty]
    private Bitmap? _previewFotoDespues;

    [ObservableProperty]
    private string _mensajeError = string.Empty;

    #endregion

    /// <summary>
    /// Abre el modal para capturar/ver fotos de un trabajo.
    /// </summary>
    public async Task AbrirModalAsync(Trabajo trabajo, bool esAntes)
    {
        try
        {
            _trabajo = trabajo;
            _esAntes = esAntes;

            TituloModal = esAntes ? "➕ Añadir foto ANTES del trabajo" : "➕ Añadir foto DESPUÉS del trabajo";
            MensajeError = string.Empty;
            EstadoConexion = "⏳ Preparando captura...";

            // Cargar previews existentes (si hay rutas guardadas)
            await CargarPreviewsAsync();

            // Iniciar servidor y generar QR
            await IniciarCapturaAsync();

            await Dispatcher.UIThread.InvokeAsync(() => EsVisible = true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al abrir modal de fotos de trabajo");
            MensajeError = $"Error al abrir el modal de fotos: {ex.Message}";
        }
    }

    /// <summary>
    /// Carga en memoria las imágenes existentes para mostrarlas como preview.
    /// </summary>
    private Task CargarPreviewsAsync()
    {
        try
        {
            PreviewFotoAntes = CargarBitmapDesdeRuta(_trabajo?.FotoAntesPath);
            PreviewFotoDespues = CargarBitmapDesdeRuta(_trabajo?.FotoDespuesPath);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error al cargar previews de fotos de trabajo");
        }

        return Task.CompletedTask;
    }

    private static Bitmap? CargarBitmapDesdeRuta(string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta) || !File.Exists(ruta))
            return null;

        try
        {
            using var stream = File.OpenRead(ruta);
            return new Bitmap(stream);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error al cargar bitmap desde ruta: {Ruta}", ruta);
            return null;
        }
    }

    /// <summary>
    /// Inicia el servidor HTTP y genera el QR para captura de foto.
    /// </summary>
    private async Task IniciarCapturaAsync()
    {
        if (_trabajo == null)
        {
            MensajeError = "No hay trabajo seleccionado para capturar foto.";
            return;
        }

        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                EstaProcesando = true;
                EstadoConexion = "🔄 Iniciando servidor...";
            });

            var servidorIniciado = await _firmaWebService.IniciarServidor().ConfigureAwait(false);
            if (!servidorIniciado)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    EstadoConexion = "❌ Error al iniciar servidor. Verifica permisos.";
                    EstaProcesando = false;
                });
                return;
            }

            _tokenActual = FirmaWebService.GenerarTokenUnico();
            _firmaWebService.RegistrarToken(_tokenActual);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                UrlFoto = _firmaWebService.GenerarUrlFoto(_tokenActual);
                QrCodeImage = QRCodeService.GenerarQRCode(UrlFoto, 300);
                EstadoConexion = "✅ Servidor activo - Escanea el código QR con tu móvil para hacer la foto";
            });

            _ = Task.Run(async () => await EsperarFotoAsync().ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al iniciar captura de foto");
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                EstadoConexion = "❌ Error al iniciar la captura de foto";
                EstaProcesando = false;
            });
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() => EstaProcesando = false);
    }

    /// <summary>
    /// Espera la recepción de la foto, la guarda en disco y actualiza el trabajo.
    /// </summary>
    private async Task EsperarFotoAsync()
    {
        if (string.IsNullOrEmpty(_tokenActual) || _trabajo == null)
        {
            Log.Warning("EsperarFotoAsync: Token o trabajo es null. Token: {Token}, Trabajo: {TrabajoId}", 
                _tokenActual, _trabajo?.Id);
            return;
        }

        Log.Information("Esperando foto para token: {Token}, Trabajo: {TrabajoId}, EsAntes: {EsAntes}", 
            _tokenActual, _trabajo.Id, _esAntes);

        try
        {
            var imagenBase64 = await _firmaWebService.EsperarFirma(_tokenActual, TimeSpan.FromMinutes(5));
            if (string.IsNullOrEmpty(imagenBase64))
            {
                Log.Warning("No se recibió imagen para el token de foto {Token} o expiró el tiempo", _tokenActual);
                EstadoConexion = "⏱️ Tiempo de espera agotado. Intenta de nuevo.";
                return;
            }

            Log.Information("Foto recibida para token {Token}, tamaño base64: {Tamaño} caracteres",
                _tokenActual, imagenBase64.Length);

            await Dispatcher.UIThread.InvokeAsync(() => EstadoConexion = "📥 Recibiendo foto...");

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(imagenBase64);
                Log.Information("Imagen decodificada correctamente, tamaño: {Tamaño} bytes", bytes.Length);
            }
            catch (FormatException ex)
            {
                Log.Error(ex, "Error al decodificar base64. Primeros 100 caracteres: {Preview}",
                    imagenBase64.Length > 100 ? imagenBase64.Substring(0, 100) : imagenBase64);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    MensajeError = "Error: El formato de la imagen no es válido. Intenta con otra foto.";
                    EstadoConexion = "❌ Error al procesar la imagen";
                });
                return;
            }

            var guardado = await GuardarFotoEnTrabajoAsync(bytes).ConfigureAwait(false);
            if (guardado == null)
                return;

            await FinalizarTrasFotoGuardadaAsync(guardado, "✅ Foto recibida y guardada correctamente")
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al recibir/guardar foto de trabajo. Token: {Token}, Trabajo: {TrabajoId}", 
                _tokenActual, _trabajo?.Id);
            MensajeError = $"Error al guardar la foto: {ex.Message}";
            EstadoConexion = "❌ Error al guardar la foto";
        }
    }

    /// <summary>
    /// Selecciona una imagen del disco y la guarda como foto antes/después del trabajo.
    /// </summary>
    [RelayCommand]
    private async Task SubirDesdeOrdenadorAsync()
    {
        if (_trabajo == null)
        {
            MensajeError = "No hay trabajo seleccionado.";
            return;
        }

        try
        {
            var topLevel = Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (topLevel == null)
            {
                MensajeError = "No se pudo abrir el selector de archivos.";
                return;
            }

            var tipo = _esAntes ? "ANTES" : "DESPUÉS";
            var archivos = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = $"Seleccionar foto {tipo} del trabajo",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Imágenes")
                    {
                        Patterns = ["*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif", "*.webp"]
                    }
                ]
            });

            if (archivos == null || archivos.Count == 0)
                return;

            EstaProcesando = true;
            EstadoConexion = "📥 Procesando imagen...";
            MensajeError = string.Empty;

            await using var stream = await archivos[0].OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();

            if (bytes.Length == 0)
            {
                MensajeError = "El archivo seleccionado está vacío.";
                EstadoConexion = "❌ Error al subir la foto";
                return;
            }

            var guardado = await GuardarFotoEnTrabajoAsync(bytes);
            if (guardado == null)
                return;

            await FinalizarTrasFotoGuardadaAsync(guardado, "✅ Foto guardada desde el PC");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error al subir foto de trabajo desde ordenador");
            MensajeError = $"Error al subir la foto: {ex.Message}";
            EstadoConexion = "❌ Error al subir la foto";
        }
        finally
        {
            EstaProcesando = false;
        }
    }

    /// <summary>
    /// Escribe la imagen en disco y actualiza el trabajo en base de datos.
    /// </summary>
    private async Task<Trabajo?> GuardarFotoEnTrabajoAsync(byte[] bytes)
    {
        if (_trabajo == null)
            return null;

        var ruta = _esAntes
            ? ConsentimientoPathService.ObtenerRutaFotoAntes(_trabajo.ClienteId, _trabajo.Id)
            : ConsentimientoPathService.ObtenerRutaFotoDespues(_trabajo.ClienteId, _trabajo.Id);

        var directorio = Path.GetDirectoryName(ruta);
        if (!string.IsNullOrEmpty(directorio))
            Directory.CreateDirectory(directorio);

        await File.WriteAllBytesAsync(ruta, bytes);
        Log.Information("Foto de trabajo guardada en: {Ruta}, {Tamaño} bytes", ruta, bytes.Length);

        var trabajoDb = await _db.Trabajos.FirstOrDefaultAsync(t => t.Id == _trabajo.Id);
        if (trabajoDb == null)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                MensajeError = "Error: No se encontró el trabajo en la base de datos.";
                EstadoConexion = "❌ Error al actualizar el trabajo";
            });
            return null;
        }

        if (_esAntes)
            trabajoDb.FotoAntesPath = ruta;
        else
            trabajoDb.FotoDespuesPath = ruta;

        await _db.SaveChangesAsync();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_esAntes)
            {
                _trabajo.FotoAntesPath = ruta;
                PreviewFotoAntes = CargarBitmapDesdeRuta(ruta);
                OnPropertyChanged(nameof(PreviewFotoAntes));
            }
            else
            {
                _trabajo.FotoDespuesPath = ruta;
                PreviewFotoDespues = CargarBitmapDesdeRuta(ruta);
                OnPropertyChanged(nameof(PreviewFotoDespues));
            }
        });

        return trabajoDb;
    }

    private async Task FinalizarTrasFotoGuardadaAsync(Trabajo trabajoDb, string mensajeExito)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            EstadoConexion = mensajeExito;
            MensajeError = string.Empty;
            FotoGuardada?.Invoke(this, trabajoDb);
        });

        await Task.Delay(1500);
        await Dispatcher.UIThread.InvokeAsync(Cerrar);
    }

    [RelayCommand]
    private void Cerrar()
    {
        EsVisible = false;
        _firmaWebService.DetenerServidor();
        _tokenActual = null;
    }
}


