using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public abstract class SimpleAttack : Attack
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegalAttack() => true;
}

public class WhiteSimpleAttack : SimpleAttack
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsWhiteOpposite(To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override bool IsQueenCaptured() => Captured == Enums.Pieces.BlackQueen;
}
public class BlackSimpleAttack : SimpleAttack
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsLegal() => Board.IsBlackOpposite(To);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override bool IsQueenCaptured() => Captured == Enums.Pieces.WhiteQueen;
}