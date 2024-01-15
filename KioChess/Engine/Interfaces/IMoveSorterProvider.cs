using Engine.Models.Boards;
using Engine.Sorting.Sorters;

namespace Engine.Interfaces;

public interface IMoveSorterProvider
{
    MoveSorterBase GetAttack(Position position);
    MoveSorterBase GetSimple(Position position);
    MoveSorterBase GetComplex(Position position);
    MoveSorterBase GetComplexQuiet(Position position);
}