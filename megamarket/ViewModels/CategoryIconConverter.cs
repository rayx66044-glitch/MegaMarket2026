using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace megamarket.ViewModels;

public class CategoryIconConverter : IValueConverter
{
    public static readonly CategoryIconConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int categoryId)
        {
            return categoryId switch
            {
                1 => "📱", 
                4 => "👕", 
                7 => "🔌", 
                _ => "📱"  
            };
        }
        return "📱";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}