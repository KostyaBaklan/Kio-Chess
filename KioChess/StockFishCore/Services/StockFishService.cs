using CoreWCF;
using Newtonsoft.Json;
using StockFishCore.Data;
using System.Diagnostics;

namespace StockFishCore.Services
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class StockFishService : IStockFishService
    {
        private readonly object _sync = new object();
        private readonly ResultContext _db;

        public StockFishService()
        {
            //Debugger.Launch();
            _db = new ResultContext();
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
                Duration = stockFishResult.Duration,
                MoveTime = stockFishResult.MoveTime,
                RunTimeId = stockFishResult.RunTimeId
            };

            lock (_sync)
            {
                _db.ResultEntities.Add(resultEntity);
                _db.SaveChanges();
            }
        }

        public void Save()
        {
        }
    }
}