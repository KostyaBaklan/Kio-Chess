using Analysis.Core.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Analysis.KioUI.Converters;

/// <summary>
/// Converts MoveClassification enum to Color (string) from resources.
/// </summary>
public class MoveClassificationToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not MoveClassification classification)
            return "Transparent";

        var resourceKey = classification switch
        {
            MoveClassification.Brilliant => "BrilliantColor",
            MoveClassification.Best => "BestColor",
            MoveClassification.Excellent => "ExcellentColor",
            MoveClassification.Good => "GoodColor",
            MoveClassification.Book => "BookColor",
            MoveClassification.Inaccuracy => "InaccuracyColor",
            MoveClassification.Mistake => "MistakeColor",
            MoveClassification.Blunder => "BlunderColor",
            _ => null
        };

        if (resourceKey == null)
            return "Transparent";

        return Application.Current.TryFindResource(resourceKey) ?? "Transparent";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
