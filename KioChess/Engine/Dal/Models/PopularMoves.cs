using DataAccess.Models;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Dal.Models;

public class Popular: PopularMoves
{
    private BookMove[] _move;
    private static bool[] _moveIDs;
    private static int[] _bookValues;

    public Popular(params BookMove[] moves)
    {
        _move = moves;
        IsEmpty = _move.Length == 0;
    }

    public static void Initialize(int depth)
    {
        _moveIDs = new bool[depth];
        _bookValues = new int[depth];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsPopular(MoveBase move)
    {
        if (!_moveIDs[move.Key])
            return false;

        move.BookValue = _bookValues[move.Key];
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Reset()
    {
        for (int i = 0; i < _move.Length; i++)
        {
            _moveIDs[_move[i].Id] = false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SetMoves()
    {
        for (int i = 0; i < _move.Length; i++)
        {
            BookMove bookMove = _move[i];

            _moveIDs[bookMove.Id] = true;
            _bookValues[bookMove.Id] = bookMove.Value;
        }
    }
}
public class PopularMoves 
{

    public static PopularMoves Default = new PopularMoves();

    public PopularMoves()
    {
        IsEmpty = true;
    }

    public bool IsEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool IsPopular(MoveBase move) => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void Reset()
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void SetMoves()
    {
    }
}
