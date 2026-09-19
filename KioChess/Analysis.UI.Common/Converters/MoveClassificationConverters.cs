using Analysis.Core.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Analysis.UI.Common.Converters;

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
