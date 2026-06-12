using Analysis.Kio.OpeningsExplorer.ViewModels;
using Analysis.Kio.OpeningsExplorer.Views;
using Analysis.UI.Common;
using System.Windows;

namespace Analysis.Kio.OpeningsExplorer;

/// <summary>
/// Opening Explorer application entry point.
/// Provides ECO opening database exploration.
/// </summary>
public partial class App : AnalysisApp
{
    protected override Window CreateShell()
    {
        var window = new AnalysisWindow();
        window.AppTitle = "Kio Opening Explorer";
        window.MainView = new OpeningExplorerView();
        return window;
    }

    protected override void RegisterAppTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<OpeningExplorerViewModel>();
    }
}
