using Engine.Models.Boards.Buffers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Arrays
{
    public class AttackArray
    {
        private readonly CellBuffer<byte> _indicies;
        private readonly AttackBase[] _moves;

        public AttackArray(List<AttackBase> moves)
        {
            _indicies = new CellBuffer<byte>();
            _moves = new AttackBase[moves.Count];

            for (byte i = 0; i < moves.Count; i++)
            {
                _indicies[moves[i].To] = i;
                _moves[i] = moves[i];
            }
        }



        public AttackBase this[byte to]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _moves[_indicies[to]];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AttackBase[] GetAll() => _moves;
    }
}
