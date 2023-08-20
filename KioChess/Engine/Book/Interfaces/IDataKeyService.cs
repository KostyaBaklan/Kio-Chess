using Engine.Models.Moves;

namespace Engine.Book.Interfaces
{
    public interface IDataKeyService
    {
        void Add(short key);
        void Delete();
        string Get();
        string Get(IEnumerable<MoveBase> history);

        void Reset();
    }
}
