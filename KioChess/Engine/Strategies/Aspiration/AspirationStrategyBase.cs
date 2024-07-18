using CommonServiceLocator;
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
    protected short AspirationDepth;
    protected int AspirationMinDepth;
    protected string[] Strategies;

    protected List<AspirationModel> Models;

    protected AspirationStrategyBase(short depth, Position position, TranspositionTable table = null) : base(depth, position,table)
    {
        Models = new List<AspirationModel>();

        var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
        var configuration = configurationProvider.AlgorithmConfiguration.AspirationConfiguration;
        AspirationDepth = (short)configuration.AspirationDepth;
        AspirationMinDepth = configuration.AspirationMinDepth;
        Strategies = configuration.Strategies;
        var factory = ServiceLocator.Current.GetInstance<IStrategyFactory>();

        var models = new Stack<AspirationModel>();
        short id = depth;
        int s = 0;

        while (id >= configuration.AspirationMinDepth)
        {
            StrategyBase strategy = factory.HasMemoryStrategy(Strategies[s])
                ? factory.GetStrategy(id, Position, Table, Strategies[s])
                : factory.GetStrategy(id, Position, Strategies[s]);

            models.Push(new AspirationModel { Window = configuration.AspirationWindow, Depth = (sbyte)id, Strategy = strategy });

            id -= AspirationDepth;
            s++;
        }

        if(models.Count == 0) 
        {
            models.Push(new AspirationModel { Window = configuration.AspirationWindow, Depth = (sbyte)depth,
                Strategy = factory.GetStrategy(id, Position, Table, "lmrd")
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
