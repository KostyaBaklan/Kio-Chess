using Engine.DataStructures.Moves.Lists;
using Engine.Strategies.Models.Contexts;

namespace Engine.Interfaces;

public interface IDataPoolService
{
    SearchContext GetCurrentContext();
    MoveValueList GetCurrentMoveList();
    SortContext GetCurrentSortContext();
    void Initialize(IPosition position);
}
