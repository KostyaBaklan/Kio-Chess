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
    protected int AspirationDepth;
    protected int AspirationMinDepth;

    protected List<AspirationModel> Models;

    protected AspirationStrategyBase(short depth, Position position, TranspositionTable table = null) : base(depth, position,table)
    {
        Models = new List<AspirationModel>();

        var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
        var configuration = configurationProvider.AlgorithmConfiguration.AspirationConfiguration;
        AspirationDepth = configuration.AspirationDepth;
        AspirationMinDepth = configuration.AspirationMinDepth;

        var t = Depth - AspirationDepth * configuration.AspirationIterations[depth];

        var model = new AspirationModel { Window = SearchValue, Depth = (sbyte)t };
        Models.Add(model);

        for (int d = t + AspirationDepth; d <= depth; d += AspirationDepth)
        {
            var aspirationModel = new AspirationModel { Window = configuration.AspirationWindow[d], Depth = (sbyte)d };
            Models.Add(aspirationModel);
        }

        InitializeModels();
    }

    protected abstract void InitializeModels();

    public override IResult GetResult()
    {
        if (MoveHistory.GetPly() < 0)
        {
            return GetFirstMove();
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
            int alpha = result.Value - model.Window;
            int beta = result.Value + model.Window;

            result = model.Strategy.GetResult(alpha, beta, model.Depth, result.Move);

            if (result.Value >= beta)
            {
                result = model.Strategy.GetResult(result.Value - model.Window, SearchValue, model.Depth);
            }
            else if (result.Value <= alpha)
            {
                result = model.Strategy.GetResult(MinusSearchValue, result.Value + model.Window, model.Depth);
            }
        }

        return result;
    }
}
