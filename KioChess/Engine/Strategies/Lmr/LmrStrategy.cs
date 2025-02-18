using Engine.DataStructures.Hash;
using Engine.Models.Boards;
using Engine.Models.Enums;

namespace Engine.Strategies.Lmr;


public class LmrStrategy : LmrStrategyBase
{
    public LmrStrategy(short depth, Position position, TranspositionTable table = null) : base(depth, position, table)
    {
    }

    public override StrategyType Type => StrategyType.LMR;

    protected override int[] GetLmrConfig()
    {
        return configurationProvider.AlgorithmConfiguration.LateMoveConfiguration.Lmr;
    }

    protected override sbyte GetOnReducableDepth(int depth, int move, int i)
    {
        return base.GetReducableDepth(depth,move, i);
    }
}
