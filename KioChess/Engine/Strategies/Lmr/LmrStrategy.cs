using Engine.DataStructures.Hash;
using Engine.Models.Boards;
using Engine.Models.Enums;

namespace Engine.Strategies.Lmr;


public class LmrStrategy : LmrStrategyBase
{
    public LmrStrategy(short depth, Position position, TranspositionTable table = null) : base(depth, position, table)
    {
    }

    protected override int MinimumMaxMoveCount => 4;

    protected override int ReducableDepth => 3;
    public override StrategyType Type => StrategyType.LMR;

    protected override sbyte GetOnReducableDepth(int depth, int move, int i)
    {
        return base.GetReducableDepth(depth,move, i);
    }
}
