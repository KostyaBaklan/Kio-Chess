using Engine.Interfaces.Config;
using Engine.Interfaces;
using Engine.Models.Config;
using Engine.Services;
using Newtonsoft.Json;
using Unity;
using CommonServiceLocator;
using Engine.Services.Bits;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using Engine.Services.Evaluation;
using Engine.Interfaces.Evaluation;
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
            configuration.GeneralConfiguration, configuration.PieceOrderConfiguration, configuration.EndGameConfiguration,
            configuration.BookConfiguration);
        container.RegisterInstance(configurationProvider);

        IStaticValueProvider staticValueProvider = new StaticValueProvider(collection);
        container.RegisterInstance(staticValueProvider);

        ITableConfigurationProvider tableConfigurationProvider = new TableConfigurationProvider(table, configurationProvider);
        container.RegisterInstance(tableConfigurationProvider);

        container.RegisterSingleton(typeof(IMoveProvider), typeof(MoveProvider));
        container.RegisterSingleton(typeof(IMoveSorterProvider), typeof(MoveSorterProvider));
        container.RegisterSingleton(typeof(IMoveFormatter), typeof(MoveFormatter));
        container.RegisterSingleton(typeof(IMoveHistoryService), typeof(MoveHistoryService));
        container.RegisterSingleton(typeof(IEvaluationServiceFactory), typeof(EvaluationServiceFactory));
        container.RegisterSingleton(typeof(IKillerMoveCollectionFactory), typeof(KillerMoveCollectionFactory));
        container.RegisterSingleton(typeof(IAttackEvaluationService), typeof(AttackEvaluationService));
        container.RegisterSingleton(typeof(IOpeningService), typeof(OpeningService));
        container.RegisterSingleton(typeof(IProbCutModelProvider), typeof(ProbCutModelProvider));
        container.RegisterSingleton(typeof(ITranspositionTableService), typeof(TranspositionTableService));
        container.RegisterSingleton(typeof(IDataPoolService), typeof(DataPoolService));
        container.RegisterSingleton(typeof(IStrategyFactory), typeof(StrategyFactory));
        container.RegisterSingleton(typeof(IGameDbService), typeof(GameDbService));
        container.RegisterSingleton(typeof(IOpeningDbService), typeof(OpeningDbService));
        container.RegisterSingleton(typeof(IMemoryDbService), typeof(MemoryDbService));
        container.RegisterSingleton(typeof(IBulkDbService), typeof(BulkDbService));
        container.RegisterSingleton(typeof(IBookService), typeof(BookService));
        container.RegisterType<IDataKeyService, DataKeyService>(new TransientLifetimeManager());

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

    public static T GetService<T>()
    {
        return ServiceLocator.Current.GetInstance<T>();
    }
}
