using Analysis.Core.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Analysis.KioUI.Converters;

/// <summary>
/// Converts MoveClassification enum to the corresponding text label from resources.
/// </summary>
public class MoveClassificationToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not MoveClassification classification)
            return string.Empty;

        var resourceKey = classification switch
        {
            MoveClassification.Brilliant => "BrilliantText",
            MoveClassification.Best => "BestText",
            MoveClassification.Excellent => "ExcellentText",
            MoveClassification.Good => "GoodText",
            MoveClassification.Book => "BookText",
            MoveClassification.Inaccuracy => "InaccuracyText",
            MoveClassification.Mistake => "MistakeText",
            MoveClassification.Blunder => "BlunderText",
            _ => null
        };

        if (resourceKey == null)
            return string.Empty;

        return Application.Current.TryFindResource(resourceKey) ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
