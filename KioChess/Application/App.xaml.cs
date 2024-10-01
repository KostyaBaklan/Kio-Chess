using System.Windows;
using Application.Interfaces;
using Application.Services;
using DataAccess.Interfaces;
using Engine.Dal.Interfaces;
using Kgb.ChessApp.Views;
using Prism.Ioc;
using Prism.Navigation.Regions;
using UI.Common;

namespace Kgb.ChessApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : UiApp
{
    protected override Window CreateShell()
    {
        var regionManager = ContainerLocator.Current.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion("Main", typeof(StartView));
        regionManager.RegisterViewWithRegion("Main", typeof(GameView));

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

        gameDbservice.LoadAsync();
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
        containerRegistry.RegisterSingleton(typeof(IStrategyProvider), typeof(StrategyProvider));

        containerRegistry.RegisterSingleton(typeof(StartViewModel));
        containerRegistry.RegisterSingleton(typeof(GameViewModel));
    }
}
