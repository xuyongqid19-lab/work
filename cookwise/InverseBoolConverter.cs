using System.Globalization;
using Microsoft.Maui.Controls;

namespace cookwise;

/// <summary>
/// 将 bool 值取反的转换器
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;
}

/// <summary>
/// 检查值是否不为 null（用于 int? 等可空类型）
/// </summary>
public class HasValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return false;
        if (value is int intValue) return intValue > 0;
        if (value is double doubleValue) return doubleValue > 0;
        if (value is string stringValue) return !string.IsNullOrWhiteSpace(stringValue);
        return true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
