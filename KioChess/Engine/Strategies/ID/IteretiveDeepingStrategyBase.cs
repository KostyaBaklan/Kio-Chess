﻿using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Strategies.Base;
using Engine.Strategies.Models;

namespace Engine.Strategies.ID;


public abstract class IteretiveDeepingStrategyBase : StrategyBase
{
    protected int InitialDepth;
    protected int DepthStep;
    protected string[] Strategies;

    protected List<IterativeDeepingModel> Models;

    protected IteretiveDeepingStrategyBase(short depth, Position position) : base(depth, position)
    {
        var configurationProvider = ContainerLocator.Current.Resolve<IConfigurationProvider>();
        var configuration = configurationProvider.AlgorithmConfiguration.IterativeDeepingConfiguration;
        Strategies = configuration.Strategies;
        var factory = ContainerLocator.Current.Resolve<IStrategyFactory>();

        var models = new Stack<IterativeDeepingModel>();
        short id = depth;
        int s = 0;

        while (id >= configuration.InitialDepth)
        {
            StrategyBase strategy = factory.HasMemoryStrategy(Strategies[s])
                ? factory.GetStrategy(id, Position, Table, Strategies[s])
                : factory.GetStrategy(id, Position, Strategies[s]);

            models.Push(new IterativeDeepingModel { Depth = (sbyte)id, Strategy = strategy });

            id -= configuration.DepthStep;
            s++;
        }

        if(models.Count < 1)
        {
            models.Push(new IterativeDeepingModel
            {
                Depth = (sbyte)id,
                Strategy = factory.GetStrategy(depth, Position, Table, "lmrd")
            }); ;
        }
        Models = models.ToList();
    }


    public override StrategyType Type => StrategyType.ID;
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

        foreach (var model in Models)
        {
            result = model.Strategy.GetResult(MinusSearchValue, SearchValue, model.Depth, result.Move);
            if (result.GameResult != GameResult.Continue) break;
        }

        return result;
    }
}
