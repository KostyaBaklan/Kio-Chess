﻿using Engine.Interfaces.Config;
using Engine.Interfaces;
using Engine.Models.Config;
using Engine.Services;
using Newtonsoft.Json;
using Unity;
using CommonServiceLocator;
using Engine.Services.Bits;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

public class Boot
{
    public static void SetUp()
    {
        IUnityContainer container = new UnityContainer();
        ServiceLocatorAdapter serviceLocatorAdapter = new ServiceLocatorAdapter(container);

        var s = File.ReadAllText(@"Config\Configuration.json");
        var configuration = JsonConvert.DeserializeObject<Configuration>(s);

        var x = File.ReadAllText(@"Config\StaticTables.json");
        var collection = JsonConvert.DeserializeObject<StaticTableCollection>(x);

        var z = File.ReadAllText(@"Config\Table.json");
        var table = JsonConvert.DeserializeObject<Dictionary<int, TableConfiguration>>(z);

        ServiceLocator.SetLocatorProvider(() => serviceLocatorAdapter);
        container.RegisterInstance<IServiceLocator>(serviceLocatorAdapter);
        container.RegisterInstance<IServiceProvider>(serviceLocatorAdapter);

        var evaluation = configuration.Evaluation;
        IConfigurationProvider configurationProvider = new ConfigurationProvider(configuration.AlgorithmConfiguration,
            new EvaluationProvider(evaluation.Static, evaluation.Opening, evaluation.Middle, evaluation.End),
            configuration.GeneralConfiguration, configuration.PieceOrderConfiguration, configuration.EndGameConfiguration);
        container.RegisterInstance(configurationProvider);

        IStaticValueProvider staticValueProvider = new StaticValueProvider(collection);
        container.RegisterInstance(staticValueProvider);

        ITableConfigurationProvider tableConfigurationProvider = new TableConfigurationProvider(table, configurationProvider);
        container.RegisterInstance(tableConfigurationProvider);

        container.RegisterSingleton(typeof(IMoveProvider), typeof(MoveProvider));
        container.RegisterSingleton(typeof(IMoveSorterProvider), typeof(MoveSorterProvider));
        container.RegisterSingleton(typeof(IMoveFormatter), typeof(MoveFormatter));
        container.RegisterSingleton(typeof(IMoveHistoryService), typeof(MoveHistoryService));
        container.RegisterSingleton(typeof(IEvaluationService), typeof(EvaluationService));
        container.RegisterSingleton(typeof(IKillerMoveCollectionFactory), typeof(KillerMoveCollectionFactory));
        container.RegisterSingleton(typeof(IAttackEvaluationService), typeof(AttackEvaluationService));
        container.RegisterSingleton(typeof(IOpeningService), typeof(OpeningService));
        container.RegisterSingleton(typeof(IProbCutModelProvider), typeof(ProbCutModelProvider));
        container.RegisterSingleton(typeof(ITranspositionTableService), typeof(TranspositionTableService));
        container.RegisterSingleton(typeof(IDataPoolService), typeof(DataPoolService));
        container.RegisterSingleton(typeof(IStrategyFactory), typeof(StrategyFactory));

        if (ArmBase.Arm64.IsSupported)
        {
            container.RegisterSingleton(typeof(IBitService), typeof(AmdBitService));
        }
        else if (Popcnt.X64.IsSupported && Bmi1.X64.IsSupported)
        {
            container.RegisterSingleton(typeof(IBitService), typeof(IntelBitService));
        }
        else
        {
            container.RegisterSingleton(typeof(IBitService), typeof(BitService));
        }
    }
}
