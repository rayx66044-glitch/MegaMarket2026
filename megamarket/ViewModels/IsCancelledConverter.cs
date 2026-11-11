using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace megamarket.ViewModels;

public class IsCancelledConverter : IValueConverter
{
    public static readonly IsCancelledConverter IsCancelled = new();
    public static readonly IsCancelledConverter IsNotCancelled = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int statusId)
        {
            
            if (this == IsCancelled) return statusId == 6;
            if (this == IsNotCancelled) return statusId != 6;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}