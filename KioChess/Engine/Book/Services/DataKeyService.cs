using Engine.Book.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Book.Services
{
    public class DataKeyService : IDataKeyService
    {
        private readonly short _depth;
        private readonly LinkedList<string> _keys;

        public DataKeyService(IConfigurationProvider configurationProvider)
        {
            _depth = configurationProvider.BookConfiguration.Depth;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Get(IEnumerable<MoveBase> history)
        {
            return string.Join("-", history.Take(_depth).Select(m => m.Key.ToString()));
        }
    }
}
