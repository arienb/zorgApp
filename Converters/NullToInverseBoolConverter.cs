using System.Globalization;

namespace zorgApp.Converters;

public class NullToInverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Returns true if value is null or empty string
        if (value == null)
            return true;
        
        if (value is string str)
            return string.IsNullOrWhiteSpace(str);
        
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}