using DataAccess.Models;

namespace Engine.Sorting.Comparers
{
    internal class WhiteBookValueComparer : IComparer<BookValue>
    {
        public int Compare(BookValue x, BookValue y)
        {
            var result = x.GetWhite().CompareTo(y.GetWhite());
            if (result != 0) return result;

            return x.GetTotal().CompareTo(y.GetTotal());
        }
    }
    internal class BlackBookValueComparer : IComparer<BookValue>
    {
        public int Compare(BookValue x, BookValue y)
        {
            var result = x.GetBlack().CompareTo(y.GetBlack());
            if (result != 0) return result;

            return x.GetTotal().CompareTo(y.GetTotal());
        }
    }
}
