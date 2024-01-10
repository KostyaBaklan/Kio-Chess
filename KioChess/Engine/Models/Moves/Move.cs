using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public class Move : MoveBase
{
    #region Overrides of MoveBase

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsEmpty(EmptyBoard);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Make() => Board.Move(Piece, From, To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void UnMake() => Board.Move(Piece, To, From);

    #endregion
}