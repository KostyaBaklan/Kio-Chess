using Engine.DataStructures.Hash;
using Engine.Models.Boards;

namespace Engine.Strategies.Lmr;


public class LmrStrategy : LmrStrategyBase
{
    public LmrStrategy(short depth, Position position, TranspositionTable table = null) : base(depth, position, table)
    {
    }

    protected override int MinimumMaxMoveCount => 4;

    protected override int MinimumMinMoveCount => 3;

    protected override bool[] InitializeReducableDepthTable()
    {
        var result = new bool[2 * Depth];
        for (int depth = 0; depth < result.Length; depth++)
        {
            result[depth] = depth > 3;
        }

        return result;
    }

    protected override sbyte[][][] InitializeReductionMinTable()
    {
        var result = new sbyte[2 * Depth][][];
        for (int depth = 0; depth < result.Length; depth++)
        {
            result[depth] = new sbyte[MaxMoveCount][];
            for (int move = 0; move < result[depth].Length; move++)
            {
                result[depth][move] = new sbyte[move];
                for (int i = 0; i < result[depth][move].Length; i++)
                {
                    if (depth > 3 && move > MinimumMinMoveCount)
                    {
                        result[depth][move][i] = (sbyte)(depth - 2);
                    }
                    else
                    {
                        result[depth][move][i] = (sbyte)(depth - 1);
                    } 
                }
            }
        }

        return result;
    }

    protected override sbyte[][][] InitializeReductionMaxTable()
    {
        var result = new sbyte[2 * Depth][][];
        for (int depth = 0; depth < result.Length; depth++)
        {
            result[depth] = new sbyte[MaxMoveCount][];
            for (int move = 0; move < result[depth].Length; move++)
            {
                result[depth][move] = new sbyte[move];
                for (int i = 0; i < result[depth][move].Length; i++)
                {
                    if (depth > 3 && move > MinimumMaxMoveCount)
                    {
                        result[depth][move][i] = (sbyte)(depth - 2);
                    }
                    else
                    {
                        result[depth][move][i] = (sbyte)(depth - 1);
                    }
                }
            }
        }

        return result;
    }
}
