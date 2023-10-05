using Engine.Dal.Interfaces;
using Engine.DataStructures;
using System.Runtime.CompilerServices;

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

        short[] moves = new short[key.Length/2];
        Buffer.BlockCopy(key,0,moves,0,key.Length);

        return string.Join("-", moves);
    }
}
