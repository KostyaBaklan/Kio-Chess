﻿using Engine.Sorting.Comparers;
using Engine.Sorting.Sorters;

namespace Engine.Interfaces
{
    public interface IMoveSorterProvider
    {
        MoveSorter GetAttack(IPosition position, IMoveComparer comparer);
        MoveSorter GetBasic(IPosition position, IMoveComparer comparer);
        MoveSorter GetInitial(IPosition position, IMoveComparer comparer);
        MoveSorter GetAdvanced(IPosition position, IMoveComparer comparer);
    }
}