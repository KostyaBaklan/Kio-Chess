using Newtonsoft.Json;
using Tools.Common;

namespace Common
{
    public class TestModel
    {
        public TestModel()
        {
            Moves = new List<MoveModel>();
        }

        public string Strategy { get; set; }
        public string Game { get; set; }
        public int Depth { get; set; }
        public TimeSpan Total { get; set; }
        public TimeSpan Min { get; set; }
        public TimeSpan Max { get; set; }
        public TimeSpan Average { get; set; }
        public TimeSpan Std { get; set; }

        [JsonIgnore]
        public int Material
        {
            get { return Moves.Select(m => m.Material).Last(); }
        }

        [JsonIgnore]
        public int StaticValue
        {
            get { return Moves.Select(m => m.StaticValue).Last(); }
        }

        [JsonIgnore]
        public long Table
        {
            get { return Moves.Max(m => m.Table); }
        }

        [JsonIgnore]
        public long Evaluation
        {
            get { return Moves.Max(m => m.Evaluation); }
        }

        [JsonIgnore]
        public long Memory
        {
            get { return Moves.Max(m => m.Memory); }
        }

        public List<MoveModel> Moves { get; set; }
        public string Position { get; set; }

        public void Calculate()
        {
            var movesCount = Moves.Count;
            List<TimeSpan> times = new List<TimeSpan>(movesCount);
            TimeSpan total = new TimeSpan();
            foreach (var moveModel in Moves)
            {
                total += moveModel.Time;
                times.Add(moveModel.Time);
            }

            var doubles = times.Select(t => t.TotalMilliseconds).ToArray();
            Min = TimeSpan.FromMilliseconds(ArrayStatistics.Minimum(doubles));
            Max = TimeSpan.FromMilliseconds(ArrayStatistics.Maximum(doubles));
            var (m, s) = ArrayStatistics.MeanStandardDeviation(doubles);
            Average = TimeSpan.FromMilliseconds(m);
            Std = TimeSpan.FromMilliseconds(s);

            Total = total;
        }
    }
}