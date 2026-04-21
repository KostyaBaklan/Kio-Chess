using Analysis.Core.Interfaces;
using Analysis.Core.Models;
using Analysis.UI.Common.Services;
using System.Windows;
using System.Windows.Controls;

namespace Analysis.UI.Common;

/// <summary>
/// Base window class for all Analysis.Kio.* applications.
/// Provides common header, theme selector, status bar, and content area.
/// </summary>
public partial class AnalysisWindow : Window
{
    private ThemeService _themeService;
    private bool _isInitializing = true;

    public AnalysisWindow()
    {
        InitializeComponent();
        Loaded += OnWindowLoaded;
    }

    /// <summary>
    /// Sets the application title shown in the header.
    /// </summary>
    public string AppTitle
    {
        get => TitleText.Text;
        set
        {
            TitleText.Text = value;
            Title = value;
        }
    }

    /// <summary>
    /// Sets the main content of the window.
    /// </summary>
    public UIElement MainView
    {
        get => (UIElement)MainContent.Content;
        set => MainContent.Content = value;
    }

    /// <summary>
    /// Updates the status bar text.
    /// </summary>
    public void SetStatus(string status) => StatusText.Text = status;

    /// <summary>
    /// Updates the engine status text.
    /// </summary>
    public void SetEngineStatus(string status) => EngineStatusText.Text = status;

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        _isInitializing = true;

        // Initialize theme selector
        _themeService = ContainerLocator.Current.Resolve<ThemeService>();
        var settings = ContainerLocator.Current.Resolve<ISettingsService>();

        ThemeComboBox.ItemsSource = ThemeInfo.All;

        // Set selected item based on the saved theme in settings
        var currentTheme = ThemeInfo.All.FirstOrDefault(t => t.Key == settings.Current.Theme)
                           ?? ThemeInfo.All[0];
        ThemeComboBox.SelectedItem = currentTheme;

        _isInitializing = false;
    }

    private void OnThemeChanged(object sender, SelectionChangedEventArgs e)
    {
        // Don't save during initialization
        if (_isInitializing || _themeService == null) return;

        if (ThemeComboBox.SelectedItem is ThemeInfo theme)
        {
            _themeService.Apply(theme);

            // Save to settings only if actually changed by user
            var settings = ContainerLocator.Current.Resolve<ISettingsService>();
            if (settings.Current.Theme != theme.Key)
            {
                settings.Current.Theme = theme.Key;
                settings.Save();
            }
        }
    }

    private void OnSettingsClicked(object sender, RoutedEventArgs e)
    {
        try
        {
            var settingsDialog = new Views.SettingsDialog
            {
                Owner = this
            };

            if (settingsDialog.ShowDialog() == true)
            {
                // Settings were saved, reload them
                var settings = ContainerLocator.Current.Resolve<ISettingsService>();
                settings.Load();

                // Update theme combo box to reflect any changes
                if (_themeService != null)
                {
                    ThemeComboBox.SelectedItem = _themeService.ActiveTheme;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error opening settings:\n{ex.Message}",
                "Settings Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}