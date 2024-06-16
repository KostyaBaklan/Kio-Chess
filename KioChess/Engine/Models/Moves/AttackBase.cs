using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public abstract class AttackBase : MoveBase,IComparable<AttackBase>
{
    public byte Captured;
    public int See;
    public static int[] CapturedValue = new int[] { 100, 325, 325, 500, 975, 10000, 100, 325, 325, 500, 975, 10000 };

    protected AttackBase()
    {
        IsAttack = true;
    }

    #region Implementation of IComparable<in AttackBase>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(AttackBase other) => other.See.CompareTo(See);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsGreater(AttackBase move) => See > move.See;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLess(AttackBase move) => Piece < move.Piece;

    #endregion


    public override string ToString() => $"[{Piece.AsKeyName()} {From.AsString()} x {To.AsString()}, S={See}, B={BookValue}]";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetCapturedValue()
    {
        See = CapturedValue[Board.GetPiece(To)];
    }
}