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
        //internal int Ply;

        internal MoveBase[] Moves;
        internal MoveBase BestMove;
        //internal MoveBase Pv;

        //internal SearchContext Previous;
        //internal SearchContext Next;

        public SearchContext()
        {
            Value = int.MinValue;
        }
    }
}
