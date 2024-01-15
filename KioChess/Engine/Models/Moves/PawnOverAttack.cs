using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.Services;

namespace Engine.Models.Moves;

public abstract class PawnOverAttack : Attack
{
    protected static MoveHistoryService history = ServiceLocator.Current.GetInstance<MoveHistoryService>();
    public MoveBase EnPassant;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => history.IsLast(EnPassant.Key) && EnPassant.IsEnPassant;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegalAttack() => IsLegal();
}

public class PawnOverWhiteAttack : PawnOverAttack
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make()
    {
        Board.RemoveBlack(EnPassant.Piece, EnPassant.To);
        Board.MoveWhite(Piece, From, To);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake()
    {
        Board.MoveWhite(Piece, To, From);
        Board.AddBlack(EnPassant.Piece, EnPassant.To);
    }
}

public class PawnOverBlackAttack : PawnOverAttack
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make()
    {
        Board.RemoveWhite(EnPassant.Piece, EnPassant.To);
        Board.MoveBlack(Piece, From, To);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake()
    {
        Board.MoveBlack(Piece, To, From);
        Board.AddWhite(EnPassant.Piece, EnPassant.To);
    }
}