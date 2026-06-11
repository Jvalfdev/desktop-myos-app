using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using Serilog;

namespace Ataena.Services;

/// <summary>
/// Recorta, endereza y mejora fotos de DNI para archivo legible.
/// Fase 1 de la línea de desarrollo DNI (sin OCR).
/// </summary>
public static class DniImagenService
{
    private const int MaxLadoDeteccion = 1200;
    private const int AnchoSalida = 1200;
    private const double RelacionDni = 85.6 / 53.98; // ISO/IEC 7810 ID-1
    private const double AreaMinimaRelativa = 0.08;

    /// <summary>
    /// Resultado del procesado de una imagen de DNI.
    /// </summary>
    public sealed class ResultadoProcesado
    {
        public required byte[] BytesJpeg { get; init; }
        public bool RecorteDocumentoAplicado { get; init; }
        public string? AvisoUsuario { get; init; }
    }

    /// <summary>
    /// Procesa la imagen en un hilo de fondo (OpenCV es CPU-bound).
    /// </summary>
    public static Task<ResultadoProcesado> ProcesarAsync(byte[] imagenEntrada, CancellationToken cancellationToken = default)
        => Task.Run(() => Procesar(imagenEntrada), cancellationToken);

    /// <summary>
    /// Detecta el documento, corrige perspectiva y mejora contraste.
    /// Si no detecta bordes, guarda la imagen completa mejorada.
    /// </summary>
    public static ResultadoProcesado Procesar(byte[] imagenEntrada)
    {
        if (imagenEntrada is not { Length: > 0 })
            throw new ArgumentException("La imagen está vacía.", nameof(imagenEntrada));

        using var original = Cv2.ImDecode(imagenEntrada, ImreadModes.Color);
        if (original.Empty())
            throw new InvalidOperationException("No se pudo decodificar la imagen.");

        Mat? recortada = null;
        try
        {
            recortada = IntentarRecortarDocumento(original);
            var salida = recortada ?? original.Clone();

            MejorarLegibilidad(salida);

            var parametros = new[] { (int)ImwriteFlags.JpegQuality, 92 };
            if (!Cv2.ImEncode(".jpg", salida, out var jpeg, parametros) || !EsJpegValido(jpeg))
                throw new InvalidOperationException("No se pudo generar el JPEG procesado.");

            var recorteOk = recortada != null;
            Log.Information(
                "DNI procesado: {Ancho}x{Alto}, recorte={Recorte}, entrada={EntradaKb}KB, salida={SalidaKb}KB",
                salida.Width, salida.Height, recorteOk,
                imagenEntrada.Length / 1024, jpeg.Length / 1024);

            return new ResultadoProcesado
            {
                BytesJpeg = jpeg,
                RecorteDocumentoAplicado = recorteOk,
                AvisoUsuario = recorteOk
                    ? null
                    : "No se detectó el borde del DNI con claridad. Se guardó la imagen mejorada; puedes repetir la foto sobre una superficie plana."
            };
        }
        finally
        {
            recortada?.Dispose();
        }
    }

    private static Mat? IntentarRecortarDocumento(Mat original)
    {
        var escala = 1.0;
        using var paraDeteccion = RedimensionarSiGrande(original, MaxLadoDeteccion, out escala);
        var esquinas = DetectarCuadrilateroDocumento(paraDeteccion);
        if (esquinas == null)
            return null;

        var esquinasOriginales = esquinas
            .Select(p => new Point2f((float)(p.X * escala), (float)(p.Y * escala)))
            .ToArray();

        esquinasOriginales = OrdenarEsquinas(esquinasOriginales);

        var altoSalida = (int)Math.Round(AnchoSalida / RelacionDni);
        var destino = new[]
        {
            new Point2f(0, 0),
            new Point2f(AnchoSalida - 1, 0),
            new Point2f(AnchoSalida - 1, altoSalida - 1),
            new Point2f(0, altoSalida - 1)
        };

        using var matriz = Cv2.GetPerspectiveTransform(esquinasOriginales, destino);
        var warped = new Mat();
        Cv2.WarpPerspective(original, warped, matriz, new Size(AnchoSalida, altoSalida));
        return warped;
    }

