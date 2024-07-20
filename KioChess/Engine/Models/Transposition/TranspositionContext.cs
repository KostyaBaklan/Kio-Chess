
using System.Runtime.InteropServices;

namespace Engine.Models.Transposition
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TranspositionContext
    {
        public short Pv;
        public bool ShouldUpdate;
        public bool IsBetaExceeded;

        public TranspositionContext(short pv)
        {
            Pv = pv;
            ShouldUpdate = true;
            IsBetaExceeded = false;
        }
    }
}
