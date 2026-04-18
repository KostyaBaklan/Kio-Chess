using Analysis.Core.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Analysis.KioUI.Converters;

/// <summary>
/// Converts MoveClassification enum to icon geometry path data (string) from resources.
/// </summary>
public class MoveClassificationToIconPathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not MoveClassification classification)
            return string.Empty;

        var resourceKey = classification switch
        {
            MoveClassification.Brilliant => "BrilliantIconPath",
            MoveClassification.Best => "BestIconPath",
            MoveClassification.Excellent => "ExcellentIconPath",
            MoveClassification.Good => "GoodIconPath",
            MoveClassification.Book => "BookIconPath",
            MoveClassification.Inaccuracy => "InaccuracyIconPath",
            MoveClassification.Mistake => "MistakeIconPath",
            MoveClassification.Blunder => "BlunderIconPath",
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
