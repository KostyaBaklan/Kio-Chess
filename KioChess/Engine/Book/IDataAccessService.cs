using Engine.Models.Moves;

namespace Engine.Book
{
    public interface IDataAccessService
    {
        void AddHistory(IEnumerable<MoveBase> history, GameValue value);
        void Connect();
        void Disconnect();
        bool Exists(string history, short key);
        HistoryValue Get(string history);
    }
}
