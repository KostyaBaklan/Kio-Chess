using System.Runtime.CompilerServices;
using Engine.Interfaces.Config;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves.Lists;


public class PromotionList : MoveBaseList<PromotionMove>
{
    public PromotionList() : base(ContainerLocator.Current.Resolve<IConfigurationProvider>().GeneralConfiguration.MaxMoveCount)
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