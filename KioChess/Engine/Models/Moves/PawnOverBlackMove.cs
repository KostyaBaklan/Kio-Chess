using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public class PawnOverBlackMove : PawnOverMove
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make()
    {
        IsEnPassant = Board.IsWhiteOver(OpponentPawns);
        Board.MoveBlack(Piece, From, To);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake()
    {
        IsEnPassant = false;
        Board.MoveBlack(Piece, To, From);
    }
}