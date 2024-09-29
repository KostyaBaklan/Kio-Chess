using Engine.Interfaces.Config;
using Engine.Interfaces;
using Engine.Models.Config;
using Engine.Services;
using Newtonsoft.Json;
using Unity;
using Engine.Services.Bits;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using Engine.Services.Evaluation;
using Unity.Lifetime;
using Engine.Dal.Interfaces;
using Engine.Dal.Services;
using DataAccess.Interfaces;
using DataAccess.Services;

public class Boot
{
    public static void SetUp()
    {
        IUnityContainer container = new UnityContainer();
        UnityContainerExtension serviceLocatorAdapter = new UnityContainerExtension(container);

        var s = File.ReadAllText(@"Config\Configuration.json");
        var configuration = JsonConvert.DeserializeObject<Configuration>(s);

        var x = File.ReadAllText(@"Config\StaticTables.json");
        var collection = JsonConvert.DeserializeObject<StaticTableCollection>(x);

        ContainerLocator.SetContainerExtension(serviceLocatorAdapter);

        var evaluation = configuration.Evaluation;
        IConfigurationProvider configurationProvider = new ConfigurationProvider(configuration.AlgorithmConfiguration,
            new EvaluationProvider(evaluation.Static, evaluation.Opening, evaluation.Middle, evaluation.End),
            configuration.GeneralConfiguration, configuration.EndGameConfiguration,
            configuration.BookConfiguration);
        container.RegisterInstance(configurationProvider);

        IStaticValueProvider staticValueProvider = new StaticValueProvider(collection);
        container.RegisterInstance(staticValueProvider);

        container.RegisterInstance(new MoveProvider(configurationProvider, staticValueProvider));
        container.RegisterSingleton(typeof(IMoveSorterProvider), typeof(MoveSorterProvider));
        container.RegisterSingleton(typeof(IMoveFormatter), typeof(MoveFormatter));
        container.RegisterSingleton(typeof(MoveHistoryService), typeof(MoveHistoryService));
        container.RegisterSingleton(typeof(IEvaluationServiceFactory), typeof(EvaluationServiceFactory));
        container.RegisterSingleton(typeof(IKillerMoveCollectionFactory), typeof(KillerMoveCollectionFactory));
        container.RegisterSingleton(typeof(ITranspositionTableService), typeof(TranspositionTableService));
        container.RegisterSingleton(typeof(DataPoolService));
        container.RegisterSingleton(typeof(IStrategyFactory), typeof(StrategyFactory));
        container.RegisterSingleton(typeof(IGameDbService), typeof(GameDbService));
        container.RegisterSingleton(typeof(ILocalDbService), typeof(LocalDbService));
        container.RegisterSingleton(typeof(IOpeningDbService), typeof(OpeningDbService));
        container.RegisterSingleton(typeof(IMemoryDbService), typeof(MemoryDbService));
        container.RegisterSingleton(typeof(IBulkDbService), typeof(BulkDbService));
        container.RegisterType<IDataKeyService, DataKeyService>(new TransientLifetimeManager());

        if (ArmBase.Arm64.IsSupported)
        {
            container.RegisterSingleton(typeof(BitServiceBase), typeof(AmdBitService));
        }
        else if (Popcnt.X64.IsSupported && Bmi1.X64.IsSupported)
        {
            container.RegisterSingleton(typeof(BitServiceBase), typeof(IntelBitService));
        }
        else
        {
            container.RegisterSingleton(typeof(BitServiceBase), typeof(BitService));
        }
    }

    public static T GetService<T>() => ContainerLocator.Current.Resolve<T>();
}
