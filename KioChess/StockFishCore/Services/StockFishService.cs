using CoreWCF;
using StockFishCore.Data;

namespace StockFishCore.Services
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class StockFishService : IStockFishService
    {
        private readonly string _branch;
        private readonly string _description;
        private readonly DateTime _runTime;

        private readonly object _sync = new object();
        private readonly ResultContext _db;

        public StockFishService()
        {
            Console.WriteLine("Please enter branch:");
            _branch = Console.ReadLine();
            Console.WriteLine("Please enter description:");
            _description = Console.ReadLine();
            var now = DateTime.Now; 
            _runTime = new DateTime(now.Year,now.Month, now.Day, now.Hour,now.Minute, now.Second);
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
                Duration = stockFishResult.Duration,
                Branch = _branch,
                Description= _description,
                RunTime = _runTime
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