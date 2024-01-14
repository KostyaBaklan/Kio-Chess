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
        for (int i = 0; i < moves.Count; i++)
        {
            Add(moves._items[i]);
        }
    }
}