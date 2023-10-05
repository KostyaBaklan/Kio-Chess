using Engine.Models.Moves;
using Engine.Sorting.Sorters;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models.Contexts.Regular;

public abstract class RegularSortContext : SortContext
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Set(MoveSorterBase sorter, MoveBase pv = null)
    {
        MoveSorter = sorter;
        MoveSorter.SetKillers();
        CounterMove = sorter.GetCounterMove();

        if (pv != null)
        {
            HasPv = true;
            Pv = pv.Key;
            IsPvCapture = pv.IsAttack;
        }
        else
        {
            HasPv = false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SetForEvaluation(MoveSorterBase sorter)
    {
        MoveSorter = sorter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsRegularMove(MoveBase move)
    {
        return true;
    }
}
