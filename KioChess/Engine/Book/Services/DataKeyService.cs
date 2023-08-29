using Engine.Book.Interfaces;
using Engine.DataStructures;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Book.Services
{
    public class DataKeyService : IDataKeyService
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Get(ref MoveKeyList span)
        {
            if(span.Count == 0) return string.Empty;

            span.Order();

            StringBuilder builder = new StringBuilder();

            byte last = (byte)(span.Count - 1);
            for (byte i = 0; i < last; i++)
            {
                builder.Append($"{span[i]}-");
            }

            builder.Append(span[last]);

            return builder.ToString();
        }
    }
}
