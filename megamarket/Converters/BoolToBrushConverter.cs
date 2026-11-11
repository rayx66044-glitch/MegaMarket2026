using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace megamarket.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public static readonly BoolToBrushConverter TrueIsGreen = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isTrue && isTrue) return Brushes.Green;
        return Brushes.Red;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}