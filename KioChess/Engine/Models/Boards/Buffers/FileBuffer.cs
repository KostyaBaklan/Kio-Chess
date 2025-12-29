using System.Runtime.CompilerServices;

namespace Engine.Models.Boards.Buffers
{
    [InlineArray(8)]
    public struct FileBuffer<T> where T : struct
    {
        public T Cells;
    }
}
