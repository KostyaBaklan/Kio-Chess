using CommonServiceLocator;
using Engine.DataStructures.Moves.Lists;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Moves;
using Engine.Strategies.Models;
using System.Runtime.CompilerServices;
using Engine.Models.Enums;

namespace Engine.Strategies.Base.Null
{
    public abstract class NullStrategyBase : StrategyBase
    {
        protected bool CanUseNull;
        protected bool IsNull;
        protected int MinReduction;
        protected int MaxReduction;
        protected int NullWindow;
        protected int NullDepthReduction;
        protected int NullDepthOffset;

        public NullStrategyBase(short depth, IPosition position) : base(depth, position)
        {
            CanUseNull = false;
            var configuration = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .AlgorithmConfiguration.NullConfiguration;

            MinReduction = configuration.MinReduction;
            MaxReduction = configuration.MaxReduction;
            NullWindow = configuration.NullWindow;
            NullDepthOffset = configuration.NullDepthOffset;
            NullDepthReduction = configuration.NullDepthReduction;
        }

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pv = null)
        {
            CanUseNull = false;
            Result result = new Result();

            if (IsDraw(result))
            {
                return result;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Search(int alpha, int beta, int depth)
        {
            if (depth <= 0) return Evaluate(alpha, beta);

            if (Position.GetPhase() == Phase.End)
                return EndGameStrategy.Search(alpha, beta, Math.Min(depth + 1, MaxEndGameDepth));

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

            SearchContext context = GetCurrentContext(alpha, depth);

            if (context.IsEndGame)
                return context.Value;

            if (context.IsFutility)
            {
                FutilitySearchInternal(alpha, beta, depth, context);
            }
            else
            {
                SearchInternal(alpha, beta, depth, context);
            }

            return context.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SearchInternal(int alpha, int beta, int depth, SearchContext context)
        {
            if (IsNull)
            {
                base.SearchInternal(alpha, beta, depth, context);
            }
            else
            {
                MoveBase move;
                int r;
                int d = depth - 1;
                int b = -beta;

                bool canUseNull = CanUseNull;

                for (var i = 0; i < context.Moves.Count; i++)
                {
                    move = context.Moves[i];

                    Position.Make(move);

                    CanUseNull = i > 0;

                    r = -Search(b, -alpha, d);

                    CanUseNull = canUseNull;

                    Position.UnMake();

                    if (r <= context.Value)
                        continue;

                    context.Value = r;
                    context.BestMove = move;

                    if (context.Value >= beta)
                    {
                        if (!move.IsAttack) Sorters[depth].Add(move.Key);
                        break;
                    }
                    if (context.Value > alpha)
                        alpha = context.Value;
                }

                context.BestMove.History += 1 << depth;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int NullSearch(int alpha, int depth)
        {
            if (depth <= 0) return Evaluate(alpha, alpha + NullWindow);

            SearchContext context = GetCurrentContext(alpha, depth);

            if (context.IsEndGame)
                return context.Value;

            if (context.IsFutility)
            {
                FutilitySearchInternal(alpha, alpha + NullWindow, depth, context);
                if (context.IsEndGame) return Position.GetValue();
            }
            else
            {
                SearchInternal(alpha, alpha + NullWindow, depth, context);
            }

            return context.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetResult(int alpha, int beta, int depth, Result result, MoveList moves)
        {
            for (var i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                Position.Make(move);

                CanUseNull = i > 0;

                var value = -Search(-beta, -alpha, depth - 1);

                Position.UnMake();

                if (value > result.Value)
                {
                    result.Value = value;
                    result.Move = move;
                }

                if (value > alpha)
                {
                    alpha = value;
                }

                if (alpha < beta) continue;
                break;
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //protected bool IsValidWindow(int alpha, int beta)
        //{
        //    return beta < SearchValue && beta - alpha > NullWindow;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UndoNullMove()
        {
            SwitchNull();
            Position.SwapTurn();
            IsNull = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void MakeNullMove()
        {
            SwitchNull();
            Position.SwapTurn();
            IsNull = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SwitchNull()
        {
            CanUseNull = !CanUseNull;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool CanDoNullMove(int depth)
        {
            return CanUseNull && !MoveHistory.IsLastMoveWasCheck() && depth - 1 < Depth;
        }
    }
}
