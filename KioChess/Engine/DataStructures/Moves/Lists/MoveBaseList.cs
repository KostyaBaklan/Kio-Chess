﻿using System.Collections;
using System.Runtime.CompilerServices;
using Engine.Interfaces.Config;
using Engine.Models.Moves;

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
    public Span<T> AsSpan() => new Span<T>(_items, 0, Count);

    #region Implementation of IReadOnlyCollection<out IMove>

    public byte Count;

    protected MoveBaseList(int capacity)
    {
        _items = new T[capacity];
    }

    #endregion


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T move) => _items[Count++] = move;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Span<T> moves)
    {
        for (int i = 0; i < moves.Length; i++)
        {
            Add(moves[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T[] moves)
    {
        for (int i = 0; i < moves.Length; i++)
        {
            Add(moves[i]);
        }
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
        var temp = _items[j];
        _items[j] = _items[i];
        _items[i] = temp;
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