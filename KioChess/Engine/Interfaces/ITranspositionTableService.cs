using Engine.DataStructures;

namespace Engine.Interfaces;

public interface ITranspositionTableService
{
    TranspositionTable Create(int depth);
}
