﻿using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Base;

namespace Engine.Strategies.Lmr;

public class LmrDeepStrategy : LmrStrategyBase
{
    public LmrDeepStrategy(int depth, IPosition position, TranspositionTable table = null) : base(depth, position, table)
    {
    }

    protected override StrategyBase CreateSubSearchStrategy() => new LmrDeepStrategy((short)(Depth - SubSearchDepth), Position);
    protected override bool[] InitializeReducableDepthTable()
    {
        var result = new bool[2 * Depth];
        for (int depth = 0; depth < result.Length; depth++)
        {
            result[depth] = depth > 2;
        }

        return result;
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
                if (depth > 5)
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
                    if (move > 12)
                    {
                        result[depth][move] = (sbyte)(depth - 3);
                    }
                    else if (move > 4)
                    {
                        result[depth][move] = (sbyte)(depth - 2);
                    }
                    else
                    {
                        result[depth][move] = (sbyte)(depth - 1);
                    }
                }
                else if (depth == 3)
                {
                    if (move > 5)
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
