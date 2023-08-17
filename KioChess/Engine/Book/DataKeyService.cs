using System.Runtime.CompilerServices;

namespace Engine.Book
{
    public class DataKeyService : IDataKeyService
    {
        private readonly LinkedList<string> _keys;
        public DataKeyService()
        {
            _keys = new LinkedList<string>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Get()
        {
            return string.Join("-", _keys);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(short key)
        {
            _keys.AddLast(key.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Delete()
        {
            _keys.RemoveLast();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _keys.Clear();
        }
    }
}
