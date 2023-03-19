using Engine.Interfaces;
using Engine.DataStructures.Hash;
using Engine.Strategies.End;
using Engine.DataStructures.Moves.Lists;
using Engine.DataStructures;
using Engine.Models.Moves;
using Engine.Strategies.Models;
using Engine.Sorting.Comparers;

namespace Engine.Strategies.Base.Null
{
    public abstract class NullLmrStrategyBase : NullMemoryStrategyBase
    {
        protected readonly bool[] CanReduceDepth;
        protected readonly bool[] CanReduceMove;
        protected readonly byte[][] Reduction;

        protected NullLmrStrategyBase(short depth, IPosition position, TranspositionTable table = null) 
            : base(depth, position, table)
        {
            InitializeSorters(depth, position, MoveSorterProvider.GetAdvanced(position, new HistoryComparer()));

            CanReduceDepth = InitializeReducableDepthTable();
            CanReduceMove = InitializeReducableMoveTable();
            Reduction = InitializeReductionTable();
        }

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

            if (MoveHistory.IsLastMoveNotReducible())
            {
                SetResult(alpha, beta, depth, result, moves);
            }
            else
            {
                int d = depth - 1;
                int b = -beta;
                for (var i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
                    Position.Make(move);

                    int value;
                    if (move.CanReduce && !move.IsCheck && CanReduceMove[i])
                    {
                        value = -Search(b, -alpha, Reduction[depth][i]);
                        if (value > alpha)
                        {
                            value = -Search(b, -alpha, d);
                        }
                    }
                    else
                    {
                        value = -Search(b, -alpha, d);
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

            result.Move.History++;
            return result;
        }

        protected override void SearchInternal(int alpha, int beta, int depth, SearchContext context)
        {
            if (!CanReduceDepth[depth] || MoveHistory.IsLastMoveNotReducible())
            {
                base.SearchInternal(alpha, beta, depth, context);
            }
            else
            {
                MoveBase move;
                int r;
                int d = depth - 1;
                int b = -beta;
                int count = context.Moves.Count; 
                
                if (count < 2)
                {
                    SingleMoveSearch(alpha, beta, depth, context);
                }
                else if (Depth - depth < ExtensionDepthDifference)
                {
                    for (var i = 0; i < count; i++)
                    {
                        move = context.Moves[i];
                        Position.Make(move);

                        int extension = GetExtension(move);

                        if (move.CanReduce && !move.IsCheck && CanReduceMove[i])
                        {
                            r = -Search(b, -alpha, Reduction[depth][i] + extension);
                            if (r > alpha)
                            {
                                r = -Search(b, -alpha, d + extension);
                            }
                        }
                        else
                        {
                            r = -Search(b, -alpha, d + extension);
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
                else
                {
                    for (var i = 0; i < count; i++)
                    {
                        move = context.Moves[i];
                        Position.Make(move);

                        if (move.CanReduce && !move.IsCheck && CanReduceMove[i])
                        {
                            r = -Search(b, -alpha, Reduction[depth][i]);
                            if (r > alpha)
                            {
                                r = -Search(b, -alpha, d);
                            }
                        }
                        else
                        {
                            r = -Search(b, -alpha, d);
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

        protected override StrategyBase CreateEndGameStrategy()
        {
            return new LmrDeepEndGameStrategy((short)Math.Min(Depth + 1, MaxEndGameDepth), Position, Table);
        }

        protected abstract byte[][] InitializeReductionTable();
        protected abstract bool[] InitializeReducableMoveTable();
        protected abstract bool[] InitializeReducableDepthTable();
    }
}
