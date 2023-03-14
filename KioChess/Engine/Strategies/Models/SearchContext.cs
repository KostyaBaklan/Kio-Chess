using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models
{
    public class SearchContext
    {
        internal bool IsEndGame;
        internal bool IsFutility;
        internal bool IsReverseFutility;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Value = int.MinValue;
        }
    }
}
