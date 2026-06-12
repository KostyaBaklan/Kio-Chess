using Newtonsoft.Json;
using StockFishCore.Data;
using System.Collections.Concurrent;

namespace StockFishCore.Services
{
    public class StockFishService : IStockFishService
    {
        private readonly ConcurrentBag<ResultEntity> _results = new ConcurrentBag<ResultEntity>();

        public StockFishService()
        {
            //Debugger.Launch();
        }

        public void ProcessResult(string json)
        {
            //Debugger.Launch();
            var stockFishResult = JsonConvert.DeserializeObject<StockFishResult>(json);

            ResultEntity resultEntity = new ResultEntity
            {
                Depth = stockFishResult.StockFishResultItem.Depth,
                StockFishDepth = stockFishResult.StockFishResultItem.StockFishDepth,
                Elo = stockFishResult.StockFishResultItem.Elo,
                Strategy = stockFishResult.StockFishResultItem.Strategy.ToString(),
                Color = stockFishResult.Color,
                Result = stockFishResult.Result.ToString(),
                OutputType = stockFishResult.OutputType.ToString(),
                KioValue = stockFishResult.GetKioValue(),
                SfValue = stockFishResult.GetStockFishValue(),
                Opening = stockFishResult.Opening,
                Sequence = stockFishResult.Sequence,
                MoveTime = stockFishResult.MoveTime,
                RunTimeId = stockFishResult.RunTimeId
            };

            _results.Add(resultEntity);
        }

        public void Save()
        {
            Console.WriteLine($"Saving {_results.Count} items");

            using var _db = new ResultContext();

            _db.ResultEntities.AddRange(_results);
            _db.SaveChanges();

            _results.Clear();
        }
    }
}