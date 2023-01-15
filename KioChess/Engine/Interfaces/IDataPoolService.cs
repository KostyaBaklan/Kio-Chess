using Engine.DataStructures.Moves;
using Engine.Strategies.Models;

namespace Engine.Interfaces
{
    public interface IDataPoolService
    {
        SearchContext GetCurrentContext();
        MoveList GetCurrentMoveList();
    }
}
