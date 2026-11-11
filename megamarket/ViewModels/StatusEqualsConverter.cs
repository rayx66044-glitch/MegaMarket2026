using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace megamarket.ViewModels;

public class StatusEqualsConverter : IValueConverter
{
    public static readonly StatusEqualsConverter Draft = new(1);        
    public static readonly StatusEqualsConverter OnModeration = new(2); 
    public static readonly StatusEqualsConverter Active = new(3);       

    private readonly int _targetStatus;
    private StatusEqualsConverter(int targetStatus) => _targetStatus = targetStatus;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int statusId && statusId == _targetStatus;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}