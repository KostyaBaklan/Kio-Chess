using Engine.DataStructures.Hash;
using Engine.Models.Boards;
using Engine.Models.Enums;

namespace Engine.Strategies.Lmr;

public class LmrDeepStrategy : LmrStrategyBase
{
    public LmrDeepStrategy(int depth, Position position, TranspositionTable table = null) : base(depth, position, table)
    {
    }

    public override StrategyType Type => StrategyType.LMRD;

    protected override int[] GetLmrConfig()
    {
        return configurationProvider.AlgorithmConfiguration.LateMoveConfiguration.Lmrd;
    }
}
