using Engine.DataStructures;
using Engine.Models.Boards;
using Engine.Models.Enums;

namespace Engine.Strategies.Lmr;

public class LmrDeepStrategy : LmrStrategyBase
{
    public LmrDeepStrategy(int depth, Position position, TranspositionTable table = null, LmrTables lmrTables = null) : base(depth, position, table, lmrTables)
    {
    }

    public override StrategyType Type => StrategyType.LMRD;
}
