using Engine.Models.Moves;

namespace Engine.DataStructures.Moves;

public class AttackWrapper : IComparable<AttackWrapper>
{
    public AttackWrapper(int value, AttackBase move)
    {
        Value = value;
        Move = move;
    }

    public int Value { get; }
    public AttackBase Move { get; }

    #region Relational members

    public int CompareTo(AttackWrapper other)
    {
        return Value.CompareTo(other.Value);
    }

    public static bool operator <(AttackWrapper left, AttackWrapper right)
    {
        return Comparer<AttackWrapper>.Default.Compare(left, right) < 0;
    }

    public static bool operator >(AttackWrapper left, AttackWrapper right)
    {
        return Comparer<AttackWrapper>.Default.Compare(left, right) > 0;
    }

    public static bool operator <=(AttackWrapper left, AttackWrapper right)
    {
        return Comparer<AttackWrapper>.Default.Compare(left, right) <= 0;
    }

    public static bool operator >=(AttackWrapper left, AttackWrapper right)
    {
        return Comparer<AttackWrapper>.Default.Compare(left, right) >= 0;
    }

    #endregion

    #region Overrides of Object

    public override string ToString()
    {
        return $"{Move}({Value})";
    }

    #endregion
}