using Engine.Interfaces.Config;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Lists
{
    public class MoveHistoryList
    {
        protected static byte Zero = 0;
        public readonly MoveHistory[] _items;

        public MoveHistoryList() 
            : this(ContainerLocator.Current.Resolve<IConfigurationProvider>().GeneralConfiguration.MaxMoveCount)
        {
        }

        public MoveHistory this[byte i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _items[i];
            }
        }

        #region Implementation of IReadOnlyCollection<out IMove>

        public byte Count;

        protected MoveHistoryList(int capacity)
        {
            _items = new MoveHistory[capacity];
        }

        #endregion


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(MoveHistory move) => _items[Count++] = move;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(MoveHistoryList moves)
        {
            moves.AsSpan().CopyTo(_items.AsSpan(Count,moves.Count));
            Count += moves.Count;

            //for (byte i = Zero; i < moves.Count; i++)
            //{
            //    Add(moves[i]);
            //}
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(MoveHistory move)
        {
            byte position = Count;

            _items[Count++] = move;

            byte parent = Parent(position);

            var items = AsSpan();

            while (position > 0 && items[position].IsGreater(items[parent]))
            {
                var temp = items[position];
                items[position] = items[parent];
                items[parent] = temp;

                position = parent;
                parent = Parent(position);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected byte Parent(byte i) => (byte)((i - 1) / 2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Count = Zero;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SortBySee()
        {
            byte count = Count;
            byte capturesCount = Sorting.Sort.SortAttackMinimum[count];

            var items = AsSpan();

            for (byte i = Zero; i < capturesCount; i++)
            {
                byte index = i;
                var max = items[i];
                for (byte j = (byte)(i + 1); j < count; j++)
                {
                    if (!items[j].IsGreater(max))
                        continue;

                    max = items[j];
                    index = j;
                }

                if (index == i) continue;

                items[index] = items[i];
                items[i] = max;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SortAndCopy(MoveHistoryList moveList)
        {
            if (moveList.Count < 2)
            {
                Add(moveList[0]);
            }
            else
            {
                moveList.AsSpan().InsertionSort();

                Add(moveList);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Span<MoveHistory> AsSpan()
        {
            return new Span<MoveHistory>(_items, 0, Count);
        }
    }
}
