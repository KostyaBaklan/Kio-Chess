using Engine.Dal.Interfaces;
using Engine.DataStructures;
using Engine.Models.Helpers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Dal.Services;

public class DataKeyService : IDataKeyService
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetKey(ref MoveKeyList span)
    {
        if (span.Count == 0) return string.Empty;

        span.Order();

        return span.AsKey();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] GetByteKey(ref MoveKeyList span)
    {
        if(span.Count == 0) return new byte[0];

        span.Order();

        return span.AsByteKey();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetKey(byte[] key)
    {
        if(key.Length == 0) return string.Empty;

        unsafe
        {
            Span<short> moves = new Span<short>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(key.AsSpan())), key.Length/2);

            return moves.Join('-');
        }
    }
}
