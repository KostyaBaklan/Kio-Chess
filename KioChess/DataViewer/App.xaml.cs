using Prism.Ioc;
using System.Windows;
using DataViewer.Views;
using UI.Common;
using Prism.Navigation.Regions;

namespace DataViewer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : UiApp
{
    protected override bool ShouldConnectToDb => false;

    protected override Window CreateShell()
    {
        var regionManager = ContainerLocator.Current.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion("Main", typeof(DataView));
        //regionManager.RegisterViewWithRegion("Main", typeof(GameView));

        return new Shell();
    }

    protected override void RegisterLocalTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton(typeof(DataViewModel));
    }
}
