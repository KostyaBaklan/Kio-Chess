using System.Windows;
using System.Windows.Media;

namespace Analysis.UI.Common.Views;

/// <summary>
/// A themed notification dialog that replaces MessageBox for all Analysis applications.
/// </summary>
public partial class NotificationDialog : Window
{
    private NotificationResult _result = NotificationResult.Primary;

    public NotificationDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Shows an information notification.
    /// </summary>
    public static void ShowInfo(string message, string title = "Information", Window owner = null)
    {
        Show(message, title, NotificationType.Info, NotificationButtons.Ok, owner);
    }

    /// <summary>
    /// Shows a warning notification.
    /// </summary>
    public static void ShowWarning(string message, string title = "Warning", Window owner = null)
    {
        Show(message, title, NotificationType.Warning, NotificationButtons.Ok, owner);
    }

    /// <summary>
    /// Shows an error notification.
    /// </summary>
    public static void ShowError(string message, string title = "Error", Window owner = null)
    {
        Show(message, title, NotificationType.Error, NotificationButtons.Ok, owner);
    }

    /// <summary>
    /// Shows a success notification.
    /// </summary>
    public static void ShowSuccess(string message, string title = "Success", Window owner = null)
    {
        Show(message, title, NotificationType.Success, NotificationButtons.Ok, owner);
    }

    /// <summary>
    /// Shows a confirmation dialog and returns the result.
    /// </summary>
    public static NotificationResult ShowConfirm(string message, string title = "Confirm", Window owner = null)
    {
        return Show(message, title, NotificationType.Question, NotificationButtons.YesNo, owner);
    }

    /// <summary>
    /// Shows a notification with full customization.
    /// </summary>
    public static NotificationResult Show(
        string message, 
        string title, 
        NotificationType type = NotificationType.Info,
        NotificationButtons buttons = NotificationButtons.Ok,
        Window owner = null)
    {
        var dialog = new NotificationDialog();
        dialog.Owner = owner ?? Application.Current.MainWindow;
        dialog.Title = title;
        dialog.TitleText.Text = title;
        dialog.MessageText.Text = message;

        // Set icon and color based on type
        ConfigureType(dialog, type);
        
        // Configure buttons
        ConfigureButtons(dialog, buttons);

        dialog.ShowDialog();
        return dialog._result;
    }

    private static void ConfigureType(NotificationDialog dialog, NotificationType type)
    {
        Color iconColor;
        string iconPath;

        switch (type)
        {
            case NotificationType.Info:
                iconColor = Color.FromRgb(0x21, 0x96, 0xF3); // Blue
                iconPath = "M7,0 A7,7 0 1,0 7,14 A7,7 0 1,0 7,0 M6,3 L8,3 L8,5 L6,5 Z M6,6 L8,6 L8,11 L6,11 Z";
                break;
            case NotificationType.Warning:
                iconColor = Color.FromRgb(0xF0, 0xC1, 0x5C); // Yellow/Orange
                iconPath = "M7,0 L14,12 L0,12 Z M6,4 L8,4 L8,8 L6,8 Z M6,9 L8,9 L8,11 L6,11 Z";
                break;
            case NotificationType.Error:
                iconColor = Color.FromRgb(0xCA, 0x34, 0x31); // Red
                iconPath = "M7,0 A7,7 0 1,0 7,14 A7,7 0 1,0 7,0 M3,4 L4,3 L7,6 L10,3 L11,4 L8,7 L11,10 L10,11 L7,8 L4,11 L3,10 L6,7 Z";
                break;
            case NotificationType.Success:
                iconColor = Color.FromRgb(0x4C, 0xAF, 0x50); // Green
                iconPath = "M7,0 A7,7 0 1,0 7,14 A7,7 0 1,0 7,0 M3,7 L5,9 L11,3 L12,4 L5,11 L2,8 Z";
                break;
            case NotificationType.Question:
                iconColor = Color.FromRgb(0x9C, 0x27, 0xB0); // Purple
                iconPath = "M7,0 A7,7 0 1,0 7,14 A7,7 0 1,0 7,0 M5,4 C5,2 9,2 9,4 C9,5 7,5 7,7 L7,8 L7,7 C7,5 9,5 9,4 C9,3 5,3 5,4 Z M6,10 L8,10 L8,12 L6,12 Z";
                break;
            default:
                iconColor = Color.FromRgb(0x75, 0x75, 0x75); // Gray
                iconPath = "M7,0 A7,7 0 1,0 7,14 A7,7 0 1,0 7,0";
                break;
        }

        dialog.IconBorder.Background = new SolidColorBrush(iconColor);
        dialog.IconPath.Data = Geometry.Parse(iconPath);
    }

    private static void ConfigureButtons(NotificationDialog dialog, NotificationButtons buttons)
    {
        switch (buttons)
        {
            case NotificationButtons.Ok:
                dialog.PrimaryButton.Content = "OK";
                dialog.SecondaryButton.Visibility = Visibility.Collapsed;
                break;
            case NotificationButtons.OkCancel:
                dialog.PrimaryButton.Content = "OK";
                dialog.SecondaryButton.Content = "Cancel";
                dialog.SecondaryButton.Visibility = Visibility.Visible;
                break;
            case NotificationButtons.YesNo:
                dialog.PrimaryButton.Content = "Yes";
                dialog.SecondaryButton.Content = "No";
                dialog.SecondaryButton.Visibility = Visibility.Visible;
                break;
            case NotificationButtons.YesNoCancel:
                dialog.PrimaryButton.Content = "Yes";
                dialog.SecondaryButton.Content = "No";
                dialog.SecondaryButton.Visibility = Visibility.Visible;
                // Note: Cancel can be done by closing the window
                break;
        }
    }

    private void OnPrimaryClick(object sender, RoutedEventArgs e)
    {
        _result = NotificationResult.Primary;
        DialogResult = true;
        Close();
    }

    private void OnSecondaryClick(object sender, RoutedEventArgs e)
    {
        _result = NotificationResult.Secondary;
        DialogResult = false;
        Close();
    }
}

/// <summary>
/// Type of notification to display.
/// </summary>
public enum NotificationType
{
    Info,
    Warning,
    Error,
    Success,
    Question
}

/// <summary>
/// Button configuration for the notification dialog.
/// </summary>
public enum NotificationButtons
{
    Ok,
    OkCancel,
    YesNo,
    YesNoCancel
}

/// <summary>
/// Result of the notification dialog.
/// </summary>
public enum NotificationResult
{
    Primary,    // OK or Yes
    Secondary,  // Cancel or No
    Closed      // Window was closed
}
