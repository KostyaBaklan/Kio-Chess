using Engine.Book.Models;
using Engine.DataStructures;

namespace Engine.Book.Interfaces
{
    public interface IBookService
    {
        void Add(string history, HistoryValue historyValue);

        BookMoves GetBlackBookValues(ref MoveKeyList history);
        BookMoves GetWhiteBookValues(ref MoveKeyList history);
        Dictionary<string, HistoryValue> GetData();
        Dictionary<short, int> GetBookValues();
    }
}
