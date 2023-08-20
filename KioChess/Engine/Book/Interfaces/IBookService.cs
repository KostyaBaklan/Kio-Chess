using Engine.Book.Models;
using Engine.Models.Moves;

namespace Engine.Book.Interfaces
{
    public interface IBookService
    {
        void Add(string history, HistoryValue historyValue);

        Dictionary<short, int> GetBlackBookValues(IEnumerable<MoveBase> history);

        Dictionary<short, int> GetWhiteBookValues(IEnumerable<MoveBase> history);
    }
}
