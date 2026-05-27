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
            var openingService = ContainerLocator.Current.Resolve<IOpeningService>();
            openingService.Connect();

            // Connect GamesService
            var gamesService = ContainerLocator.Current.Resolve<IGamesService>();
            gamesService.Connect();

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
            var gamesService = ContainerLocator.Current.Resolve<IGamesService>();
            gamesService.Disconnect();

            var service = ContainerLocator.Current.Resolve<IGameDbService>();

            service.Disconnect();

            var openingDbservice = ContainerLocator.Current.Resolve<IOpeningDbService>();

            openingDbservice.Disconnect();

            var localDbservice = ContainerLocator.Current.Resolve<ILocalDbService>();

            localDbservice.Disconnect();
            
            var appDbService = ContainerLocator.Current.Resolve<IAppDbService>();
            appDbService.Disconnect();
        }

        protected override void RegisterLocalTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton(typeof(IStrategyProvider), typeof(StrategyProvider));

            containerRegistry.RegisterSingleton(typeof(StartViewModel));
            containerRegistry.RegisterSingleton(typeof(GameViewModel));
        }
    }

}
