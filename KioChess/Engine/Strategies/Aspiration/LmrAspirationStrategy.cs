using Engine.DataStructures.Hash;
using Engine.Models.Boards;
using Engine.Strategies.Lmr;

namespace Engine.Strategies.Aspiration;

public class LmrAspirationStrategy : AspirationStrategyBase
{
    public LmrAspirationStrategy(short depth, Position position) : base(depth, position)
    {
    }

    protected override void InitializeModels(TranspositionTable table)
    {
        for (int i = 0; i < Models.Count; i++)
        {
            Models[i].Strategy = new LmrStrategy(Models[i].Depth, Position, table);
        }
    }
}
