using System.Windows;
using System.Windows.Input;

namespace Analysis.KioUI.Behaviors;

/// <summary>
/// Attached behavior for custom window chrome with draggable title bar.
/// </summary>
public static class WindowChromeBehavior
{
    public static readonly DependencyProperty EnableCustomChromeProperty =
        DependencyProperty.RegisterAttached(
            "EnableCustomChrome",
            typeof(bool),
            typeof(WindowChromeBehavior),
            new PropertyMetadata(false, OnEnableCustomChromeChanged));

    public static bool GetEnableCustomChrome(DependencyObject obj)
        => (bool)obj.GetValue(EnableCustomChromeProperty);

    public static void SetEnableCustomChrome(DependencyObject obj, bool value)
        => obj.SetValue(EnableCustomChromeProperty, value);

    private static void OnEnableCustomChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window) return;

        if ((bool)e.NewValue)
        {
            window.StateChanged += OnStateChanged;
            window.Loaded += OnWindowLoaded;
        }
        else
        {
            window.StateChanged -= OnStateChanged;
            window.Loaded -= OnWindowLoaded;
        }
    }

    private static void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        var window = (Window)sender;
        
        // Find title bar and wire up drag
        if (window.Template?.FindName("TitleBar", window) is UIElement titleBar)
        {
            titleBar.MouseLeftButtonDown += (s, args) =>
            {
                if (args.ClickCount == 2)
                {
                    // Double-click to maximize/restore
                    window.WindowState = window.WindowState == WindowState.Maximized
                        ? WindowState.Normal
                        : WindowState.Maximized;
                }
                else if (args.LeftButton == MouseButtonState.Pressed)
                {
                    // Single click to drag
                    window.DragMove();
                }
            };
        }

        // Wire up window control buttons
        WireButton(window, "MinimizeButton", () => window.WindowState = WindowState.Minimized);
        WireButton(window, "MaximizeButton", () => window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);
        WireButton(window, "CloseButton", () => window.Close());
    }

    private static void WireButton(Window window, string buttonName, Action action)
    {
        if (window.Template?.FindName(buttonName, window) is System.Windows.Controls.Button button)
        {
            button.Click += (s, e) => action();
        }
    }

    private static void OnStateChanged(object sender, EventArgs e)
    {
        var window = (Window)sender!;
        
        // Update maximize button icon
        if (window.Template?.FindName("MaximizeIcon", window) is System.Windows.Shapes.Path icon)
        {
            icon.Data = window.WindowState == WindowState.Maximized
                ? System.Windows.Media.Geometry.Parse("M0,3 L7,3 L7,10 L0,10 Z M3,0 L10,0 L10,7")
                : System.Windows.Media.Geometry.Parse("M0,0 L10,0 L10,10 L0,10 Z");
        }
    }
}
