using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Ataena.Converters;

/// <summary>
/// Borde rojo en campos con mensaje de error (chivato de validación).
/// </summary>
public class CampoErrorBorderBrushConverter : IValueConverter
{
    public static readonly CampoErrorBorderBrushConverter Instance = new();

    private static readonly SolidColorBrush Normal = new(Color.Parse("#374151"));
    private static readonly SolidColorBrush Error = new(Color.Parse("#ef5350"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.IsNullOrWhiteSpace(value as string) ? Normal : Error;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Grosor de borde según si el campo tiene error.
/// </summary>
public class CampoErrorBorderThicknessConverter : IValueConverter
{
    public static readonly CampoErrorBorderThicknessConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.IsNullOrWhiteSpace(value as string) ? new Thickness(1) : new Thickness(2);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
