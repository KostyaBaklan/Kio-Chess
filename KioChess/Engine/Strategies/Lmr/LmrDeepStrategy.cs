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
}
