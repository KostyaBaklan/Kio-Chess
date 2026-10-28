using Engine.DataStructures;
using Engine.Models.Boards;

namespace Engine.Interfaces;

public interface ITranspositionTableService
{
    TranspositionTable Create(int depth, Board board);
    TranspositionTable CreateNull(int depth, Board board);
}