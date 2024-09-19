using System;
using System.Globalization;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Windows;
using Application.Interfaces;
using Application.Services;
using CommonServiceLocator;
using DataAccess.Interfaces;
using DataAccess.Services;
using Engine.Dal.Interfaces;
using Engine.Dal.Services;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Config;
using Engine.Services;
using Engine.Services.Bits;
using Engine.Services.Evaluation;
using Kgb.ChessApp.Views;
using Newtonsoft.Json;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;

namespace Kgb.ChessApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    protected override void ConfigureServiceLocator()
    {
        base.ConfigureServiceLocator();

        var localDbservice = ServiceLocator.Current.GetInstance<ILocalDbService>();

        localDbservice.Connect();

        var gameDbservice = ServiceLocator.Current.GetInstance<IGameDbService>();

        gameDbservice.Connect();

        gameDbservice.LoadAsync();

        var openingDbservice = ServiceLocator.Current.GetInstance<IOpeningDbService>();

        openingDbservice.Connect();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        var service = ServiceLocator.Current.GetInstance<IGameDbService>();

        service.Disconnect();

        var openingDbservice = ServiceLocator.Current.GetInstance<IOpeningDbService>();

        openingDbservice.Disconnect();

        var localDbservice = ServiceLocator.Current.GetInstance<ILocalDbService>();

        localDbservice.Disconnect();
    }
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        var s = File.ReadAllText(@"Config\Configuration.json");
        Configuration configuration = JsonConvert.DeserializeObject<Configuration>(s);

        var x = File.ReadAllText(@"Config\StaticTables.json");
        StaticTableCollection collection = JsonConvert.DeserializeObject<StaticTableCollection>(x);

        Evaluation evaluation = configuration.Evaluation;
        IConfigurationProvider configurationProvider = new ConfigurationProvider(configuration.AlgorithmConfiguration, new EvaluationProvider(evaluation.Static, evaluation.Opening, evaluation.Middle, evaluation.End),
            configuration.GeneralConfiguration, configuration.EndGameConfiguration,
        configuration.BookConfiguration);
        containerRegistry.RegisterInstance(configurationProvider);

        IStaticValueProvider staticValueProvider = new StaticValueProvider(collection);
        containerRegistry.RegisterInstance(staticValueProvider);

        containerRegistry.RegisterInstance(new MoveProvider(configurationProvider, staticValueProvider));
        containerRegistry.RegisterSingleton(typeof(IMoveSorterProvider), typeof(MoveSorterProvider));
        containerRegistry.RegisterSingleton(typeof(IMoveFormatter), typeof(MoveFormatter));
        containerRegistry.RegisterSingleton(typeof(MoveHistoryService), typeof(MoveHistoryService));
        containerRegistry.RegisterSingleton(typeof(IEvaluationServiceFactory), typeof(EvaluationServiceFactory));
        containerRegistry.RegisterSingleton(typeof(IKillerMoveCollectionFactory), typeof(KillerMoveCollectionFactory));
        containerRegistry.RegisterSingleton(typeof(ITranspositionTableService), typeof(TranspositionTableService));
        containerRegistry.RegisterSingleton(typeof(DataPoolService));
        containerRegistry.RegisterSingleton(typeof(IStrategyFactory), typeof(StrategyFactory));
        containerRegistry.RegisterSingleton(typeof(IGameDbService), typeof(GameDbService));
        containerRegistry.RegisterSingleton(typeof(ILocalDbService), typeof(LocalDbService));
        containerRegistry.RegisterSingleton(typeof(IOpeningDbService), typeof(OpeningDbService));
        containerRegistry.RegisterSingleton(typeof(IBulkDbService), typeof(BulkDbService));
        containerRegistry.Register<IDataKeyService, DataKeyService>();

        if (ArmBase.Arm64.IsSupported)
        {
            containerRegistry.RegisterSingleton(typeof(BitServiceBase), typeof(AmdBitService));
        }
        else if (Popcnt.X64.IsSupported && Bmi1.X64.IsSupported)
        {
            containerRegistry.RegisterSingleton(typeof(BitServiceBase), typeof(IntelBitService));
        }
        else
        {
            containerRegistry.RegisterSingleton(typeof(BitServiceBase), typeof(BitService));
        }


        containerRegistry.RegisterSingleton(typeof(IStrategyProvider), typeof(StrategyProvider));

        containerRegistry.RegisterSingleton(typeof(StartViewModel));
        containerRegistry.RegisterSingleton(typeof(GameViewModel));
    }

    protected override Window CreateShell()
    {
        var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
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
            var resolve = ServiceLocator.Current.GetInstance(vmType);
            return resolve;
        });
    }

    #endregion
}
