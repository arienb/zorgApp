using System.Globalization;

namespace zorgApp.Converters;

public class EditModeHeaderConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isEditMode)
            return isEditMode ? "Patiënt Bewerken" : "Nieuwe Patiënt Toevoegen";
        
        return "Nieuwe Patiënt Toevoegen";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}