using CoreWCF;

namespace StockFishCore
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
                Skill = stockFishResult.StockFishResultItem.Skill,
                Strategy = stockFishResult.StockFishResultItem.Strategy,
                Color = stockFishResult.Color,
                Result = stockFishResult.Result,
                Time = DateTime.Now
            };

            lock(_sync )
            {
                _db.ResultEntities.Add( resultEntity );
                _db.SaveChanges();
            }
        }

        public void Save()
        {
        }
    }
}