    private static Mat RedimensionarSiGrande(Mat src, int maxLado, out double escala)
    {
        var maxActual = Math.Max(src.Width, src.Height);
        if (maxActual <= maxLado)
        {
            escala = 1.0;
            return src.Clone();
        }

        escala = maxActual / (double)maxLado;
        var nuevoAncho = (int)Math.Round(src.Width / escala);
        var nuevoAlto = (int)Math.Round(src.Height / escala);
        var resized = new Mat();
        Cv2.Resize(src, resized, new Size(nuevoAncho, nuevoAlto));
        return resized;
    }

    private static Point[]? DetectarCuadrilateroDocumento(Mat imagen)
    {
        using var gris = new Mat();
        Cv2.CvtColor(imagen, gris, ColorConversionCodes.BGR2GRAY);
        Cv2.GaussianBlur(gris, gris, new Size(5, 5), 0);

        using var bordes = new Mat();
        Cv2.Canny(gris, bordes, 50, 150);

        using var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
        Cv2.Dilate(bordes, bordes, kernel);

        Cv2.FindContours(bordes, out var contornos, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
        if (contornos.Length == 0)
            return null;

        var areaImagen = imagen.Width * imagen.Height;
        var candidatos = contornos
            .OrderByDescending(c => Cv2.ContourArea(c))
            .Take(12);

        foreach (var contorno in candidatos)
        {
            var area = Cv2.ContourArea(contorno);
            if (area < areaImagen * AreaMinimaRelativa)
                continue;

            var perimetro = Cv2.ArcLength(contorno, true);
            var aprox = Cv2.ApproxPolyDP(contorno, 0.02 * perimetro, true);
            if (aprox.Length != 4)
                continue;

            if (!Cv2.IsContourConvex(aprox))
                continue;

            var puntos = aprox.Select(p => new Point(p.X, p.Y)).ToArray();
            var ancho = Math.Max(
                Distancia(puntos[0], puntos[1]),
                Distancia(puntos[2], puntos[3]));
            var alto = Math.Max(
                Distancia(puntos[1], puntos[2]),
                Distancia(puntos[3], puntos[0]));

            if (ancho < 1 || alto < 1)
                continue;

            var ratio = ancho / alto;
            if (ratio < 1.25 || ratio > 2.2)
                continue;

            return puntos;
        }

        return null;
    }

    private static void MejorarLegibilidad(Mat imagen)
    {
        using var lab = new Mat();
        Cv2.CvtColor(imagen, lab, ColorConversionCodes.BGR2Lab);
        Cv2.Split(lab, out var canales);

        try
        {
            using var clahe = Cv2.CreateCLAHE(2.0, new Size(8, 8));
            clahe.Apply(canales[0], canales[0]);
            Cv2.Merge(canales, lab);
            Cv2.CvtColor(lab, imagen, ColorConversionCodes.Lab2BGR);
        }
        finally
        {
            foreach (var c in canales)
                c.Dispose();
        }

        // Ligero enfoque
        using var suavizada = new Mat();
        Cv2.GaussianBlur(imagen, suavizada, new Size(0, 0), 3);
        Cv2.AddWeighted(imagen, 1.4, suavizada, -0.4, 0, imagen);
    }

    private static Point2f[] OrdenarEsquinas(Point2f[] puntos)
    {
        if (puntos.Length != 4)
            throw new ArgumentException("Se esperaban 4 esquinas.");

        var porY = puntos.OrderBy(p => p.Y).ToArray();
        var arriba = porY.Take(2).OrderBy(p => p.X).ToArray();
        var abajo = porY.Skip(2).OrderBy(p => p.X).ToArray();
        return [arriba[0], arriba[1], abajo[1], abajo[0]];
    }

    private static double Distancia(Point a, Point b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static bool EsJpegValido(byte[] datos) =>
        datos.Length >= 4 &&
        datos[0] == 0xFF && datos[1] == 0xD8 && datos[2] == 0xFF;
}
