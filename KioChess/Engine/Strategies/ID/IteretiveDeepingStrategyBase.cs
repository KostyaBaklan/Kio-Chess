using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Strategies.Base;
using Engine.Strategies.Models;

namespace Engine.Strategies.ID;


public abstract class IteretiveDeepingStrategyBase : MemoryStrategyBase
{
    protected int InitialDepth;
    protected int DepthStep;
    protected string[] Strategies;

    protected List<IterativeDeepingModel> Models;

    protected IteretiveDeepingStrategyBase(short depth, Position position) : base(depth, position)
    {
        var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
        var configuration = configurationProvider.AlgorithmConfiguration.IterativeDeepingConfiguration;
        Strategies = configuration.Strategies;
        var factory = ServiceLocator.Current.GetInstance<IStrategyFactory>();

        var models = new Stack<IterativeDeepingModel>();
        short id = depth;
        int s = 0;

        while (id >= configuration.InitialDepth)
        {
            MemoryStrategyBase strategy = factory.HasMemoryStrategy(Strategies[s])
                ? factory.GetStrategy(id, Position, Table, Strategies[s])
                : factory.GetStrategy(id, Position, Strategies[s]);

            models.Push(new IterativeDeepingModel { Depth = (sbyte)id, Strategy = strategy });

            id -= configuration.DepthStep;
            s++;
        }

        Models = models.ToList();
    }
    
    public override IResult GetResult()
    {
        if (MoveHistory.GetPly() < 0)
        {
            return Models.Last().Strategy.GetFirstMove();
        }

        if (Position.GetPhase() == Phase.End)
        {
            return EndGameStrategy.GetResult();
        }

        IResult result = new Result
        {
            GameResult = GameResult.Continue,
            Move = null,
            Value = 0
        };

        foreach (var model in Models)
        {
            result = model.Strategy.GetResult(MinusSearchValue, SearchValue, model.Depth, result.Move);
            if (result.GameResult != GameResult.Continue) break;
        }

        return result;
    }
}
