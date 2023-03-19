using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models
{

    public class SearchContext
    {
        internal EndGameType EndGameType;


        internal int Value;
        internal int Ply;

        internal MoveList Moves;
        internal MoveBase BestMove;

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
