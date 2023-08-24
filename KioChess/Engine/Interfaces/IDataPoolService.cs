using Engine.Book.Interfaces;
using Engine.DataStructures.Moves.Lists;
using Engine.Strategies.Models;

namespace Engine.Interfaces
{
    public interface IDataPoolService
    {
        SearchContext GetCurrentContext();
        MoveList GetCurrentMoveList();
        SortContext GetCurrentSortContext();
        void Initialize(IPosition position, IBookService bookService, IMoveHistoryService moveHistory);
    }
}
