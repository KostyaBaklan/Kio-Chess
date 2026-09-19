using Engine.Interfaces.Config;
using Engine.Interfaces;
using Engine.Models.Config;
using Engine.Services;
using Newtonsoft.Json;
using Unity;
using Engine.Services.Evaluation;
using Engine.Dal.Services;
using Engine.Dal.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Services;

public class Boot
{
    public static void SetUp()
    {
        IUnityContainer container = new UnityContainer();
        UnityContainerExtension serviceLocatorAdapter = new(container);

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
        container.RegisterSingleton<IMoveSorterProvider, MoveSorterProvider>();
        container.RegisterSingleton<IMoveFormatter, MoveFormatter>();
        container.RegisterSingleton<MoveHistoryService, MoveHistoryService>();
        container.RegisterSingleton<IEvaluationServiceFactory, EvaluationServiceFactory>();
        container.RegisterSingleton<ITranspositionTableService, TranspositionTableService>();
        container.RegisterSingleton<DataPoolService>();
        container.RegisterSingleton<IStrategyFactory, StrategyFactory>();
        container.RegisterSingleton<IMemoryGameService, MemoryGameService>();
        var appDbService = new AppDbService(configuration.BookConfiguration.DatabasePath);
        container.RegisterInstance<IAppDbService>(appDbService);

        var openingService = new OpeningService(configuration.BookConfiguration.DatabasePath);
        container.RegisterInstance<IOpeningService>(openingService);
        container.RegisterSingleton<IGamesService, GamesService>();
        container.RegisterSingleton<GameEntityFactory, GameEntityFactory>();
        container.RegisterSingleton<ICacheLoaderService, CacheLoaderService>();
        container.RegisterSingleton<IGameHistoryService, GameHistoryService>();
    }

    public static T GetService<T>() => ContainerLocator.Current.Resolve<T>();
}
