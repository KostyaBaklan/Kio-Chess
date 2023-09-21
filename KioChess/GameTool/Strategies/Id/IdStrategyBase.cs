using Engine.DataStructures.Hash;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Strategies.Base;
using Engine.Strategies.End;
using Engine.Strategies.Models;
using System.Runtime.CompilerServices;
using CommonServiceLocator;

namespace GameTool.Strategies.Id
{

    public abstract class IdStrategyBase : StrategyBase
    {
        protected int InitialDepth;
        protected int DepthStep;
        protected TranspositionTable Table;

        protected List<IterativeDeepingModel> Models;

        protected IdStrategyBase(short depth, IPosition position, int depthStep) : base(depth, position)
        {
            int id = depth;
            while (id > 3)
            {
                id -= depthStep;
            }

            InitialDepth = id;
            DepthStep = depthStep;

            Models = new List<IterativeDeepingModel>();

            var service = ServiceLocator.Current.GetInstance<ITranspositionTableService>();

            Table = service.Create(depth);
            int s = 0;

            for (int i = InitialDepth; i <= Depth; i += DepthStep)
            {
                short d = (short)i;
                StrategyBase strategy = GetStrategy(d, Position, Table);

                Models.Add(new IterativeDeepingModel { Depth = (sbyte)i, Strategy = strategy });

                s++;
            }
        }

        protected abstract StrategyBase GetStrategy(short d, IPosition position, TranspositionTable table);

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
}
