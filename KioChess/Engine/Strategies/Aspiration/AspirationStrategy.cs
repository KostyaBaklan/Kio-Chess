using Engine.DataStructures.Hash;
using Engine.Models.Boards;

namespace Engine.Strategies.Aspiration
{
    public class AspirationStrategy : AspirationStrategyBase
    {
        public AspirationStrategy(short depth, Position position, TranspositionTable table = null)
            : base(depth, position, table)
        {
        }
    }
}
