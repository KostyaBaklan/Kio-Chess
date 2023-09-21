using Engine.DataStructures;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Strategies.Lmr;
using Engine.Strategies.Models;

namespace SequenceTool
{
    internal class SequenceStrategy : LmrDeepStrategy
    {
        public SequenceStrategy(IPosition position) : base(7, position, null)
        {
        }

        public IResult GetResult(HashSet<short> movesToExclude)
        {
            short alpha = (short)-SearchValue;
            short beta = SearchValue;
            sbyte depth = Depth;

            Result result = new Result();
            if (IsDraw(result))
            {
                return result;
            }

            SortContext sortContext = DataPoolService.GetCurrentSortContext();
            sortContext.Set(Sorters[Depth]);
            MoveList ms = Position.GetAllMoves(sortContext);

            MoveList list = new MoveList(ms.Count);

            for (byte i = 0; i < ms.Count; i++)
            {
                if (!movesToExclude.Contains(ms[i].Key))
                {
                    list.Add(ms[i]);
                }
            }

            DistanceFromRoot = sortContext.Ply;
            MaxExtensionPly = DistanceFromRoot + Depth + ExtensionDepthDifference;

            if (CheckEndGame(list.Count, result)) return result;

            if (MoveHistory.IsLastMoveNotReducible())
            {
                SetResult(alpha, beta, depth, result, list);
            }
            else
            {
                short value;
                sbyte d = (sbyte)(depth - 1);
                short b = (short)-beta;
                for (byte i = 0; i < list.Count; i++)
                {
                    var move = list[i];
                    Position.Make(move);

                    if (move.CanReduce && !move.IsCheck && CanReduceMove[i])
                    {
                        value = (short)-Search(b, (short)-alpha, Reduction[depth][i]);
                        if (value > alpha)
                        {
                            value = (short)-Search(b, (short)-alpha, d);
                        }
                    }
                    else
                    {
                        value = (short)-Search(b, (short)-alpha, d);
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
    }
}
