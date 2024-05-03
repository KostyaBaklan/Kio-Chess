using ProtoBuf;

namespace StockFishCore
{

    [ProtoContract]
    public class StockFishResult
    {
        [ProtoMember(1)]
        public StockFishResultItem StockFishResultItem { get; set; }

        [ProtoMember(2)]
        public string Color { get; set; }

        [ProtoMember(3)]
        public StockFishGameResultType Result { get; set; }

        public static IEnumerable<string> GetHeaders()
        {
            return new List<string> { "Depth", "ST Depth", "Level", "Result" };
        }

        public double GetKioValue()
        {
            if (Result == StockFishGameResultType.Draw) return 0.5;

            if (Color == "w" && Result == StockFishGameResultType.White || Color == "b" && Result == StockFishGameResultType.Black) return 0;

            return 1;
        }

        public double GetStockFishValue()
        {
            if (Result == StockFishGameResultType.Draw) return 0.5;

            if (Color == "w" && Result == StockFishGameResultType.Black || Color == "b" && Result == StockFishGameResultType.White) return 0;

            return 1;
        }
    }
}