using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Strategies.Base;
using Engine.Strategies.Models;

namespace Engine.Strategies.Aspiration;

public class AspirationStrategy : StrategyBase
{
    protected List<AspirationModel> Models;

    private readonly StrategyType _type;

    public AspirationStrategy(short depth, Position position, TranspositionTable table = null) : this(depth, position, table, null, null)
    {
    }

    public AspirationStrategy(short depth, Position position, TranspositionTable table, Func<int, string> strategySelector, int? stages, StrategyType type = StrategyType.ASP) : base(depth, position, table)
    {
        _type = type;

        Models = [];

        var configurationProvider = ContainerLocator.Current.Resolve<IConfigurationProvider>();
        var configuration = configurationProvider.AlgorithmConfiguration.AspirationConfiguration;
        var factory = ContainerLocator.Current.Resolve<IStrategyFactory>();

        var models = new Stack<AspirationModel>();
        int id = depth;
        int s = 0;

        int minDepth = stages.HasValue
            ? depth - (stages.Value - 1) * configuration.AspirationDepth
            : configuration.AspirationMinDepth[depth];

        while (id >= minDepth)
        {
            string code = strategySelector != null ? strategySelector(s) : configuration.Strategies[s];

            StrategyBase strategy = factory.HasMemoryStrategy(code)
                ? factory.GetStrategy((short)id, Position, Table, code)
                : factory.GetStrategy((short)id, Position, code);

            models.Push(new AspirationModel { Window = configuration.AspirationWindow, Depth = (sbyte)id, Strategy = strategy });

            id -= configuration.AspirationDepth;
            s++;
        }

        if (models.Count == 0)
        {
            models.Push(new AspirationModel
            {
                Window = configuration.AspirationWindow,
                Depth = (sbyte)depth,
                Strategy = factory.GetStrategy((short)id, Position, Table, "lmrd")
            });
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

        var window = SearchValue;

        foreach (var model in Models)
        {
            int alpha = result.Value - window;
            int beta = result.Value + window;

            var move = result.Move;
            window = model.Window;

            result = model.Strategy.GetResult(alpha, beta, model.Depth, move);
            if (result.GameResult != GameResult.Continue) break;

            if (result.Value < beta && result.Value > alpha)
                continue;

            result = model.Strategy.GetResult(MinusSearchValue, SearchValue, model.Depth, move);
        }

        return result;
    }
}
