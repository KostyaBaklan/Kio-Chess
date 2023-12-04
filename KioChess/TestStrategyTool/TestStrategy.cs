using Engine.DataStructures.Hash;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Strategies.Base;
using Engine.Strategies.End;
using Engine.Strategies.Models;
using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.Strategies.Lmr;

namespace TestStrategyTool;

internal class TestStrategy : StrategyBase
{
    protected List<AspirationModel> Models;
    protected TranspositionTable Table;

    public TestStrategy(short depth, IPosition position) : base(depth, position)
    {
        var service = ServiceLocator.Current.GetInstance<ITranspositionTableService>();

        Table = service.Create(depth); ;
        Models = new List<AspirationModel>();

        int AspirationDepth = 2;

        var t = Depth %2 == 0?4:5;

        var model = new AspirationModel { Window = SearchValue, Depth = (sbyte)t };
        Models.Add(model);

        int window = 400;

        for (int d = t + AspirationDepth; d <= depth; d += AspirationDepth)
        {
            var aspirationModel = new AspirationModel { Window = (short)window, Depth = (sbyte)d };
            Models.Add(aspirationModel);

            window += 25;
        }

        InitializeModels();
    }

    private void InitializeModels()
    {
        for (int i = 0; i < Models.Count; i++)
        {
            if (Models[i].Depth > 8)
            {
                Models[i].Strategy = new LmrDeepStrategy(Models[i].Depth, Position, Table); 
            }
            else
            {
                Models[i].Strategy = new LmrStrategy(Models[i].Depth, Position, Table);
            }
        }
    }

    public override int Size => Table.Count;

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
            var alpha = result.Value - model.Window;
            var beta = result.Value + model.Window;

            result = model.Strategy.GetResult((short)alpha, (short)beta, model.Depth, result.Move);

            if (result.Value >= beta)
            {
                result = model.Strategy.GetResult((short)(result.Value - model.Window), SearchValue, model.Depth, result.Move);
            }
            else if (result.Value <= alpha)
            {
                result = model.Strategy.GetResult((short)-SearchValue, (short)(result.Value + model.Window), model.Depth, result.Move);
            }
        }

        return result;
    }

    protected override StrategyBase CreateEndGameStrategy()
    {
        return new LmrDeepEndGameStrategy((short)Math.Min(Depth + 1, MaxEndGameDepth), Position, Table);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsBlocked()
    {
        return Table.IsBlocked();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ExecuteAsyncAction()
    {
        Table.Update();
    }
}
