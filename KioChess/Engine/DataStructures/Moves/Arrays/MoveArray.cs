using Engine.Models.Boards.Buffers;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Arrays
{
    public class MoveArray
    {
        private readonly CellBuffer<byte> _indicies;
        private readonly MoveBase[] _moves;

        public MoveArray(List<MoveBase> moves)
        {
            _indicies = new CellBuffer<byte>();
            _moves = new MoveBase[moves.Count];

            for (byte i = 0; i < moves.Count; i++)
            {
                _indicies[moves[i].To] = i;
                _moves[i] = moves[i];
            }
        }



        public MoveBase this[byte to]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _moves[_indicies[to]];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveBase[] GetAll() => _moves;
    }
}
