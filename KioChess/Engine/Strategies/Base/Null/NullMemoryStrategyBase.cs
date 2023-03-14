﻿using CommonServiceLocator;
using Engine.DataStructures.Moves.Lists;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Strategies.Models;
using System.Runtime.CompilerServices;
using Engine.Models.Enums;
using Engine.DataStructures.Hash;
using Engine.Models.Transposition;

namespace Engine.Strategies.Base.Null
{
    public abstract class NullMemoryStrategyBase : NullStrategyBase
    {
        protected readonly TranspositionTable Table;

        protected NullMemoryStrategyBase(short depth, IPosition position, TranspositionTable table = null) : base(depth, position)
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
            if (depth < 1) return Evaluate(alpha, beta);

            if (Position.GetPhase() == Phase.End)
                return EndGameStrategy.Search(alpha, beta, Math.Min(depth + 1, MaxEndGameDepth));

            MoveBase pv = null;
            bool shouldUpdate = false;
            bool isInTable = false;

            if (Table.TryGet(Position.GetKey(), out var entry))
            {
                isInTable = true;

                if (entry.Depth < depth)
                {
                    shouldUpdate = true;
                }
                else
                {
                    if (entry.Value >= beta)
                        return entry.Value;

                    if (entry.Value > alpha)
                        alpha = entry.Value;
                }

                pv = GetPv(entry.PvMove);
            }

            if (CheckDraw()) return 0;

            if (CanDoNullMove(depth))
            {
                MakeNullMove();
                var v = -NullSearch(-beta, depth - NullDepthReduction - 1);
                UndoNullMove();
                if (v >= beta)
                {
                    return v;
                }
            }

            SearchContext context = GetCurrentContext(alpha, beta, depth, pv);

            if (context.IsEndGame)
                return context.Value;

            if (context.IsFutility)
            {
                FutilitySearchInternal(alpha, beta, depth, context);
                if (context.IsEndGame) return Position.GetValue();
            }
            else
            {
                SearchInternal(alpha, beta, depth, context);
            }

            if (IsNull) return context.Value;

            if (isInTable && !shouldUpdate) return context.Value;

            return StoreValue((byte)depth, (short)context.Value, context.BestMove.Key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int StoreValue(byte depth, short value, short bestMove)
        {
            Table.Set(Position.GetKey(), new TranspositionEntry { Depth = depth, Value = value, PvMove = bestMove });

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
            return pv.IsWhite && turn != Turn.White || pv.IsBlack && turn != Turn.Black
                ? null
                : pv;
        }
    }
}