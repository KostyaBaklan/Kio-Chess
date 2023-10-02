using Engine.Models.Helpers;
using System.Runtime.CompilerServices;

namespace Engine.Models.Moves;

public abstract class AttackBase : MoveBase,IComparable<AttackBase>
{
    public byte Captured;
    public short See;

    protected AttackBase()
    {
        IsAttack = true;
    }

    #region Implementation of IComparable<in AttackBase>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(AttackBase other)
    {
        return other.See.CompareTo(See);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsGreater(AttackBase move)
    {
        return See > move.See;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLess(AttackBase move)
    {
        return Piece < move.Piece;
    }

    #endregion


    public override string ToString()
    {
        return $"[{Piece.AsKeyName()} {From.AsString()} x {To.AsString()}, S={See}, B={BookValue}]";
    }
}