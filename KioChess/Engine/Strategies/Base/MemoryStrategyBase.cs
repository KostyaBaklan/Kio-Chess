using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Models.Transposition;
using Engine.Strategies.Models;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Base
{
    public abstract class MemoryStrategyBase : StrategyBase
    {
        protected readonly TranspositionTable Table;
        protected MemoryStrategyBase(short depth, IPosition position, TranspositionTable table = null) : base(depth, position)
        {
            if (table == null)
            {
                var service = ServiceLocator.Current.GetInstance<ITranspositionTableService>();

                Table = service.Create(depth);
            }
            else
            {
                Table = table;
            }
        }
        public override int Size => Table.Count;

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null)
        {
            Result result = new Result();
            if (IsDraw(result))
            {
                return result;
            }

            MoveBase pv = pvMove;
            if (pv == null)
            {
                if (Table.TryGet(Position.GetKey(), out var entry))
                {
                    pv = GetPv(entry.PvMove);
                }
            }
            SortContext sortContext = DataPoolService.GetCurrentSortContext();
            sortContext.Set(Sorters[Depth], pv);
            MoveList moves = Position.GetAllMoves(sortContext);

            if (CheckEndGame(moves.Count, result)) return result;

            if (moves.Count > 1)
            {
                moves = SubSearch(moves, alpha, beta, depth);

                SetResult(alpha, beta, depth, result, moves);
            }
            else
            {
                result.Move = moves[0];
            }

            result.Move.History++;
            return result;
        }

        public override int Search(int alpha, int beta, int depth)
        {
            if (depth <= 0) return Evaluate(alpha, beta);

            if (Position.GetPhase() == Phase.End)
                return EndGameStrategy.Search(alpha, beta, depth);

            MoveBase pv = null;
            bool shouldUpdate = false;
            bool isInTable = false;

            if (Table.TryGet(Position.GetKey(), out var entry))
            {
                isInTable = true;
                var entryDepth = entry.Depth;

                if (entryDepth >= depth)
                {
                    if (entry.Value > alpha)
                    {
                        alpha = entry.Value;
                        if (alpha >= beta)
                            return alpha;
                    }
                }
                else
                {
                    shouldUpdate = true;
                }

                pv = GetPv(entry.PvMove);
            }

            if (CheckDraw()) return 0;

            SearchContext context = GetCurrentContext(alpha, depth, pv);

            if (context.IsEndGame)
            {
                return context.Value;
            }

            if (context.IsFutility)
            {
                FutilitySearchInternal(alpha, beta, depth, context);
                if (context.IsEndGame) return Position.GetValue();
            }
            else
            {
                SearchInternal(alpha, beta, depth, context);
            }

            if (isInTable && !shouldUpdate) return context.Value;

            return StoreValue((byte)depth, (short)context.Value, context.BestMove.Key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int StoreValue(byte depth, short value, short bestMove)
        {
            TranspositionEntry te = new TranspositionEntry
            { Depth = depth, Value = value, PvMove = bestMove };

            Table.Set(Position.GetKey(), te);

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Table.Clear();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected MoveBase GetPv(short entry)
        {
            var pv = MoveProvider.Get(entry);
            var turn = Position.GetTurn();
            return pv.Piece.IsWhite() && turn != Turn.White || pv.Piece.IsBlack() && turn != Turn.Black
                ? null
                : pv;
        }
    }
}
