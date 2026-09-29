using System.Runtime.CompilerServices;

namespace Engine.Models.Boards.Buffers
{
    [InlineArray(25)]
    public struct PstTable
    {
        public CellBuffer<short> Values;
    }
}
