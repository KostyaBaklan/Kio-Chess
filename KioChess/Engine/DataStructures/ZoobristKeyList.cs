using Engine.Models.Transposition;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures;


public class ZoobristKeyList
{
    private Node<ulong> _root;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ulong item)
    {
        _root = new Node<ulong>(item,_root);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void GetAndClear(Dictionary<ulong, TranspositionEntry> table)
    {
        var current = _root;
        while (current != null)
        {
            table.Remove(current.Value);
            var temp = current;
            current = current.Next;
            temp.Next = null;
        }
        _root = null;
    }
}