using System.Runtime.CompilerServices;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves.Lists;

public class PromotionAttackList : MoveBaseList<PromotionAttack>
{
    public PromotionAttackList() : base(128)
    {
    }

    public PromotionAttackList(int capacity) : base(capacity)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(PromotionAttackList moves)
    {
        Array.Copy(moves._items, 0, _items, Count, moves.Count);
        Count += moves.Count;
    }
}