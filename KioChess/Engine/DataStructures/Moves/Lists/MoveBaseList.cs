using Engine.Interfaces.Config;
using Engine.Models.Moves;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.DataStructures.Moves.Lists;


public abstract class MoveBaseList<T> : IEnumerable<T> where T : MoveBase
{
    protected static byte Zero = 0;
    public readonly T[] _items;
    public static MoveBase[] Moves;

    protected MoveBaseList() : this(ContainerLocator.Current.Resolve<IConfigurationProvider>().GeneralConfiguration.MaxMoveCount)
    {
    }

    public T this[byte i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return _items[i];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() => new(_items, 0, Count);

    #region Implementation of IReadOnlyCollection<out IMove>

    public byte Count;

    protected MoveBaseList(int capacity)
    {
        _items = new T[capacity];
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T move)
    {
        // Unsafe optimized add - eliminates StelemRef_Helper overhead
        ref T slot = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_items), Count);
        slot = move;
        Count++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Span<T> moves)
    {
        ref T destination = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_items), Count);

        for (int i = 0; i < moves.Length; i++)
        {
            Unsafe.Add(ref destination, i) = moves[i];
        }
        Count += (byte)moves.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T[] moves)
    {
        ref T destination = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_items), Count);
        ref T source = ref MemoryMarshal.GetArrayDataReference(moves);

        for (int i = 0; i < moves.Length; i++)
        {
            Unsafe.Add(ref destination, i) = Unsafe.Add(ref source, i);
        }
        Count += (byte)moves.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => Count = Zero;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected byte Left(byte i) => (byte)(2 * i + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected byte Parent(byte i) => (byte)((i - 1) / 2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Swap(byte i, byte j)
    {
        // Unsafe optimized swap - eliminates StelemRef_Helper overhead
        ref T itemsRef = ref MemoryMarshal.GetArrayDataReference(_items);
        ref T itemI = ref Unsafe.Add(ref itemsRef, i);
        ref T itemJ = ref Unsafe.Add(ref itemsRef, j);

        T temp = itemJ;
        itemJ = itemI;
        itemI = temp;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return _items[i];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasPv(short pv)
    {
        for (byte i = Zero; i < Count; i++)
        {
            if (_items[i].Key == pv) return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => $"Count={Count}";
}