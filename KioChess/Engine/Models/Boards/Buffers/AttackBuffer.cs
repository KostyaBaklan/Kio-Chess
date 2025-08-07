using System.Runtime.CompilerServices;

namespace Engine.Models.Boards.Buffers
{
    [InlineArray(32)]
    public struct AttackBuffer
    {
        public byte Pieces;
    }
}
