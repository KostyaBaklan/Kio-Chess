using Prism.Ioc;
using System.Windows;
using DataViewer.Views;
using UI.Common;
using Prism.Navigation.Regions;
using DataAccess.Interfaces;
using Engine.Dal.Interfaces;

namespace DataViewer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : UiApp
{
    protected override Window CreateShell()
    {
        var regionManager = ContainerLocator.Current.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion("Main", typeof(DataView));
        //regionManager.RegisterViewWithRegion("Main", typeof(GameView));

        return new Shell();
    }

    protected override void DbConnect()
    {
        var gameDbservice = ContainerLocator.Current.Resolve<IGameDbService>();

        gameDbservice.Connect();

        var openingDbservice = ContainerLocator.Current.Resolve<IOpeningDbService>();

        openingDbservice.Connect();

        var localDbservice = ContainerLocator.Current.Resolve<ILocalDbService>();

        localDbservice.Connect();
    }

    protected override void DbDisconnect()
    {
        var service = ContainerLocator.Current.Resolve<IGameDbService>();

        service.Disconnect();

        var openingDbservice = ContainerLocator.Current.Resolve<IOpeningDbService>();

        openingDbservice.Disconnect();

        var localDbservice = ContainerLocator.Current.Resolve<ILocalDbService>();

        localDbservice.Disconnect();
    }

    protected override void RegisterLocalTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton(typeof(DataViewModel));
    }
}
