using System.Windows;
using System.Windows.Input;

namespace Analysis.KioUI.Services;

/// <summary>
/// Service for enhancing accessibility features including keyboard navigation,
/// screen reader support, and high contrast mode detection.
/// </summary>
public class AccessibilityService
{
    private readonly IServiceProvider _serviceProvider;

    public AccessibilityService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets whether Windows High Contrast mode is enabled.
    /// </summary>
    public bool IsHighContrastEnabled =>
        SystemParameters.HighContrast;

    /// <summary>
    /// Announces a message to screen readers.
    /// </summary>
    public void Announce(string message, bool isPolite = true)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        // Create an invisible TextBlock with LiveRegionChanged property
        Application.Current.Dispatcher.Invoke(() =>
        {
            var announcer = new System.Windows.Controls.TextBlock
            {
                Text = message,
                Visibility = Visibility.Collapsed
            };

            System.Windows.Automation.AutomationProperties.SetLiveSetting(
                announcer,
                isPolite 
                    ? System.Windows.Automation.AutomationLiveSetting.Polite 
                    : System.Windows.Automation.AutomationLiveSetting.Assertive
            );

            // Add to main window temporarily
            if (Application.Current.MainWindow?.Content is System.Windows.Controls.Panel panel)
            {
                panel.Children.Add(announcer);
                
                // Remove after announcement
                System.Windows.Threading.DispatcherTimer timer = new()
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                timer.Tick += (s, e) =>
                {
                    panel.Children.Remove(announcer);
                    timer.Stop();
                };
                timer.Start();
            }
        });
    }

    /// <summary>
    /// Announces a game event (move, capture, check, etc.) to screen readers.
    /// </summary>
    public void AnnounceGameEvent(string eventType, string details)
    {
        var message = eventType switch
        {
            "move" => $"Move played: {details}",
            "capture" => $"Capture: {details}",
            "check" => $"Check! {details}",
            "checkmate" => "Checkmate!",
            "stalemate" => "Stalemate - game drawn",
            "draw" => "Game drawn",
            "resign" => $"{details} resigned",
            _ => details
        };

        Announce(message, isPolite: false);
    }

    /// <summary>
    /// Sets up keyboard navigation for a board control.
    /// </summary>
    public void SetupBoardKeyboardNavigation(UIElement boardElement, 
        Action<int, int> onSquareSelected,
        Func<int, int, bool> isValidSquare)
    {
        int currentRow = 0;
        int currentCol = 0;
        bool hasSelection = false;

        boardElement.KeyDown += (s, e) =>
        {
            var handled = true;

            switch (e.Key)
            {
                case Key.Left:
                    if (currentCol > 0) currentCol--;
                    break;

                case Key.Right:
                    if (currentCol < 7) currentCol++;
                    break;

                case Key.Up:
                    if (currentRow > 0) currentRow--;
                    break;

                case Key.Down:
                    if (currentRow < 7) currentRow++;
                    break;

                case Key.Enter:
                case Key.Space:
                    if (isValidSquare(currentRow, currentCol))
                    {
                        onSquareSelected(currentRow, currentCol);
                        hasSelection = !hasSelection;
                        Announce($"Square {(char)('a' + currentCol)}{8 - currentRow} selected");
                    }
                    break;

                case Key.Escape:
                    hasSelection = false;
                    Announce("Selection cleared");
                    break;

                default:
                    handled = false;
                    break;
            }

            if (handled)
            {
                e.Handled = true;
                
                // Announce current square
                var square = $"{(char)('a' + currentCol)}{8 - currentRow}";
                Announce($"Current square: {square}", isPolite: true);
            }
        };

        // Make board focusable
        if (boardElement is System.Windows.Controls.Control control)
        {
            control.Focusable = true;
            System.Windows.Automation.AutomationProperties.SetName(control, "Chess Board");
            System.Windows.Automation.AutomationProperties.SetHelpText(
                control,
                "Use arrow keys to navigate, Enter or Space to select, Escape to cancel"
            );
        }
    }

    /// <summary>
    /// Applies high contrast theme if Windows High Contrast is enabled.
    /// </summary>
    public void ApplyHighContrastIfNeeded(Action<string> applyTheme)
    {
        if (IsHighContrastEnabled)
        {
            applyTheme("HighContrast");
            Announce("High contrast mode enabled");
        }
    }

    /// <summary>
    /// Sets accessibility properties on an element.
    /// </summary>
    public void SetAccessibilityProperties(UIElement element, string name, string helpText = "", string role = "")
    {
        System.Windows.Automation.AutomationProperties.SetName(element, name);
        
        if (!string.IsNullOrEmpty(helpText))
            System.Windows.Automation.AutomationProperties.SetHelpText(element, helpText);
        
        if (!string.IsNullOrEmpty(role))
            System.Windows.Automation.AutomationProperties.SetItemType(element, role);
    }

    /// <summary>
    /// Makes a notification visible to screen readers.
    /// </summary>
    public void MakeNotificationAccessible(UIElement notification, string message)
    {
        System.Windows.Automation.AutomationProperties.SetName(notification, "Notification");
        System.Windows.Automation.AutomationProperties.SetLiveSetting(
            notification,
            System.Windows.Automation.AutomationLiveSetting.Assertive
        );
        
        Announce(message, isPolite: false);
    }
}
