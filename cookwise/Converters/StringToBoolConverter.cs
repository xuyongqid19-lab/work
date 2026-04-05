using System.Globalization;
using Microsoft.Maui.Controls;

namespace cookwise.Converters;

/// <summary>
/// 将非空字符串转换为 true，空字符串或 null 转换为 false
/// </summary>
public class StringToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}