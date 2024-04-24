using System.Runtime.CompilerServices;

namespace DataAccess.Models;

public struct BookMove:IComparable<BookMove>
{
    public short Id;
    public short Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(BookMove other) => other.Value.CompareTo(Value);

    public override string ToString() => $"[{Id},{Value}]";
}
