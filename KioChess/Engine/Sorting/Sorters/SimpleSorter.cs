using Engine.DataStructures.Moves.Collections;
using Engine.Interfaces;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters;

public class SimpleSorter : CommonMoveSorter<SimpleMoveCollection>
{
    public SimpleSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
    {
       
    }

    protected override void InitializeMoveCollection()
    {
       AttackCollection = new SimpleMoveCollection(Comparer);
    }
}
