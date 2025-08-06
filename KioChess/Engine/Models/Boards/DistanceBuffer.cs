using System.Runtime.CompilerServices;

namespace Engine.Models.Boards
{
    [InlineArray(64)]
    public struct DistanceBuffer
    {
        public CellBuffer<byte> Distances;
    }
}
