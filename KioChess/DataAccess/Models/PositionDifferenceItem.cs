using System.Runtime.CompilerServices;

namespace DataAccess.Models;

public struct PositionDifferenceItem : IComparable<PositionDifferenceItem>
{
    public short Id;
    public int Total;
    public short Difference;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(PositionDifferenceItem other) => other.Total.CompareTo(Total);

    public override string ToString() => $"[{Id},{Total},{Difference}]";
}
