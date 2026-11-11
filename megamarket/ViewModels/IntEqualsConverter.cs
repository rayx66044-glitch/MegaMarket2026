using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace megamarket.ViewModels;

public class IntEqualsConverter : IValueConverter
{
    public static readonly IntEqualsConverter One = new(1);
    public static readonly IntEqualsConverter Two = new(2);
    public static readonly IntEqualsConverter Three = new(3);
    public static readonly IntEqualsConverter Four = new(4);
    public static readonly IntEqualsConverter Five = new(5);

    private readonly int _targetValue;
    private IntEqualsConverter(int targetValue) => _targetValue = targetValue;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int intValue && intValue == _targetValue;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool isChecked && isChecked ? _targetValue : Avalonia.Data.BindingOperations.DoNothing;
}