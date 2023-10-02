using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Strategies.Base;
using Engine.Strategies.End;
using Engine.Strategies.Models;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.ID;


public abstract class IteretiveDeepingStrategyBase : StrategyBase
{
    protected int InitialDepth;
    protected int DepthStep;
    protected string[] Strategies;
    protected TranspositionTable Table;

    protected List<IterativeDeepingModel> Models;

    protected IteretiveDeepingStrategyBase(short depth, IPosition position) : base(depth, position)
    {
        var configurationProvider = ServiceLocator.Current.GetInstance<IConfigurationProvider>();
        var configuration = configurationProvider.AlgorithmConfiguration.IterativeDeepingConfiguration;

        int id = depth;
        while (id > configuration.InitialDepth)
        {
            id -= configuration.DepthStep;
        }

        InitialDepth = id;
        DepthStep = configuration.DepthStep;
        Strategies = configuration.Strategies;
        Models = new List<IterativeDeepingModel>();
        
        var service = ServiceLocator.Current.GetInstance<ITranspositionTableService>();

        Table = service.Create(depth);
        int s = 0; 
        
        var factory = ServiceLocator.Current.GetInstance<IStrategyFactory>();
        for (int i = InitialDepth; i <= Depth; i+= DepthStep)
        {
            short d = (short)i;
            StrategyBase strategy;
            if (factory.HasMemoryStrategy(Strategies[s]))
            {
                strategy = factory.GetStrategy(d, Position, Table, Strategies[s]);
            }
            else
            {
                strategy = factory.GetStrategy(d, Position, Strategies[s]);
            }

            Models.Add(new IterativeDeepingModel { Depth = (sbyte)i, Strategy = strategy });

            s++;
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
            result = model.Strategy.GetResult((short)-SearchValue, SearchValue, model.Depth, result.Move);
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
