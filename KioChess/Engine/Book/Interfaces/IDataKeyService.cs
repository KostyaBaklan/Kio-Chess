
using Engine.DataStructures;

namespace Engine.Book.Interfaces
{
    public interface IDataKeyService
    {
        void Add(short key);
        string Get(ref MoveKeyList span);
        string Get();
        void Reset();
    }
}
