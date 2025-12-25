using System.Runtime.CompilerServices;

namespace Engine.Models.Boards.Buffers
{
    [InlineArray(8)]
    public struct RankBuffer<T> where T : struct
    {
        public T Cells;
    }
}
