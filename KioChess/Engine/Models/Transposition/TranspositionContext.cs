
using Engine.Models.Moves;

namespace Engine.Models.Transposition
{
    public struct TranspositionContext
    {
        public MoveBase Pv;
        public bool NotShouldUpdate;
        public bool IsBetaExceeded;

        public TranspositionContext()
        {
            Pv = null;
            NotShouldUpdate = false;
            IsBetaExceeded = false;
        }
    }
}
