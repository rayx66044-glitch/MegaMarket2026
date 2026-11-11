using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace megamarket.ViewModels;

public class LowStockConverter : IValueConverter
{
    public static readonly LowStockConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int qty)
        {
            return qty < 10;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}