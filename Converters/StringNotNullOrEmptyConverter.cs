using System.Globalization;

namespace zorgApp.Converters;

public class StringNotNullOrEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Returns true if value is NOT null and NOT empty
        if (value == null)
            return false;
        
        if (value is string str)
            return !string.IsNullOrWhiteSpace(str);
        
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}