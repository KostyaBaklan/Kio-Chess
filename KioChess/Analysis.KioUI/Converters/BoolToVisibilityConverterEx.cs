using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Analysis.KioUI.Converters;

/// <summary>
/// Converts boolean to Visibility with support for inversion.
/// Parameter="Inverse" will invert the logic.
/// </summary>
public class BoolToVisibilityConverterEx : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isVisible = value is bool b && b;
        
        if (parameter is string param && param == "Inverse")
            isVisible = !isVisible;
        
        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isVisible = value is Visibility v && v == Visibility.Visible;
        
        if (parameter is string param && param == "Inverse")
            isVisible = !isVisible;
        
        return isVisible;
    }
}
