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
        // Initialize theme selector
        _themeService = ContainerLocator.Current.Resolve<ThemeService>();
        ThemeComboBox.ItemsSource = ThemeInfo.All;
        ThemeComboBox.SelectedItem = _themeService.ActiveTheme;
    }

    private void OnThemeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_themeService == null) return;
        if (ThemeComboBox.SelectedItem is ThemeInfo theme)
        {
            _themeService.Apply(theme);
            
            // Save to settings
            var settings = ContainerLocator.Current.Resolve<ISettingsService>();
            if (settings.Current.Theme != theme.Key)
            {
                settings.Current.Theme = theme.Key;
                settings.Save(); 
            }
        }
    }
}
