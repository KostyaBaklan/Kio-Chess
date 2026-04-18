using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Analysis.KioUI.Converters;

/// <summary>
/// Highlights matching text in search suggestions.
/// Takes two values: [0] = text to display, [1] = search query
/// </summary>
public class HighlightTextConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not string text || values[1] is not string query)
            return values[0];

        if (string.IsNullOrWhiteSpace(query))
            return text;

        var textBlock = new System.Windows.Controls.TextBlock
        {
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontSize = 12
        };
        
        // Try to get BodyTextStyle for consistency
        if (Application.Current.Resources["BodyTextStyle"] is Style bodyStyle)
        {
            textBlock.Style = bodyStyle;
        }

        var queryLower = query.ToLowerInvariant();
        var textLower = text.ToLowerInvariant();
        
        int currentIndex = 0;
        int matchIndex;

        while ((matchIndex = textLower.IndexOf(queryLower, currentIndex, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            // Add non-highlighted text before match
            if (matchIndex > currentIndex)
            {
                textBlock.Inlines.Add(new Run(text.Substring(currentIndex, matchIndex - currentIndex)));
            }

            // Add highlighted match
            var highlightedRun = new Run(text.Substring(matchIndex, query.Length))
            {
                FontWeight = FontWeights.Bold,
                Foreground = Application.Current.Resources["AccentBrush"] as Brush ?? Brushes.Orange
            };
            textBlock.Inlines.Add(highlightedRun);

            currentIndex = matchIndex + query.Length;
        }

        // Add remaining text after last match
        if (currentIndex < text.Length)
        {
            textBlock.Inlines.Add(new Run(text.Substring(currentIndex)));
        }

        return textBlock;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

