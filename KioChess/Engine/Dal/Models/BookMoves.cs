using DataAccess.Models;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Dal.Models;


public class BookMoves
{
    private static readonly BookMove _default = new BookMove { Id = -1, Value = 0 };

    private BookMove _total = _default;
    private BookMove _max = _default;
    private BookMove _min = _default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetTotal(BookMove move)
    {
        _total = move;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMax(BookMove move)
    {
        _max = move;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMin(BookMove move)
    {
        _min = move;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsTotal(MoveBase move)
    {
        if (move.Key != _total.Id)
            return false;

        move.BookValue = _total.Value;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMax(MoveBase move)
    {
        if (move.Key != _max.Id)
            return false;

        move.BookValue = _max.Value;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMin(MoveBase move)
    {
        if (move.Key != _min.Id)
            return false;

        move.BookValue = _min.Value;
        return true;
    }
}
