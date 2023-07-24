using Engine.DataStructures;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using Engine.Strategies.Base;
using Engine.Strategies.Models;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.End
{
    public class LmrNoCacheStrategy : StrategyBase
    {
        protected int DepthReduction;
        protected int LmrDepthThreshold;
        protected int LmrDepthLimitForReduce;

        public LmrNoCacheStrategy(short depth, IPosition position) : base(depth, position)
        {
            LmrDepthThreshold = configurationProvider
                .AlgorithmConfiguration.LateMoveConfiguration.LmrDepthThreshold;
            DepthReduction = configurationProvider
                .AlgorithmConfiguration.LateMoveConfiguration.LmrDepthReduction;

            LmrDepthLimitForReduce = DepthReduction + 2;
            ExtensionDepthDifference = configurationProvider
                .AlgorithmConfiguration.EndExtensionDepthDifference[depth];

            InitializeSorters(depth, position, MoveSorterProvider.GetAdvanced(position, new HistoryComparer()));
        }

        public override IResult GetResult()
        {
            return GetResult((short)-SearchValue, SearchValue, Depth);
        }

        public override IResult GetResult(short alpha, short beta, sbyte depth, MoveBase pvMove = null)
        {
            Result result = new Result();
            if (IsEndGameDraw(result)) return result;

            SortContext sortContext = DataPoolService.GetCurrentSortContext();
            sortContext.Set(Sorters[Depth]);
            MoveList moves = Position.GetAllMoves(sortContext);

            DistanceFromRoot = sortContext.Ply; MaxExtensionPly = DistanceFromRoot + Depth + ExtensionDepthDifference;

            if (CheckEndGame(moves.Count, result)) return result;

            if (moves.Count > 1)
            {
                if (MoveHistory.IsLastMoveNotReducible())
                {
                    SetResult(alpha, beta, depth, result, moves);
                }
                else
                {
                    for (byte i = 0; i < moves.Count; i++)
                    {
                        var move = moves[i];
                        Position.Make(move);

                        short value;
                        if (alpha > -SearchValue && i > LmrDepthThreshold && move.CanReduce && !move.IsCheck)
                        {
                            value = (short)-Search((short)-beta, (short)-alpha, (sbyte)(depth - DepthReduction));
                            if (value > alpha)
                            {
                                value = (short)-Search((short)-beta, (short)-alpha, (sbyte)(depth - 1));
                            }
                        }
                        else
                        {
                            value = (short)-Search((short)-beta, (short)-alpha, (sbyte)(depth - 1));
                        }

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

            if (CheckEndGameDraw()) {return (short)-Position.GetValue();}

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

            for (byte i = 0; i < context.Moves.Count; i++)
            {
                move = context.Moves[i];
                Position.Make(move);

                int extension = GetExtension(move);

                if (i > LmrDepthThreshold && move.CanReduce && !move.IsCheck)
                {
                    r = (short)-Search(b, (short)-alpha, (sbyte)(depth - DepthReduction + extension));
                    if (r > alpha)
                    {
                        r = (short)-Search(b, (short)-alpha, (sbyte)(d + extension));
                    }
                }
                else
                {
                    r = (short)-Search(b, (short)-alpha, (sbyte)(d + extension));
                }

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
            if (LmrDepthLimitForReduce > depth || MoveHistory.IsLastMoveNotReducible())
            {
                if (MaxExtensionPly > context.Ply)
                {
                    base.ExtensibleSearchInternal(alpha, beta, depth, context);
                }
                else
                {
                    base.RegularSearch(alpha, beta, depth, context);
                }
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

        protected override void RegularSearch(short alpha, short beta, sbyte depth, SearchContext context)
        {
            MoveBase move;
            short r;
            sbyte d = (sbyte)(depth - 1);
            short b = (short)-beta;

            for (byte i = 0; i < context.Moves.Count; i++)
            {
                move = context.Moves[i];
                Position.Make(move);

                if (i > LmrDepthThreshold && move.CanReduce && !move.IsCheck)
                {
                    r = (short)-Search(b, (short)-alpha, (sbyte)(depth - DepthReduction));
                    if (r > alpha)
                    {
                        r = (short)-Search(b, (short)-alpha, d);
                    }
                }
                else
                {
                    r = (short)-Search(b, (short)-alpha, d);
                }

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
    }
}
