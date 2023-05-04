using Engine.Models.Boards;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Engine.Services.Bits
{
    public class IntelBitService : BitServiceBase
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte BitScanForward(BitBoard b)
        {
            return (byte)Bmi1.X64.TrailingZeroCount(b.AsValue());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte Count(BitBoard b)
        {
            return (byte)Popcnt.X64.PopCount(b.AsValue());
        }
    }
}
