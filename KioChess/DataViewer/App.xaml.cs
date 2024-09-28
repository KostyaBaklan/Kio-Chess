using Engine.Dal.Interfaces;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Globalization;
using DataViewer.Views;
using DataAccess.Interfaces;
using UI.Common;
using Prism.Navigation.Regions;

namespace DataViewer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : UiApp
{
    protected override bool ShouldConnectToDb => false;

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
        regionManager.RegisterViewWithRegion("Main", typeof(DataView));
        //regionManager.RegisterViewWithRegion("Main", typeof(GameView));

        return new Shell();
    }

    #region Overrides of PrismApplicationBase

    protected override void ConfigureViewModelLocator()
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
        containerRegistry.RegisterSingleton(typeof(DataViewModel));
    }

    #endregion
}
