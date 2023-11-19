using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Strategies.Base;
using Engine.Strategies.Lmr;
using Engine.Strategies.Models.Contexts;

namespace Engine.Strategies.End;

public class LmrDeepEndGameStrategy : LmrDeepStrategy
{
    private LmrDeepEndGameAbStrategy _strategy;
    public LmrDeepEndGameStrategy(short depth, IPosition position, TranspositionTable table = null)
        : base(depth, position, table)
    {
        ExtensionDepthDifference = configurationProvider
            .AlgorithmConfiguration.ExtensionConfiguration.EndDepthDifference[depth];
    }

    public override IResult GetResult()
    {
        if(_strategy == null) _strategy = new LmrDeepEndGameAbStrategy(Depth, Position, Table);
        //if(Position.GetPhase()!=Phase.End)
        //    return GetResult((short)-SearchValue, SearchValue, (sbyte)(Depth - 1));
        if (IsLateEndGame())
            return GetResult((short)-SearchValue, SearchValue, (sbyte)(Depth + 1));
        return GetResult((short)-SearchValue, SearchValue, Depth);
        //if (IsLateEndGame())
        //    return _strategy.GetResult((short)-SearchValue, SearchValue, (sbyte)(Depth + 1));
        //return _strategy.GetResult((short)-SearchValue, SearchValue, Depth);
    }

    public override IResult GetResult(short alpha, short beta, sbyte depth, MoveBase pv = null)
    {
        Result result = new Result();
        if (IsEndGameDraw(result)) return result;

        if (pv == null && Table.TryGet(Position.GetKey(), out var entry))
        {
            pv = GetPv(entry.PvMove);
        }

        SortContext sortContext = DataPoolService.GetCurrentSortContext();
        sortContext.Set(Sorters[depth], pv);
        MoveList moves = sortContext.GetAllMoves(Position);

        DistanceFromRoot = sortContext.Ply; 
        MaxExtensionPly = DistanceFromRoot + depth + ExtensionDepthDifference;

        if (CheckEndGame(moves.Count, result)) return result;

        if (moves.Count > 1)
        {
            if (MoveHistory.IsLastMoveNotReducible())
            {
                result.Move = pv;
                SetResult(alpha, beta, depth, result, moves);
            }
            else
            {
                short value;
                sbyte d = (sbyte)(depth - 1);
                short b = (short)-beta;
                for (byte i = 0; i < moves.Count; i++)
                {
                    var move = moves[i];
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
                        value = (short)-Search(b, (short)-alpha, (IsPvEnabled && i == 0 && pv != null) ? depth : d);
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

    public override short Search(short alpha, short beta, sbyte depth)
    {
        if (CheckEndGameDraw()) return 0;
        if (depth < 1) return Evaluate(alpha, beta);

        MoveBase pv = null;
        bool shouldUpdate = false;
        bool isInTable = false;

        if (Table.TryGet(Position.GetKey(), out var entry))
        {
            isInTable = true;

            shouldUpdate = entry.Depth < depth;

            pv = GetPv(entry.PvMove);
        }

        SearchContext context = GetCurrentContext(alpha, beta, depth, pv);

        if (SetSearchValue(alpha, beta, depth, context)) return context.Value;

        if (isInTable && !shouldUpdate) return context.Value;

        return StoreValue(depth, context.Value, context.BestMove.Key);
    }

    protected override bool[] InitializeReducableMoveTable()
    {
        var result = new bool[128];
        for (int move = 0; move < result.Length; move++)
        {
            result[move] = move > 3;
        }

        return result;
    }

    protected override sbyte[][] InitializeReductionTable()
    {
        var result = new sbyte[2 * Depth][];
        for (int depth = 0; depth < result.Length; depth++)
        {
            result[depth] = new sbyte[128];
            for (int move = 0; move < result[depth].Length; move++)
            {
                if (depth > 4)
                {
                    if (move > 10)
                    {
                        result[depth][move] = (sbyte)(depth - 3);
                    }
                    else if (move > 3)
                    {
                        result[depth][move] = (sbyte)(depth - 2);
                    }
                    else
                    {
                        result[depth][move] = (sbyte)(depth - 1);
                    }
                }
                else if (depth > 3)
                {
                    if (move > 3)
                    {
                        result[depth][move] = (sbyte)(depth - 2);
                    }
                    else
                    {
                        result[depth][move] = (sbyte)(depth - 1);
                    }
                }
                else
                {
                    result[depth][move] = (sbyte)(depth - 1);
                }

            }
        }

        return result;
    }

    protected override StrategyBase CreateSubSearchStrategy()
    {
        return new LmrDeepEndGameStrategy((short)(Depth - SubSearchDepth), Position);
    }
}
