using Engine.DataStructures.Hash;

namespace Engine.Interfaces
{
    public interface ITranspositionTableService
    {
        TranspositionTable Create(short depth);
    }
}
