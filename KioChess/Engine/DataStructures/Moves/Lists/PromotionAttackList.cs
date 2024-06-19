using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.Interfaces.Config;
using Engine.Models.Moves;

namespace Engine.DataStructures.Moves.Lists;

public class PromotionAttackList : MoveBaseList<PromotionAttack>
{
    public PromotionAttackList() : base(ServiceLocator.Current.GetInstance<IConfigurationProvider>().GeneralConfiguration.MaxMoveCount)
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