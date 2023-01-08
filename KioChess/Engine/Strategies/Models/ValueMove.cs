using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Base
{
    public abstract partial class StrategyBase
    {
        protected class ValueMove : IComparable<ValueMove>
        {
            public int Value;
            public MoveBase Move;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CompareTo(ValueMove other)
            {
                return other.Value.CompareTo(Value);
            }

            public override string ToString()
            {
                return $"{Move}={Value}";
            }
        }
    }
}
