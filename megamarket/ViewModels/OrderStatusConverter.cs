using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace megamarket.ViewModels;

public class OrderStatusConverter : IValueConverter
{
    public static readonly OrderStatusConverter InTransitBrush = new();
    public static readonly OrderStatusConverter DeliveredBrush = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int statusId) return Brushes.LightGray;

        if (this == InTransitBrush)
            return statusId >= 4 ? Brushes.Blue : Brushes.LightGray;
        
        if (this == DeliveredBrush)
            
            return (statusId == 5 || statusId == 7) ? Brushes.Green : Brushes.LightGray;

        return Brushes.LightGray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}