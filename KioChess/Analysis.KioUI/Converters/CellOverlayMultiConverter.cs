using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Analysis.KioUI.Converters;

/// <summary>
/// Returns an overlay brush for a cell based on its current highlight state.
/// Priority: Check > Selected > ValidTarget > LastMoveTo > LastMoveFrom > Transparent
/// </summary>
public class CellOverlayMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 5) return Brushes.Transparent;

        bool isInCheck     = values[0] is bool b0 && b0;
        bool isSelected    = values[1] is bool b1 && b1;
        bool isValidTarget = values[2] is bool b2 && b2;
        bool isLastMoveTo  = values[3] is bool b3 && b3;
        bool isLastMoveFrom = values[4] is bool b4 && b4;

        var res = System.Windows.Application.Current.Resources;

        if (isInCheck)     return res["BoardCheckBrush"]       as Brush ?? Brushes.Transparent;
        if (isSelected)    return res["BoardSelectedBrush"]    as Brush ?? Brushes.Transparent;
        if (isValidTarget) return res["BoardValidTargetBrush"] as Brush ?? Brushes.Transparent;
        if (isLastMoveTo)  return res["BoardLastMoveBrush"]    as Brush ?? Brushes.Transparent;
        if (isLastMoveFrom) return res["BoardLastMoveBrush"]   as Brush ?? Brushes.Transparent;

        return Brushes.Transparent;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
