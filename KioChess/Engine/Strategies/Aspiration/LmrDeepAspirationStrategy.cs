using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Strategies.Lmr;

namespace Engine.Strategies.Aspiration
{
    public class LmrDeepAspirationStrategy : AspirationStrategyBase
    {
        public LmrDeepAspirationStrategy(short depth, IPosition position) : base(depth, position)
        {
        }

        protected override void InitializeModels(TranspositionTable table)
        {
            Models[0].Strategy = new LmrAdvancedDeepStrategy((short)Models[0].Depth, Position, table);

            for (int i = 1; i < Models.Count; i++)
            {
                Models[i].Strategy = new LmrDeepStrategy((short)Models[i].Depth, Position, table);
            }
        }
    }
}
