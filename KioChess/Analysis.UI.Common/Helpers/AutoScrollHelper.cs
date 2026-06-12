using System.Windows;

namespace Analysis.UI.Common.Helpers;

/// <summary>
/// Attached behavior to enable automatic scrolling to bring an item into view.
/// </summary>
public static class AutoScrollHelper
{
    public static readonly DependencyProperty ScrollOnChangeProperty =
        DependencyProperty.RegisterAttached(
            "ScrollOnChange",
            typeof(bool),
            typeof(AutoScrollHelper),
            new PropertyMetadata(false, OnScrollOnChangeChanged));

    public static bool GetScrollOnChange(DependencyObject obj)
    {
        return (bool)obj.GetValue(ScrollOnChangeProperty);
    }

    public static void SetScrollOnChange(DependencyObject obj, bool value)
    {
        obj.SetValue(ScrollOnChangeProperty, value);
    }

    private static void OnScrollOnChangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element || (bool)e.NewValue == false)
            return;

        element.Loaded += (s, args) =>
        {
            element.BringIntoView();
        };
        
        if (element.IsLoaded)
        {
            element.BringIntoView();
        }
    }
}
