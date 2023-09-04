using Engine.Book.Models;

namespace Engine.Book.Interfaces
{
    public interface IDataAccessService
    {
        void Upsert(string history, short key, GameValue value);
        void UpdateHistory(GameValue value);
        void Clear();
        void Connect();
        void Disconnect();
        void Execute(string sql);
        bool Exists(string history, short key);
        void Export(string file);
        HistoryValue Get(string history);

        Task LoadAsync(IBookService bookService);
        void WaitToData();
        void SaveOpening(string key, short id);
        void SaveOpening(string opening, string variation, string sequence = null);
        HashSet<string> GetOpeningNames();
        string GetOpening(string key);
    }
}
