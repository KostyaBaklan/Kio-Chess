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
                Strategy = stockFishResult.StockFishResultItem.Strategy,
                Color = stockFishResult.Color,
                Result = stockFishResult.Result,
                KioValue = stockFishResult.GetKioValue(),
                SfValue = stockFishResult.GetStockFishValue(),
                Sequence = stockFishResult.Sequence,
                Time = DateTime.Now
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