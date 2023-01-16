using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Lmr;

namespace Engine.Strategies.Aspiration
{
    public class LmrAspirationStrategy : AspirationStrategyBase
    {
        public LmrAspirationStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        protected override void InitializeModels(TranspositionTable table)
        {
            foreach (var aspirationModel in Models)
            {
                aspirationModel.Strategy = new LmrStrategy((short)aspirationModel.Depth, Position, table);
            }
        }
    }
}
