using CommonServiceLocator;
using Prism.Ioc;
using Prism.Regions;
using Chess.Views;
using UI.Common.App;

namespace Chess
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : AppBase
    {
        protected override void RegisterInternal(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton(typeof(StartViewModel));
            containerRegistry.RegisterSingleton(typeof(GameViewModel));
        }

        protected override void RegisterRegions()
        {
            var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            regionManager.RegisterViewWithRegion("Main", typeof(StartView));
            regionManager.RegisterViewWithRegion("Main", typeof(GameView));
        }
    }
}
