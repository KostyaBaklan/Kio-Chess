using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Collections.Extended;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters.Extended
{
    public abstract class ExtendedSorter : MoveSorter
    {
        protected ExtendedMoveCollection ExtendedMoveCollection;

        protected ExtendedSorter(IPosition position, IMoveComparer comparer) : base(position, comparer)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves)
        {
           OrderAttacks(ExtendedMoveCollection, attacks);

            ProcessMoves(moves);

            return ExtendedMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override MoveBase[] OrderInternal(AttackList attacks, MoveList moves,
            MoveBase pvNode)
        {
            if (pvNode is AttackBase attack)
            {
                OrderAttacks(ExtendedMoveCollection, attacks, attack);

                ProcessMoves(moves);
            }
            else
            {
                OrderAttacks(ExtendedMoveCollection, attacks);

                ProcessMoves(moves, pvNode);
            }

            return ExtendedMoveCollection.Build();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void ProcessMoves(MoveList moves);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void ProcessMoves(MoveList moves, MoveBase pvNode);
    }
}