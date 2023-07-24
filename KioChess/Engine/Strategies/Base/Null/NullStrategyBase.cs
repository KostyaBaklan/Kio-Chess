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
        protected sbyte MaxReduction;
        protected int NullWindow;
        protected int NullDepthReduction;
        protected int NullDepthOffset;

        public NullStrategyBase(short depth, IPosition position) : base(depth, position)
        {
            CanUseNull = false;
            var configuration = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .AlgorithmConfiguration.NullConfiguration;

            MinReduction = configuration.MinReduction;
            MaxReduction = (sbyte)configuration.MaxReduction;
            NullWindow = configuration.NullWindow;
            NullDepthOffset = configuration.NullDepthOffset;
            NullDepthReduction = configuration.NullDepthReduction;
        }

        public override IResult GetResult(short alpha, short beta, sbyte depth, MoveBase pv = null)
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

            DistanceFromRoot = sortContext.Ply; MaxExtensionPly = DistanceFromRoot + Depth + ExtensionDepthDifference;

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
        public override short Search(short alpha, short beta, sbyte depth)
        {
            if (depth < 1) return Evaluate(alpha, beta);

            if (Position.GetPhase() == Phase.End)
                return EndGameStrategy.Search(alpha, beta, (sbyte)Math.Min(depth + 1, MaxEndGameDepth));

            if (CheckDraw()) {return (short)-Position.GetValue();}

            if (CanDoNullMove(depth))
            {
                MakeNullMove();
                short v = (short)-NullSearch((short)-beta, (sbyte)(depth - NullDepthReduction - 1));
                UndoNullMove();
                if (v >= beta)
                {
                    return v;
                }
            }

            SearchContext context = GetCurrentContext(alpha, beta, depth);

            if(SetSearchValue(alpha, beta, depth, context))return context.Value;

            return context.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ExtensibleSearchInternal(short alpha, short beta, sbyte depth, SearchContext context)
        {
            MoveBase move;
            short r;
            sbyte d = (sbyte)(depth - 1);
            short b = (short)-beta;

            bool canUseNull = CanUseNull;

            for (byte i = 0; i < context.Moves.Count; i++)
            {
                move = context.Moves[i];

                Position.Make(move);

                sbyte extension = GetExtension(move);

                CanUseNull = i > 0;

                r = (short)-Search(b, (short)-alpha, (sbyte)(d + extension));

                CanUseNull = canUseNull;

                Position.UnMake();

                if (r <= context.Value)
                    continue;

                context.Value = r;
                context.BestMove = move;

                if (r >= beta)
                {
                    if (!move.IsAttack) Sorters[depth].Add(move.Key);
                    break;
                }
                if (r > alpha)
                    alpha = r;
            }

            context.BestMove.History += 1 << depth;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SearchInternal(short alpha, short beta, sbyte depth, SearchContext context)
        {
            if (IsNull)
            {
                base.SearchInternal(alpha, beta, depth, context);
            }
            else if (MaxExtensionPly > context.Ply)
            {
                ExtensibleSearch(alpha, beta, depth, context);
            }
            else
            {
                RegularSearch(alpha, beta, depth, context);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void RegularSearch(short alpha, short beta, sbyte depth, SearchContext context)
        {
            MoveBase move;
            short r;
            sbyte d = (sbyte)(depth - 1);
            short b = (short)-beta;

            bool canUseNull = CanUseNull;

            for (byte i = 0; i < context.Moves.Count; i++)
            {
                move = context.Moves[i];

                Position.Make(move);

                CanUseNull = i > 0;

                r = (short)-Search(b, (short)-alpha, d);

                CanUseNull = canUseNull;

                Position.UnMake();

                if (r <= context.Value)
                    continue;

                context.Value = r;
                context.BestMove = move;

                if (r >= beta)
                {
                    if (!move.IsAttack) Sorters[depth].Add(move.Key);
                    break;
                }
                if (r > alpha)
                    alpha = r;
            }

            context.BestMove.History += 1 << depth;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected short NullSearch(short alpha, sbyte depth)
        {
            short beta = (short)(alpha + NullWindow);
            if (depth < 1) return Evaluate(alpha, beta);

            SearchContext context = GetCurrentContext(alpha, beta, depth);

            if(SetSearchValue(alpha, beta, depth, context))return context.Value;

            return context.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetResult(short alpha, short beta, sbyte depth, Result result, MoveList moves)
        {
            for (byte i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                Position.Make(move);

                CanUseNull = i > 0;

                short value = (short)-Search((short)-beta, (short)-alpha, (sbyte)(depth - 1));

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
