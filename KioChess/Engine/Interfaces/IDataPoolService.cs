using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Strategies.Models.Contexts;

namespace Engine.Interfaces;

public interface IDataPoolService
{
    SearchContext GetCurrentContext();
    MoveList GetCurrentMoveList();
    SortContext GetCurrentSortContext(); 
    SortContext GetCurrentNullSortContext();
    void Initialize(Position position);
}
