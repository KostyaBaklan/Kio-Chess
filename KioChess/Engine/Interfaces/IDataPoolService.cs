using Engine.DataStructures.Moves.Lists;
using Engine.Strategies.Models;
using Engine.Strategies.Models.Contexts;

namespace Engine.Interfaces;

public interface IDataPoolService
{
    SearchContext GetCurrentContext();
    MoveList GetCurrentMoveList();
    SortContext GetCurrentSortContext();
    void Initialize(IPosition position);
}
