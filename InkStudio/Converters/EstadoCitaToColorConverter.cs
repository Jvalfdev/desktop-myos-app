using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using InkStudio.Models;

namespace InkStudio.ViewModels;

public class EstadoCitaToColorConverter : IMultiValueConverter
{
    public static readonly EstadoCitaToColorConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count > 0 && values[0] is EstadoCita estado)
        {
            return estado switch
            {
                EstadoCita.Pendiente => new SolidColorBrush(Color.Parse("#ffa726")),    // Naranja
                EstadoCita.Confirmada => new SolidColorBrush(Color.Parse("#66bb6a")),   // Verde
                EstadoCita.EnProceso => new SolidColorBrush(Color.Parse("#42a5f5")),    // Azul
                EstadoCita.Completada => new SolidColorBrush(Color.Parse("#78909c")),   // Gris
                EstadoCita.Cancelada => new SolidColorBrush(Color.Parse("#ef5350")),    // Rojo
                EstadoCita.NoShow => new SolidColorBrush(Color.Parse("#ab47bc")),       // Morado
                _ => new SolidColorBrush(Color.Parse("#9e9e9e"))
            };
        }
        return new SolidColorBrush(Color.Parse("#9e9e9e"));
    }
}

