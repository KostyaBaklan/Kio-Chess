using Engine.DataStructures.Moves.Lists;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections
{
    public abstract class MoveCollectionBase //: IMoveCollection
    {
        //protected List<MoveBase> Moves;
        protected readonly IMoveComparer Comparer;

        protected MoveCollectionBase(IMoveComparer comparer)
        {
            Comparer = comparer;
        }

        public abstract MoveList Build();
    }
}