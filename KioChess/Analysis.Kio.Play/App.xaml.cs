using Analysis.Kio.Play.ViewModels;
using Analysis.Kio.Play.Views;
using Analysis.UI.Common;
using System.Windows;

namespace Analysis.Kio.Play;

/// <summary>
/// Play application entry point.
/// Provides interactive chess play against Stockfish engine.
/// </summary>
public partial class App : AnalysisApp
{
    protected override Window CreateShell()
    {
        var window = new AnalysisWindow();
        window.AppTitle = "Kio Chess Play";
        window.MainView = new PlayView();
        return window;
    }

    protected override void RegisterAppTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<PlayViewModel>();
    }
}
