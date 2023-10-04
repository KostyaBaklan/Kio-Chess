using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Windows;
using CommonServiceLocator;
using DataAccess.Interfaces;
using DataAccess.Services;
using Engine.Dal.Interfaces;
using Engine.Dal.Services;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Interfaces.Evaluation;
using Engine.Models.Config;
using Engine.Services;
using Engine.Services.Bits;
using Engine.Services.Evaluation;
using EvaluationEditor.Views;
using Newtonsoft.Json;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;

namespace EvaluationEditor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        var s = File.ReadAllText(@"Config\Configuration.json");
        Configuration configuration = JsonConvert.DeserializeObject<Configuration>(s);

        var x = File.ReadAllText(@"Config\StaticTables.json");
        StaticTableCollection collection = JsonConvert.DeserializeObject<StaticTableCollection>(x);

        var z = File.ReadAllText(@"Config\Table.json");
        Dictionary<int, TableConfiguration> table = JsonConvert.DeserializeObject<Dictionary<int, TableConfiguration>>(z);

        Evaluation evaluation = configuration.Evaluation;
        IConfigurationProvider configurationProvider = new ConfigurationProvider(configuration.AlgorithmConfiguration, new EvaluationProvider(evaluation.Static, evaluation.Opening, evaluation.Middle, evaluation.End),
            configuration.GeneralConfiguration, configuration.PieceOrderConfiguration, configuration.EndGameConfiguration,
        configuration.BookConfiguration);
        containerRegistry.RegisterInstance(configurationProvider);

        IStaticValueProvider staticValueProvider = new StaticValueProvider(collection);
        containerRegistry.RegisterInstance(staticValueProvider);

        ITableConfigurationProvider tableConfigurationProvider = new TableConfigurationProvider(table, configurationProvider);
        containerRegistry.RegisterInstance(tableConfigurationProvider);

        containerRegistry.RegisterSingleton(typeof(IMoveProvider), typeof(MoveProvider));
        containerRegistry.RegisterSingleton(typeof(IMoveSorterProvider), typeof(MoveSorterProvider));
        containerRegistry.RegisterSingleton(typeof(IMoveFormatter), typeof(MoveFormatter));
        containerRegistry.RegisterSingleton(typeof(IMoveHistoryService), typeof(MoveHistoryService));
        containerRegistry.RegisterSingleton(typeof(IEvaluationServiceFactory), typeof(EvaluationServiceFactory));
        containerRegistry.RegisterSingleton(typeof(IKillerMoveCollectionFactory), typeof(KillerMoveCollectionFactory));
        containerRegistry.RegisterSingleton(typeof(IAttackEvaluationService), typeof(AttackEvaluationService));
        containerRegistry.RegisterSingleton(typeof(IOpeningService), typeof(OpeningService));
        containerRegistry.RegisterSingleton(typeof(IProbCutModelProvider), typeof(ProbCutModelProvider));
        containerRegistry.RegisterSingleton(typeof(ITranspositionTableService), typeof(TranspositionTableService));
        containerRegistry.RegisterSingleton(typeof(IDataPoolService), typeof(DataPoolService));
        containerRegistry.RegisterSingleton(typeof(IStrategyFactory), typeof(StrategyFactory));
        containerRegistry.RegisterSingleton(typeof(IGameDbService), typeof(GameDbService));
        containerRegistry.RegisterSingleton(typeof(IOpeningDbService), typeof(OpeningDbService));
        containerRegistry.RegisterSingleton(typeof(IBookService), typeof(BookService));
        containerRegistry.Register<IDataKeyService, DataKeyService>();

        if (ArmBase.Arm64.IsSupported)
        {
            containerRegistry.RegisterSingleton(typeof(IBitService), typeof(AmdBitService));
        }
        else if (Popcnt.X64.IsSupported && Bmi1.X64.IsSupported)
        {
            containerRegistry.RegisterSingleton(typeof(IBitService), typeof(IntelBitService));
        }
        else
        {
            containerRegistry.RegisterSingleton(typeof(IBitService), typeof(BitService));
        }

        containerRegistry.RegisterSingleton(typeof(EditorViewModel));
    }

    protected override Window CreateShell()
    {
        var regionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
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
            var resolve = ServiceLocator.Current.GetInstance(vmType);
            return resolve;
        });
    }

    #endregion
}
