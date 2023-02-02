using System.Runtime.CompilerServices;
using Engine.Models.Boards;

namespace Engine.DataStructures
{
    public class PieceSet
    {
        private readonly Square[] _items;
        private readonly int[] _occuped;
        private readonly int[] _index;
        private int _lastOccuped;

        public PieceSet()
        {
            var size = 64;
            _items = new Square[size];
            _occuped = new int[10];
            _index = new int[size];
            for (var i = 0; i < _items.Length; i++)
            {
                _items[i] = new Square(i);
            }
        }

        public int Count
        {
            get { return _lastOccuped; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(byte rank)
        {
            _occuped[_lastOccuped] = rank;
            _index[rank] = _lastOccuped++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(byte rank)
        {
            var occuped = _index[rank];
            --_lastOccuped;

            if (_lastOccuped <= occuped) return;

            _occuped[occuped] = _occuped[_lastOccuped];
            _index[_occuped[_lastOccuped]] = occuped;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Square> Items()
        {
            for (int i = 0; i < _lastOccuped; i++)
            {
                yield return _items[_occuped[i]];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<int> Coordinates()
        {
            for (int i = 0; i < _lastOccuped; i++)
            {
                yield return _occuped[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(params int[] items)
        {
            foreach (var item in items)
            {
                Add((byte) item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Replace(byte from, byte to)
        {
            var occuped = _index[from];
            _index[to] = occuped;
            _occuped[occuped] = to;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Square[] GetPiecePositions()
        {
            Square[] positions = new Square[_lastOccuped];
            for (var i = 0; i < positions.Length; i++)
            {
                positions[i] = _items[_occuped[i]];
            }

            return positions;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<int> Coordinates(byte from)
        {
            for (int i = 0; i < _lastOccuped; i++)
            {
                if (_occuped[i] != from)
                {
                    yield return _occuped[i];
                }
            }
        }
    }
}
