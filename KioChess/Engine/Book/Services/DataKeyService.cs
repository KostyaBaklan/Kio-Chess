using Engine.Book.Interfaces;
using Engine.DataStructures;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Book.Services
{
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
    }
}
