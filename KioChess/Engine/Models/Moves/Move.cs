using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public abstract class Move : MoveBase
{
    #region Overrides of MoveBase

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsEmpty(EmptyBoard);

    #endregion
}

public class WhiteMove : Move
{
    #region Overrides of MoveBase

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make() => Board.MoveWhite(Piece, From, To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake() => Board.MoveWhite(Piece, To, From);

    #endregion
}

public class BlackMove : Move
{
    #region Overrides of MoveBase

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make() => Board.MoveBlack(Piece, From, To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake() => Board.MoveBlack(Piece, To, From);

    #endregion
}