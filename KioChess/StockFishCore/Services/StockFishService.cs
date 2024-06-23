using CoreWCF;
using StockFishCore.Data;

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

        public void ProcessResult(StockFishResult stockFishResult)
        {
            //Debugger.Launch();

            ResultEntity resultEntity = new ResultEntity
            {
                Depth = stockFishResult.StockFishResultItem.Depth,
                StockFishDepth = stockFishResult.StockFishResultItem.StockFishDepth,
                Elo = stockFishResult.StockFishResultItem.Elo,
                Strategy = stockFishResult.StockFishResultItem.Strategy.ToString(),
                Color = stockFishResult.Color,
                Result = stockFishResult.Result.ToString(),
                KioValue = stockFishResult.GetKioValue(),
                SfValue = stockFishResult.GetStockFishValue(),
                Opening = stockFishResult.Opening,
                Sequence = stockFishResult.Sequence,
                Time = DateTime.Now,
                Duration = stockFishResult.Duration
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