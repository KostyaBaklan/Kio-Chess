using Analysis.Core.Models;
using System.Windows;

namespace Analysis.KioUI.Services;

/// <summary>
/// Swaps the active theme ResourceDictionary at runtime without restarting.
/// All theme files live under Themes/{Key}/Theme.xaml as WPF Page resources.
/// </summary>
public class ThemeService
{
    // Tag placed on the merged dictionary so we can find and replace it
    private const string ThemeTag = "KioChessActiveTheme";

    public ThemeInfo ActiveTheme { get; private set; } = ThemeInfo.All[0];

    public void Apply(ThemeInfo theme)
    {
        var uri = new Uri($"pack://application:,,,/Analysis.KioUI;component/{theme.ResourcePath}",
                          UriKind.Absolute);

        var newDict = new ResourceDictionary { Source = uri };
        newDict[ThemeTag] = true;

        var appDicts = System.Windows.Application.Current.Resources.MergedDictionaries;

        // Remove the previous theme dictionary
        var existing = appDicts.FirstOrDefault(d => d.Contains(ThemeTag));
        if (existing != null)
            appDicts.Remove(existing);

        appDicts.Add(newDict);
        ActiveTheme = theme;
    }

    public void ApplyByKey(string key)
    {
        var theme = ThemeInfo.All.FirstOrDefault(t => t.Key == key);
        if (theme is not null)
            Apply(theme);
    }
}
