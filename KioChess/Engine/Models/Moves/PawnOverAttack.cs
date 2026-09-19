using Engine.Services;
using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public abstract class PawnOverAttack : AttackBase
{
    protected static MoveHistoryService history = ContainerLocator.Current.Resolve<MoveHistoryService>();
    public MoveBase EnPassant;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => history.IsLast(EnPassant.Key) && EnPassant.IsEnPassant;
}

public sealed class PawnOverWhiteAttack : PawnOverAttack
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

public sealed class PawnOverBlackAttack : PawnOverAttack
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