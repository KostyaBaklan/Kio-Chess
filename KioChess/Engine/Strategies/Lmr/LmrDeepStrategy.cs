using Engine.DataStructures.Hash;
using Engine.Models.Boards;
using Engine.Models.Enums;

namespace Engine.Strategies.Lmr;

public class LmrDeepStrategy : LmrStrategyBase
{
    public LmrDeepStrategy(int depth, Position position, TranspositionTable table = null) : base(depth, position, table)
    {
    }

    protected override int MinimumMaxMoveCount => 4;

    protected override int ReducableDepth => 2;
    public override StrategyType Type => StrategyType.LMRD;

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
                    if (depth > ReducableDepth+1)
                    {
                        if (move > 50)
                        {
                            if (i > 20)
                            {
                                result[depth][move][i] = (sbyte)(depth - 3);
                            }
                            else if (i > MinimumMaxMoveCount)
                            {
                                result[depth][move][i] = (sbyte)(depth - 2);
                            }
                            else
                            {
                                result[depth][move][i] = (sbyte)(depth - 1);
                            }
                        }
                        else if (move > 40)
                        {
                            if (i > 18)
                            {
                                result[depth][move][i] = (sbyte)(depth - 3);
                            }
                            else if (i > MinimumMaxMoveCount)
                            {
                                result[depth][move][i] = (sbyte)(depth - 2);
                            }
                            else
                            {
                                result[depth][move][i] = (sbyte)(depth - 1);
                            }
                        }
                        else if (move > 30)
                        {
                            if (i > 15)
                            {
                                result[depth][move][i] = (sbyte)(depth - 3);
                            }
                            else if (i > MinimumMaxMoveCount)
                            {
                                result[depth][move][i] = (sbyte)(depth - 2);
                            }
                            else
                            {
                                result[depth][move][i] = (sbyte)(depth - 1);
                            }
                        }
                        else if (move > 20)
                        {
                            if (i > 12)
                            {
                                result[depth][move][i] = (sbyte)(depth - 3);
                            }
                            else if (i > MinimumMaxMoveCount)
                            {
                                result[depth][move][i] = (sbyte)(depth - 2);
                            }
                            else
                            {
                                result[depth][move][i] = (sbyte)(depth - 1);
                            }
                        }
                        else
                        {
                            if (i > 10)
                            {
                                result[depth][move][i] = (sbyte)(depth - 3);
                            }
                            else if (i > MinimumMaxMoveCount)
                            {
                                result[depth][move][i] = (sbyte)(depth - 2);
                            }
                            else
                            {
                                result[depth][move][i] = (sbyte)(depth - 1);
                            }
                        }
                    }
                    else if (depth > ReducableDepth)
                    {
                        if (i > MinimumMaxMoveCount)
                        {
                            result[depth][move][i] = (sbyte)(depth - 2);
                        }
                        else
                        {
                            result[depth][move][i] = (sbyte)(depth - 1);
                        }
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
