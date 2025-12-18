using System.Runtime.CompilerServices;

namespace Engine.Models.Boards.Buffers
{
    [InlineArray(1024)]
    public struct GameBuffer<T> where T : struct
    {
        public T Items;
    }
}
