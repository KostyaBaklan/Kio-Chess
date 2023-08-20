using Engine.DataStructures.Moves.Lists;
using Engine.Strategies.Models;

namespace Engine.Interfaces
{
    public interface IDataPoolService
    {
        SearchContext GetCurrentContext();
        MoveList GetCurrentMoveList();
        SortContext GetCurrentSortContext();
        void Initialize(IPosition position, Book.Interfaces.IBookService bookService);
    }
}
