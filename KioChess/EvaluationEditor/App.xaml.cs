using System;
using System.Globalization;
using System.Windows;
using EvaluationEditor.Views;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using UI.Common;

namespace EvaluationEditor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : UiApp
{

    protected override Window CreateShell()
    {
        var regionManager = ContainerLocator.Current.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion("Main", typeof(EditorView));

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
        containerRegistry.RegisterSingleton(typeof(EditorViewModel));
    }

    #endregion
}
