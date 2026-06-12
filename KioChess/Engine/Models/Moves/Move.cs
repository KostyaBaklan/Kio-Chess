using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public sealed class WhiteMove : MoveBase
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsEmpty(EmptyBoard);

    #region Overrides of MoveBase

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make() => Board.MoveWhite(Piece, From, To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake() => Board.MoveWhite(Piece, To, From);

    #endregion
}

public sealed class BlackMove : MoveBase
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsEmpty(EmptyBoard);

    #region Overrides of MoveBase

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make() => Board.MoveBlack(Piece, From, To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake() => Board.MoveBlack(Piece, To, From);

    #endregion
}