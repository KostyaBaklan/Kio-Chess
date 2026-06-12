using DataAccess.Interfaces;
using Engine.Dal.Interfaces;
using Engine.Models.Hash;
using KioChess.App.Interfaces;
using KioChess.App.Services;
using KioChess.App.Views;
using System.Windows;
using UI.Common;

namespace KioChess.App
{
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
            var appDbService = ContainerLocator.Current.Resolve<IAppDbService>();
            appDbService.Connect();
            var hash = appDbService.GetAllMoveHashValues();
            MoveHashSequenceHasher.Initialize(hash);

            // Connect OpeningService
            ContainerLocator.Current.Resolve<IOpeningService>().Connect();

            // Connect GamesService
            ContainerLocator.Current.Resolve<IGamesService>().Connect();

            var cacheLoader = ContainerLocator.Current.Resolve<ICacheLoaderService>();
            cacheLoader.LoadAsync();
        }

        protected override void DbDisconnect()
        {
            ContainerLocator.Current.Resolve<IGamesService>().Disconnect();

            ContainerLocator.Current.Resolve<IAppDbService>().Disconnect();

            ContainerLocator.Current.Resolve<IOpeningService>().Disconnect();
        }

        protected override void RegisterLocalTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton(typeof(IStrategyProvider), typeof(StrategyProvider));

            containerRegistry.RegisterSingleton(typeof(StartViewModel));
            containerRegistry.RegisterSingleton(typeof(GameViewModel));
        }
    }

}
