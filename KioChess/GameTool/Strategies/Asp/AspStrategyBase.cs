using CommonServiceLocator;
using Engine.DataStructures.Hash;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Strategies.Base;
using Engine.Strategies.End;
using System.Runtime.CompilerServices;

namespace GameTool.Strategies.Asp
{
    public abstract class AspStrategyBase : StrategyBase
    {
        protected int AspirationDepth;
        protected int AspirationMinDepth;
        protected TranspositionTable Table;

        protected List<AspModel> Models;
        public short[] AspirationWindow;

        protected AspStrategyBase(short depth, IPosition position, int step) : base(depth, position)
        {
            short w = 200;
            short s = (short)(step == 10 ? 20 : 25);

            AspirationWindow = new short[20];
            for (int i = 0; i < 20; i++)
            {
                AspirationWindow[i] = w;
                w += s;
            }

            int id = depth;
            while (id > 3)
            {
                id -= step;
            }

            var service = ServiceLocator.Current.GetInstance<ITranspositionTableService>();

            Table = service.Create(depth); 

            Models = new List<AspModel>();

            AspirationDepth = step;
            AspirationMinDepth = id+step;

            var model = new AspModel { Window = SearchValue, Depth = (sbyte)id };
            Models.Add(model);

            for (int i = AspirationMinDepth; i <= Depth; i += AspirationDepth)
            {
                var aspirationModel = new AspModel { Window = AspirationWindow[i], Depth = (sbyte)i };
                Models.Add(aspirationModel);
            }

            for (int i = 0; i < Models.Count; i++)
            {
                Models[i].Strategy = GetStrategy(Models[i].Depth, Position, Table);
            }
        }

        protected abstract StrategyBase GetStrategy(short depth, IPosition position, TranspositionTable table);

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
