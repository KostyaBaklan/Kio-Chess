using Analysis.Core.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Analysis.KioUI.Converters;

/// <summary>
/// Converts MoveClassification enum to the corresponding icon geometry from resources.
/// </summary>
public class MoveClassificationToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not MoveClassification classification)
            return DependencyProperty.UnsetValue;

        var resourceKey = classification switch
        {
            MoveClassification.Brilliant => "BrilliantIcon",
            MoveClassification.Best => "BestIcon",
            MoveClassification.Excellent => "ExcellentIcon",
            MoveClassification.Good => "GoodIcon",
            MoveClassification.Book => "BookIcon",
            MoveClassification.Inaccuracy => "InaccuracyIcon",
            MoveClassification.Mistake => "MistakeIcon",
            MoveClassification.Blunder => "BlunderIcon",
            _ => null
        };

        if (resourceKey == null)
            return DependencyProperty.UnsetValue;

        return Application.Current.TryFindResource(resourceKey) ?? DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
