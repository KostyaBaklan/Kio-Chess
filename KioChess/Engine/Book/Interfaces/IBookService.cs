using Engine.Book.Models;
using Engine.DataStructures;

namespace Engine.Book.Interfaces
{
    public interface IBookService
    {
        void Add(string key, BookMoves bookMoves);
        BookMoves GetBook(string key);

        BookMoves GetBook(ref MoveKeyList history);
    }
}
