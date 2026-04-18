using Analysis.KioUI.Models;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Analysis.KioUI.Converters;

/// <summary>
/// Returns the light/dark square brush from the active theme.
/// Falls back to hardcoded values if the theme is not yet loaded.
/// </summary>
[ValueConversion(typeof(CellType), typeof(Brush))]
public class CellTypeToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is CellType ct)
        {
            var key = ct == CellType.Light ? "BoardLightSquareBrush" : "BoardDarkSquareBrush";
            if (System.Windows.Application.Current.Resources[key] is Brush b)
                return b;
            return ct == CellType.Light ? Brushes.AntiqueWhite : Brushes.SaddleBrown;
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
