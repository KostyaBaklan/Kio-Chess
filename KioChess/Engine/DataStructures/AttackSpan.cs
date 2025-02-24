using Engine.DataStructures.Moves.Lists;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly ref struct AttackSpan(AttackList attacks)
{
    private readonly Span<AttackBase> _items = attacks.AsSpan();
    private readonly int _captureCount = Sorting.Sort.SortAttackMinimum[attacks.Count];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Sort()
    {
        for (int i = 0; i < _captureCount; i++)
        {
            int index = i;
            var max = _items[i];
            for (int j = i + 1; j < _items.Length; j++)
            {
                if (!_items[j].IsGreater(max))
                    continue;

                max = _items[j];
                index = j;
            }

            if (index == i) continue;

            _items[index] = _items[i];
            _items[i] = max;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FullSort()
    {
        _items.InsertionSort();
    }
}