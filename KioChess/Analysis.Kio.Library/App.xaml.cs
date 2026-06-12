using Analysis.Kio.Library.ViewModels;
using Analysis.Kio.Library.Views;
using Analysis.UI.Common;
using System.Windows;

namespace Analysis.Kio.Library;

/// <summary>
/// Library application entry point.
/// Provides game statistics database browsing and opening exploration.
/// </summary>
public partial class App : AnalysisApp
{
    protected override Window CreateShell()
    {
        var window = new AnalysisWindow();
        window.AppTitle = "Kio Chess Library";
        window.MainView = new LibraryView();
        return window;
    }

    protected override void RegisterAppTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<LibraryViewModel>();
    }
}
