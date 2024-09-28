using System;
using System.Globalization;
using System.Windows;
using Application.Interfaces;
using Application.Services;
using DataAccess.Interfaces;
using Engine.Dal.Interfaces;
using Kgb.ChessApp.Views;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using UI.Common;

namespace Kgb.ChessApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : UiApp
{
    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        var service = ContainerLocator.Current.Resolve<IGameDbService>();

        service.Disconnect();

        var openingDbservice = ContainerLocator.Current.Resolve<IOpeningDbService>();

        openingDbservice.Disconnect();

        var localDbservice = ContainerLocator.Current.Resolve<ILocalDbService>();

        localDbservice.Disconnect();
    }

    protected override Window CreateShell()
    {
        var regionManager = ContainerLocator.Current.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion("Main", typeof(StartView));
        regionManager.RegisterViewWithRegion("Main", typeof(GameView));

        return new Shell();
    }

    #region Overrides of PrismApplicationBase

    protected override void  ConfigureViewModelLocator()
    {
        ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(viewType =>
        {
            var viewName = viewType.FullName;
            var viewAssemblyName = viewType.Assembly.FullName;
            var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}Model, {1}", viewName, viewAssemblyName);
            return Type.GetType(viewModelName);
        });

        ViewModelLocationProvider.SetDefaultViewModelFactory(vmType =>
        {
            var resolve = ContainerLocator.Current.Resolve(vmType);
            return resolve;
        });
    }

    protected override void RegisterLocalTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton(typeof(IStrategyProvider), typeof(StrategyProvider));

        containerRegistry.RegisterSingleton(typeof(StartViewModel));
        containerRegistry.RegisterSingleton(typeof(GameViewModel));
    }

    #endregion
}
