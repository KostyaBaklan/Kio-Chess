﻿using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Strategies.Base;
using Engine.Strategies.End;
using Engine.Strategies.Models;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Aspiration
{
    public abstract class AspirationStrategyBase : StrategyBase
    {
        protected int AspirationDepth;
        protected int AspirationMinDepth;
        protected TranspositionTable Table;

        protected List<AspirationModel> Models;

        protected AspirationStrategyBase(short depth, IPosition position, TranspositionTable table = null) : base(depth, position)
        {
            var service = ServiceLocator.Current.GetInstance<ITranspositionTableService>();

            if (table == null)
            {
                table = service.Create(depth); 
            }

            Table = table;
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
                var aspirationModel = new AspirationModel { Window = (short)configuration.AspirationWindow[d], Depth = (sbyte)d };
                Models.Add(aspirationModel);
            }

            InitializeModels(table);
        }
        public override int Size => Table.Count;

        protected abstract void InitializeModels(TranspositionTable table);

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
                short alpha = (short)(result.Value - model.Window);
                short beta = (short)(result.Value + model.Window);

                result = model.Strategy.GetResult(alpha, beta, model.Depth, result.Move);

                if (result.Value >= beta)
                {
                    result = model.Strategy.GetResult((short)(result.Value - model.Window), SearchValue, model.Depth);
                }
                else if (result.Value <= alpha)
                {
                    result = model.Strategy.GetResult((short)-SearchValue, (short)(result.Value + model.Window), model.Depth);
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
}
