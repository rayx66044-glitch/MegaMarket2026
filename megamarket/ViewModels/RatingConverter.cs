using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace megamarket.ViewModels;

public class RatingConverter : IValueConverter
{
    public static readonly RatingConverter Instance = new();

    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int rating)
        {
            return rating - 1; 
        }
        return 4; 
    }

    
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index)
        {
            return index + 1;
        }   
        return 5;
    }
}