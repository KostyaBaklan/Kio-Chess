using System.Collections;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures
{
    public sealed class ShortHashSet:IEnumerable<ushort>
    {
        private const short Empty = 0;
        private const int _mask = 31;

        private readonly ushort[] _keys;  

        public ShortHashSet()
        {
            _keys = new ushort[32];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ushort v)
        {
            int idx = Hash(v) & _mask; 

            // Linear probing
            for (int probes = 0; probes < _keys.Length; probes++)
            {
                ushort cur = _keys[idx];
                if (cur == Empty) return false;   // hit empty slot => not present
                if (cur == v) return true;        // matc

                idx = (idx + 1) & _mask;
            }
            return false; // table full (shouldn't happen)
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ushort v)
        {
            int idx = Hash(v) & _mask; 

            for (; ; )
            {
                ushort cur = _keys[idx];
                if (cur == Empty)
                {
                    _keys[idx] = v;
                    return;
                }
                if (cur == v)
                    return; // already present

                idx = (idx + 1) & _mask;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Array.Clear(_keys, 0, _keys.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Hash(ushort v)
        {
            unchecked
            {
                return v * 40503; // 40503 = 2654435761 mod 2^16 }
            }
        }

        public IEnumerator<ushort> GetEnumerator()
        {
            for (int i = 0; i < _keys.Length; i++)
            {
                ushort v = _keys[i];
                if (v != Empty)
                    yield return v;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}