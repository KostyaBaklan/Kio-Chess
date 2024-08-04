
namespace StockFishCore
{
    public class StockFishDepthEloItem
    {
        public int Elo { get; set; }
        public short Depth { get; set; }
        public override bool Equals(object obj) => Equals(obj as StockFishDepthEloItem);

        public bool Equals(StockFishDepthEloItem other) => other is not null &&
                   Elo == other.Elo &&
                   Depth == other.Depth;

        public override int GetHashCode() => HashCode.Combine(Elo, Depth);

        public static bool operator ==(StockFishDepthEloItem left, StockFishDepthEloItem right)
        {
            return EqualityComparer<StockFishDepthEloItem>.Default.Equals(left, right);
        }

        public static bool operator !=(StockFishDepthEloItem left, StockFishDepthEloItem right)
        {
            return !(left == right);
        }
    }
}