using System.Runtime.CompilerServices;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves.Lists;


public class PromotionList : MoveBaseList<PromotionMove>
{
    public PromotionList() : base(128)
    {
    }

    public PromotionList(int capacity) : base(capacity)
    {
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(PromotionList moves)
    {
        for (byte i = Zero; i < moves.Count; i++)
        {
            Add(moves._items[i]);
        }
    }
}