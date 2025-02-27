using System.Runtime.CompilerServices;

namespace DataAccess.Models;
public struct PositionItem : IComparable<PositionItem>
{
    public short Id;
    public int Total;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(PositionItem other) => other.Total.CompareTo(Total);

    public override string ToString() => $"[{Id},{Total}]";
}
