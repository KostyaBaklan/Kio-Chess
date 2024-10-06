using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Strategies.Base;
using Engine.Strategies.Models;

namespace Engine.Strategies.Aspiration;

public abstract class AspirationStrategyBase : StrategyBase
{
    protected List<AspirationModel> Models;

    protected AspirationStrategyBase(short depth, Position position, TranspositionTable table = null) : base(depth, position,table)
    {
        Models = new List<AspirationModel>();

        var configurationProvider = ContainerLocator.Current.Resolve<IConfigurationProvider>();
        var configuration = configurationProvider.AlgorithmConfiguration.AspirationConfiguration;
        var factory = ContainerLocator.Current.Resolve<IStrategyFactory>();

        var models = new Stack<AspirationModel>();
        int id = depth;
        int s = 0;

        while (id >= configuration.AspirationMinDepth[depth])
        {
            StrategyBase strategy = factory.HasMemoryStrategy(configuration.Strategies[s])
                ? factory.GetStrategy((short)id, Position, Table, configuration.Strategies[s])
                : factory.GetStrategy((short)id, Position, configuration.Strategies[s]);

            models.Push(new AspirationModel { Window = configuration.AspirationWindow, Depth = (sbyte)id, Strategy = strategy });

            id -= configuration.AspirationDepth;
            s++;
        }

        if(models.Count == 0) 
        {
            models.Push(new AspirationModel { Window = configuration.AspirationWindow, Depth = (sbyte)depth,
                Strategy = factory.GetStrategy((short)id, Position, Table, "lmrd")
            }); 
        }

        Models = models.ToList();
    }

    public override StrategyType Type => StrategyType.ASP;

    public override IResult GetResult()
    {
        if (MoveHistory.GetPly() < 0)
        {
            return Models.Last().Strategy.GetFirstMove();
        }

        if (MoveHistory.IsEndPhase())
        {
            return EndGameStrategy.GetResult();
        }

        IResult result = new Result
        {
            GameResult = GameResult.Continue,
            Move = null,
            Value = 0
        };

        var window = SearchValue;

        foreach (var model in Models)
        {
            int alpha = result.Value - window;
            int beta = result.Value + window;

            var move = result.Move;
            window = model.Window;

            result = model.Strategy.GetResult(alpha, beta, model.Depth, move);

            if (result.Value < beta && result.Value > alpha)
                continue;

            result = model.Strategy.GetResult(MinusSearchValue, SearchValue, model.Depth, move);
        }

        return result;
    }
}
