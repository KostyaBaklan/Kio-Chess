using Engine.DataStructures.Moves;
using Engine.Models.Moves;

namespace Engine.Sorting.Sorters
{
    public interface IMoveSorter
    {
        //IMove[] Order(IEnumerable<AttackBase> attacks, IEnumerable<MoveBase> moves, MoveBase pvNode);
        //IMove[] Order(IEnumerable<AttackBase> attacks);
        MoveBase[] Order(AttackList attacks, MoveList moves, MoveBase pvNode);
        MoveBase[] Order(AttackList attacks);
        void Add(short move);
    }
}