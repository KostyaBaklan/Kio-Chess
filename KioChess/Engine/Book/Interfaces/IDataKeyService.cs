
using Engine.DataStructures;

namespace Engine.Book.Interfaces
{
    public interface IDataKeyService
    {
        string Get(ref MoveKeyList span);
    }
}
