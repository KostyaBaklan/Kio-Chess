using Engine.Models.Enums;
using ProtoBuf;

namespace StockFishCore
{
    [ProtoContract]
    public class StockFishResultItem : IEquatable<StockFishResultItem>
    {
        [ProtoMember(1)]
        public int Skill { get; set; }

        [ProtoMember(2)]
        public short Depth { get; set; }

        [ProtoMember(3)]
        public short StockFishDepth { get; set; }

        [ProtoMember(4)]
        public StrategyType Strategy { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as StockFishResultItem);
        }

        public bool Equals(StockFishResultItem other)
        {
            return other is not null &&
                   Skill == other.Skill &&
                   Depth == other.Depth &&
                   StockFishDepth == other.StockFishDepth &&
                   Strategy == other.Strategy;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Skill, Depth, StockFishDepth, Strategy);
        }

        public static bool operator ==(StockFishResultItem left, StockFishResultItem right)
        {
            return EqualityComparer<StockFishResultItem>.Default.Equals(left, right);
        }

        public static bool operator !=(StockFishResultItem left, StockFishResultItem right)
        {
            return !(left == right);
        }
    }
}