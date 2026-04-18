using Analysis.Core.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Analysis.KioUI.Converters;

/// <summary>
/// Converts MoveClassification.None to Visibility.Collapsed, all others to Visibility.Visible.
/// </summary>
public class MoveClassificationToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not MoveClassification classification)
            return Visibility.Collapsed;

        return classification == MoveClassification.None 
            ? Visibility.Collapsed 
            : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
