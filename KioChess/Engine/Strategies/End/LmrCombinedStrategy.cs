using Engine.DataStructures;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Strategies.Models;

namespace Engine.Strategies.End
{
    public class LmrCombinedStrategy : LmrEndGameStrategy
    {
        private int EndGameDepthOffset;

        public LmrCombinedStrategy(short depth, IPosition position) : base(depth, position)
        {
            EndGameDepthOffset = configurationProvider.EndGameConfiguration.EndGameDepthOffset;
        }

        public override IResult GetResult(int alpha, int beta, int depth, MoveBase pvMove = null)
        {
            Result result = new Result();
            if (IsDraw(result))
            {
                return result;
            }

            SortContext sortContext = DataPoolService.GetCurrentSortContext();
            sortContext.Set(Sorters[Depth]);
            MoveList moves = Position.GetAllMoves(sortContext);

            if (CheckEndGame(moves.Count, result)) return result;

            if (moves.Count > 1)
            {
                moves = SubSearch(moves, alpha, beta, depth);

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

            if (CheckEndGameDraw()) return 0;

            bool useCache = depth < Depth - EndGameDepthOffset;

            MoveBase pv = null;
            bool shouldUpdate = false;
            bool isInTable = false;

            if (useCache && Table.TryGet(Position.GetKey(), out var entry))
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

            if (!useCache||(isInTable && !shouldUpdate)) return context.Value;

            return StoreValue((byte)depth, (short)context.Value, context.BestMove.Key);
        }
    }
}
