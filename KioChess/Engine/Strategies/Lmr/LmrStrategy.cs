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

    protected override sbyte[][] InitializeReductionMinTable()
    {
        var result = new sbyte[2 * Depth][];
        for (int depth = 0; depth < result.Length; depth++)
        {
            result[depth] = new sbyte[128];
            for (int move = 0; move < result[depth].Length; move++)
            {
                if (depth > 3 && move > MinimumMinMoveCount)
                {
                    result[depth][move] = (sbyte)(depth - 2);
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
                if (depth > 3 && move > MinimumMaxMoveCount)
                {
                    result[depth][move] = (sbyte)(depth - 2);
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
