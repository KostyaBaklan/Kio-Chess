
namespace StockFishCore
{
    public class StockFishMatchItemInfo
    {

        public StockFishMatchItemInfo(string branch, double value, double coeficient)
        {
            Branch = branch;
            Value = value;
            Coeficient = coeficient;
        }
        public string Branch { get;  }

        public double Value { get;  }
        public double Coeficient { get;  }
    }
}