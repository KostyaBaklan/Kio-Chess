using System.Runtime.CompilerServices;

namespace Engine.Book.Models
{
    public struct BookValue
    {
        public int White;
        public int Draw;
        public int Black;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetWhite() => White - Black;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBlack() => Black - White;
    }
}
