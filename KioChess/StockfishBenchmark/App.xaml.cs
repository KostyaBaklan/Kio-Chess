using DataAccess.Interfaces;
using Engine.Dal.Interfaces;
using Engine.Models.Hash;
using StockfishBenchmark.Services;
using StockfishBenchmark.ViewModels;
using System.Windows;
using UI.Common;

namespace StockfishBenchmark
{
    public partial class App : UiApp
    {
        protected override Window CreateShell()
        {
            return new MainWindow();
        }

        protected override void DbConnect()
        {
            var appDbService = ContainerLocator.Current.Resolve<IAppDbService>();
            appDbService.Connect();
            var hash = appDbService.GetAllMoveHashValues();
            MoveHashSequenceHasher.Initialize(hash);

            var cacheLoader = ContainerLocator.Current.Resolve<ICacheLoaderService>();
            cacheLoader.LoadAsync();
        }

        protected override void DbDisconnect()
        {
            ContainerLocator.Current.Resolve<IAppDbService>().Disconnect();
        }

        protected override void RegisterLocalTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton(typeof(IBenchmarkFileService), typeof(BenchmarkFileService));
            containerRegistry.RegisterSingleton(typeof(IBenchmarkRunner),      typeof(BenchmarkRunner));
            containerRegistry.RegisterSingleton(typeof(IReplayRunner),         typeof(ReplayRunner));

            containerRegistry.RegisterSingleton(typeof(MainWindowViewModel));
        }
    }
}


