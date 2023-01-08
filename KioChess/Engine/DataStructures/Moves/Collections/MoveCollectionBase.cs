using Engine.Models.Moves;
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

        protected int Count;

        //public MoveBase this[int index] => Moves[index];

        public abstract MoveBase[] Build();

        #region Overrides of Object

        public override string ToString()
        {
            return $"Count={Count}";
        }

        #endregion
    }
}