using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace megamarket.ViewModels;

public class StockToVisibilityConverter : IValueConverter
{
    public static readonly StockToVisibilityConverter InStock = new();
    public static readonly StockToVisibilityConverter OutOfStock = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int stock)
        {
            return this == InStock ? stock > 0 : stock <= 0;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}