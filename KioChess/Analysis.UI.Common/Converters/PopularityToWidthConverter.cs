using System.Globalization;
using System.Windows.Data;

namespace Analysis.UI.Common.Converters;

/// <summary>
/// Converts popularity percentage (0-100) to width for a progress bar.
/// Can be used for any percentage-to-width conversion scenario.
/// </summary>
public class PopularityToWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2 ||
            values[0] is not int popularity ||
            values[1] is not double containerWidth)
        {
            return 0.0;
        }

        // Ensure popularity is between 0 and 100
        var normalizedPopularity = Math.Max(0, Math.Min(100, popularity));

        // Calculate width as percentage of container
        return (containerWidth * normalizedPopularity) / 100.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}