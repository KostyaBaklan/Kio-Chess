using Engine.Models.Moves;

namespace Engine.DataStructures.Moves;

public class MoveWrapper :IComparable<MoveWrapper>
{
    public MoveWrapper(int value, MoveBase move)
    {
        Value = value;
        Move = move;
    }

    public int Value { get; }
    public MoveBase Move { get; }

    #region Relational members

    public int CompareTo(MoveWrapper other)
    {
        return Value.CompareTo(other.Value);
    }

    public static bool operator <(MoveWrapper left, MoveWrapper right)
    {
        return Comparer<MoveWrapper>.Default.Compare(left, right) < 0;
    }

    public static bool operator >(MoveWrapper left, MoveWrapper right)
    {
        return Comparer<MoveWrapper>.Default.Compare(left, right) > 0;
    }

    public static bool operator <=(MoveWrapper left, MoveWrapper right)
    {
        return Comparer<MoveWrapper>.Default.Compare(left, right) <= 0;
    }

    public static bool operator >=(MoveWrapper left, MoveWrapper right)
    {
        return Comparer<MoveWrapper>.Default.Compare(left, right) >= 0;
    }

    #endregion

    #region Overrides of Object

    public override string ToString()
    {
        return $"{Move}({Value})";
    }

    #endregion
}