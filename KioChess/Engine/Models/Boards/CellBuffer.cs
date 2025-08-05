using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    [InlineArray(64)]
    public struct CellBuffer<T> where T : struct
    {
        public T Pieces;
    }
}
