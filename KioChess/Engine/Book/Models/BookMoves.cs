using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Book.Models
{
    public class BookMoves
    {
        private readonly BookMove[] _sugested;
        private readonly BookMove[] _nonSugested;

        public BookMoves()
        {
            _sugested= new BookMove[0];
            _nonSugested= new BookMove[0];
        }

        public BookMoves(BookMove[] suggestedBookMoves, BookMove[] nonSuggestedBookMoves)
        {
            _sugested = suggestedBookMoves;
            _nonSugested= nonSuggestedBookMoves;
        }

        internal BookMove[] GetSuggested()
        {
            return _sugested;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsNonSuggested(MoveBase move)
        {
            for (int i = 0; i < _nonSugested.Length; i++)
            {
                if (_nonSugested[i].Id != move.Key)
                    continue;

                move.BookValue = _nonSugested[i].Value;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsSuggested(MoveBase move)
        {
            for (int i = 0; i < _sugested.Length; i++)
            {
                if (_sugested[i].Id != move.Key)
                    continue;

                move.BookValue = _sugested[i].Value;
                return true;
            }

            return false;
        }
    }
}
