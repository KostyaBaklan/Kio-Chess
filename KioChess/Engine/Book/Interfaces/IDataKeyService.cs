
using Engine.DataStructures;

namespace Engine.Book.Interfaces
{
    public interface IDataKeyService
    {
        byte[] GetByteKey(ref MoveKeyList span);
        string GetKey(ref MoveKeyList span);
    }
}
