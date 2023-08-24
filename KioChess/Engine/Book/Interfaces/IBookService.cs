using Engine.Book.Models;
using Engine.DataStructures;

namespace Engine.Book.Interfaces
{
    public interface IBookService
    {
        void Add(string history, HistoryValue historyValue);

        Dictionary<short, int> GetBlackBookValues(ref MoveKeyList history);
        Dictionary<short, int> GetWhiteBookValues(ref MoveKeyList history);
        Dictionary<string, HistoryValue> GetData();
    }
}
