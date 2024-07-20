using System.Runtime.CompilerServices;

namespace DataAccess.Models;

public struct BookValue
{
    public int White;
    public int Draw;
    public int Black;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetWhite() => White - Black;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetBlack() => Black - White;

    public override string ToString() => $"W={White} D={Draw} B={Black}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetTotal() => White + Draw + Black;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetWhitePercentage(int total) => GetPercentage(White, total);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetDrawPercentage(int total) => GetPercentage(Draw, total);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetPercentageDifference(int total, bool isWhiteToMove)
    {
        var pd = 100.0*(GetWhitePercentage(total) - GetBlackPercentage(total));
        return (short)(isWhiteToMove ? pd : -pd);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetBlackPercentage(int total) => GetPercentage(Black, total);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double GetPercentage(int value, int total)
    {
        if (total == 0) return 0;

        return Math.Round(100.0 * value / total, 2);
    }

    public BookValue Merge(BookValue value) => new BookValue
    {
        White = White + value.White,
        Draw = Draw + value.Draw,
        Black = Black + value.Black
    };
}
