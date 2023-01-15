using Engine.DataStructures.Moves;
using Engine.Models.Moves;

namespace Engine.Strategies.Models
{
    public class SearchContext
    {
        internal bool IsEndGame;
        internal bool IsFutility;
        //internal bool ShouldUpdate;
        //internal bool IsInTable;

        internal int Value;
        internal int Ply;

        internal MoveList Moves;
        internal MoveBase BestMove;
        //internal MoveBase Pv;

        //internal SearchContext Previous;
        //internal SearchContext Next;

        public SearchContext()
        {
            Value = int.MinValue;
        }

        public void Clear()
        {
            IsEndGame = false;
            IsFutility = false;
            Value = int.MinValue;
            BestMove = null;
        }
    }
}
