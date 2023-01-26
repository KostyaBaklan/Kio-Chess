using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;

namespace Engine.Sorting.Sorters
{
    public interface IMoveSorter
    {
        MoveList Order(AttackList attacks, MoveList moves, MoveBase pvNode);
        MoveList Order(AttackList attacks);
        void Add(short move);
    }
}