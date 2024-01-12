using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public class PawnOverWhiteMove: PawnOverMove
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make()
    {
        IsEnPassant = Board.IsBlackOver(OpponentPawns);
        Board.MoveWhite(Piece, From, To);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake()
    {
        IsEnPassant = false;
        Board.MoveWhite(Piece, To, From);
    }
}