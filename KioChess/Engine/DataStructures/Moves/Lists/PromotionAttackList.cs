using System.Runtime.CompilerServices;
using Engine.Interfaces.Config;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves.Lists;

public class PromotionAttackList : MoveBaseList<PromotionAttack>
{
    public PromotionAttackList() : base(ContainerLocator.Current.Resolve<IConfigurationProvider>().GeneralConfiguration.MaxMoveCount)
    {
    }

    public PromotionAttackList(int capacity) : base(capacity)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(PromotionAttackList moves)
    {
        for (byte i = Zero; i < moves.Count; i++)
        {
            Add(moves._items[i]);
        }
    }
}