using Analysis.Core.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Analysis.KioUI.Converters;

/// <summary>
/// Converts MoveClassification enum to the corresponding Brush from resources.
/// </summary>
public class MoveClassificationToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not MoveClassification classification)
            return DependencyProperty.UnsetValue;

        var resourceKey = classification switch
        {
            MoveClassification.Brilliant => "BrilliantMoveBrush",
            MoveClassification.Best => "BestMoveBrush",
            MoveClassification.Excellent => "ExcellentMoveBrush",
            MoveClassification.Good => "GoodMoveBrush",
            MoveClassification.Book => "BookMoveBrush",
            MoveClassification.Inaccuracy => "InaccuracyMoveBrush",
            MoveClassification.Mistake => "MistakeMoveBrush",
            MoveClassification.Blunder => "BlunderMoveBrush",
            _ => null
        };

        if (resourceKey == null)
            return Brushes.Transparent;

        return Application.Current.TryFindResource(resourceKey) ?? Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
