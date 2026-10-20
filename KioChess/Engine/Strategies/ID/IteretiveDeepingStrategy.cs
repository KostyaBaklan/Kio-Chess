using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Strategies.Base;
using Engine.Strategies.Models;

namespace Engine.Strategies.ID;


public class IteretiveDeepingStrategy : StrategyBase
{
    private List<IterativeDeepingModel> Models;

    private readonly StrategyType _type;

    public IteretiveDeepingStrategy(short depth, Position position) : this(depth, position, null, null)
    {
    }

    public IteretiveDeepingStrategy(short depth, Position position, Func<int, string> strategySelector, int? stages, StrategyType type = StrategyType.ID) : base(depth, position)
    {
        _type = type;

        var configurationProvider = ContainerLocator.Current.Resolve<IConfigurationProvider>();
        var configuration = configurationProvider.AlgorithmConfiguration.IterativeDeepingConfiguration;
        var factory = ContainerLocator.Current.Resolve<IStrategyFactory>();

        var models = new Stack<IterativeDeepingModel>();
        short id = depth;
        int s = 0;

        int minDepth = stages.HasValue
            ? depth - (stages.Value - 1) * configuration.DepthStep
            : configuration.InitialDepth[depth];

        while (id >= minDepth)
        {
            string code = strategySelector != null ? strategySelector(s) : configuration.Strategies[s];

            StrategyBase strategy = factory.HasMemoryStrategy(code)
                ? factory.GetStrategy(id, Position, Table, code)
                : factory.GetStrategy(id, Position, code);

            models.Push(new IterativeDeepingModel { Depth = (sbyte)id, Strategy = strategy });

            id -= configuration.DepthStep;
            s++;
        }

        if (models.Count < 1)
        {
            models.Push(new IterativeDeepingModel
            {
                Depth = (sbyte)id,
                Strategy = factory.GetStrategy(depth, Position, Table, "lmrd")
            }); ;
        }
        Models = models.ToList();
    }


    public override StrategyType Type => _type;
    public override IResult GetResult()
    {
        if (MoveHistory.GetPly() < 0)
        {
            return Models.Last().Strategy.GetFirstMove();
        }

        DataPoolService.Resize(Table);

        if (MoveHistory.IsEndPhase())
        {
            return _endGameStrategy.GetResult();
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
