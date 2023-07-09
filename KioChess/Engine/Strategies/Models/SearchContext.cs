using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models
{

    public class SearchContext
    {
        internal SearchResultType SearchResultType;


        internal short Value;
        internal int Ply;

        internal MoveList Moves;
        internal MoveBase BestMove;
        internal short SearchValue;

        public SearchContext()
        {
            Value = short.MinValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Value = SearchValue;
        }
    }
}
