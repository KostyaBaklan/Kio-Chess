using Analysis.Core.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Analysis.KioUI.Converters;

/// <summary>
/// Converts MoveClassification enum to the corresponding text symbol from resources.
/// </summary>
public class MoveClassificationToSymbolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not MoveClassification classification)
            return string.Empty;

        var resourceKey = classification switch
        {
            MoveClassification.Brilliant => "BrilliantSymbol",
            MoveClassification.Best => "BestSymbol",
            MoveClassification.Excellent => "ExcellentSymbol",
            MoveClassification.Good => "GoodSymbol",
            MoveClassification.Book => "BookSymbol",
            MoveClassification.Inaccuracy => "InaccuracySymbol",
            MoveClassification.Mistake => "MistakeSymbol",
            MoveClassification.Blunder => "BlunderSymbol",
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
