using System.Globalization;

namespace zorgApp.Converters;

public class DiaryItemEditModeTitleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isEditMode)
            return isEditMode ? "Dagboek Item Bewerken" : "Nieuw Dagboek Item";
        
        return "Nieuw Dagboek Item";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}