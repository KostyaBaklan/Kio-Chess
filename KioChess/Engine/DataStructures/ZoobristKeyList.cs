using Engine.Models.Transposition;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.DataStructures;

/// <summary>
/// Optimized key list for transposition table depth tracking.
/// Uses struct-based nodes with pre-allocated pool to eliminate heap allocations.
/// </summary>
public class ZoobristKeyList
{
    // Pre-allocated node pool to avoid allocations
    private NodeStruct[] _nodePool;
    private readonly Stack<int> _freeIndices;
    private int _rootIndex;
    private int _poolSize;

    /// <summary>
    /// Struct-based node for better memory layout and cache performance.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    private struct NodeStruct
    {
        public ulong Value;
        public int NextIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NodeStruct(ulong value, int nextIndex)
        {
            Value = value;
            NextIndex = nextIndex;
        }
    }

    public ZoobristKeyList()
    {
        _poolSize = 1 << 10;
        _nodePool = new NodeStruct[_poolSize];
        _freeIndices = new Stack<int>(_poolSize);

        // Initialize free indices
        for (int i = _poolSize - 1; i >= 0; i--)
        {
            _freeIndices.Push(i);
        }

        _rootIndex = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ulong item)
    {
        int nodeIndex = GetFreeNodeIndex();
        _nodePool[nodeIndex] = new NodeStruct(item, _rootIndex);
        _rootIndex = nodeIndex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetFreeNodeIndex()
    {
        if (_freeIndices.Count > 0)
        {
            return _freeIndices.Pop();
        }

        // Pool exhausted - expand it
        ExpandPool();
        return _freeIndices.Pop();
    }

    private void ExpandPool()
    {
        int oldSize = _poolSize;
        _poolSize *= 2;

        var newPool = new NodeStruct[_poolSize];
        Array.Copy(_nodePool, newPool, oldSize);
        _nodePool = newPool;

        // Add new indices to free list
        for (int i = _poolSize - 1; i >= oldSize; i--)
        {
            _freeIndices.Push(i);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void GetAndClear(Dictionary<ulong, TranspositionEntry> table)
    {
        int currentIndex = _rootIndex;

        while (currentIndex != -1)
        {
            ref var current = ref _nodePool[currentIndex];
            int nextIndex = current.NextIndex;

            // Remove from table
            table.Remove(current.Value);

            // Return node to pool
            _freeIndices.Push(currentIndex);

            currentIndex = nextIndex;
        }

        _rootIndex = -1;
    }
}