using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    [InlineArray(12)]
    public struct PieceBuffer<T> where T : struct
    {
        public T Pieces;
    }
}
