using Engine.Services;
using Newtonsoft.Json;

namespace Engine.Tools
{
    public class LmrParityPerformance
    {
        private static LmrParity LmrParity;
        private static MoveHistoryService MoveHistory;

        public static void Add(int depth, int index, int count)
        {
            var ply = MoveHistory.GetPly();
            LmrParity.Add(ply,depth,index, count);
        }

        public static void Initialize(short level)
        {
            MoveHistory = ContainerLocator.Current.Resolve<MoveHistoryService>();
            LmrParity = new LmrParity { Depth = level };
        }

        public static void Save()
        {
            var json = JsonConvert.SerializeObject(LmrParity, Formatting.Indented);

            File.WriteAllText($"{nameof(LmrParityPerformance)}_{LmrParity.Depth}.json", json);
        }
    }
}
