using Engine.DataStructures;

namespace Engine.Dal.Interfaces;

public interface IDataKeyService
{
    byte[] GetByteKey(ref MoveKeyList span);
    string GetKey(ref MoveKeyList span);
    string GetKey(byte[] key);
}
