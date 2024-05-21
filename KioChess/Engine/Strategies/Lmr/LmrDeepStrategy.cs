using Engine.DataStructures.Hash;
using Engine.Models.Boards;

namespace Engine.Strategies.Lmr;

public class LmrDeepStrategy : LmrStrategyBase
{
    public LmrDeepStrategy(int depth, Position position, TranspositionTable table = null) : base(depth, position, table)
    {
    }

    protected override int MinimumMaxMoveCount => 4;

    protected override int MinimumMinMoveCount => 3;

    protected override bool[] InitializeReducableDepthTable()
    {
        var result = new bool[2 * Depth];
        for (int depth = 0; depth < result.Length; depth++)
        {
            result[depth] = depth > 2;
        }

        return result;
    }

    protected override sbyte[][] InitializeReductionMinTable()
    {
        var result = new sbyte[2 * Depth][];
        for (int depth = 0; depth < result.Length; depth++)
        {
            result[depth] = new sbyte[128];
            for (int move = 0; move < result[depth].Length; move++)
            {
                if (depth > 5)
                {
                    if (move > 10)
                    {
                        result[depth][move] = (sbyte)(depth - 3);
                    }
                    else if (move > MinimumMinMoveCount)
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
                    if (move > 12)
                    {
                        result[depth][move] = (sbyte)(depth - 3);
                    }
                    else if (move > MinimumMinMoveCount)
                    {
                        result[depth][move] = (sbyte)(depth - 2);
                    }
                    else
                    {
                        result[depth][move] = (sbyte)(depth - 1);
                    }
                }
                else if (depth > 2)
                {
                    if (move > MinimumMinMoveCount)
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

    protected override sbyte[][] InitializeReductionMaxTable()
    {
        var result = new sbyte[2 * Depth][];
        for (int depth = 0; depth < result.Length; depth++)
        {
            result[depth] = new sbyte[128];
            for (int move = 0; move < result[depth].Length; move++)
            {
                if (depth > 5)
                {
                    if (move > 12)
                    {
                        result[depth][move] = (sbyte)(depth - 3);
                    }
                    else if (move > MinimumMaxMoveCount)
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
                    if (move > 14)
                    {
                        result[depth][move] = (sbyte)(depth - 3);
                    }
                    else if (move > MinimumMaxMoveCount)
                    {
                        result[depth][move] = (sbyte)(depth - 2);
                    }
                    else
                    {
                        result[depth][move] = (sbyte)(depth - 1);
                    }
                }
                else if (depth > 2)
                {
                    if (move > MinimumMaxMoveCount)
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
}
