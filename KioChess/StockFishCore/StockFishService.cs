using CoreWCF;
using System.Collections.Concurrent;

namespace StockFishCore
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class StockFishService : IStockFishService
    {
        private ConcurrentQueue<StockFishResult> _queue;

        public StockFishService()
        {
            _queue = new ConcurrentQueue<StockFishResult>();
        }

        public void ProcessResult(StockFishResult stockFishResult)
        {
            _queue.Enqueue(stockFishResult);
        }

        public void Save()
        {
            using (var writter = new StreamWriter("StockFishResults.csv"))
            {
                IEnumerable<string> headers = StockFishResult.GetHeaders();

                writter.WriteLine(string.Join(",", headers));

                var groups = _queue.GroupBy(r => r.StockFishResultItem);

                foreach (var group in groups)
                {
                    var games = group.ToList();

                    List<string> values = new List<string>()
                    {
                        group.Key.Depth.ToString(),group.Key.StockFishDepth.ToString(),group.Key.Skill.ToString()
                    };

                    double kio = 0.0;
                    double st = 0.0;
                    foreach (var game in games)
                    {
                        kio += game.GetKioValue();
                        st += game.GetStockFishValue();
                    }

                    values.Add($"{Math.Round(kio, 1)}x{Math.Round(st, 1)}");

                    writter.WriteLine(string.Join(",", values));
                } 
            }
        }
    }
}