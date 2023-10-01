using Engine.Book.Models;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Moves;

namespace Engine.Book.Interfaces
{
    public interface IBookService
    {
        void Add(string key, IPopularMoves bookMoves);

        IPopularMoves GetBook(ref MoveKeyList history);
        List<MoveBase> GetOpeningMoves(IMoveProvider moveProvider);
        void SetOpening(List<BookMove> open);
    }
}
