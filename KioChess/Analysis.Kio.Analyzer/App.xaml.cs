using Analysis.Kio.Analyzer.ViewModels;
using Analysis.Kio.Analyzer.Views;
using Analysis.UI.Common;
using System.Windows;

namespace Analysis.Kio.Analyzer;

/// <summary>
/// Analyzer application entry point.
/// Provides game analysis with engine evaluation.
/// </summary>
public partial class App : AnalysisApp
{
    protected override Window CreateShell()
    {
        var window = new AnalysisWindow();
        window.AppTitle = "Kio Chess Analyzer";
        window.MainView = new AnalyseView();
        return window;
    }

    protected override void RegisterAppTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<AnalyseViewModel>();
    }
}
