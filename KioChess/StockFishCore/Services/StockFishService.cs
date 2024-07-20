using CoreWCF;
using Newtonsoft.Json;
using StockFishCore.Data;

namespace StockFishCore.Services
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class StockFishService : IStockFishService
    {
        private readonly object _sync = new object();
        private readonly ResultContext _db;
        private readonly int _runTimeID;

        public StockFishService()
        {
            Console.WriteLine("Please enter branch:");
            var _branch = Console.ReadLine();
            Console.WriteLine("Please enter description:");
            var _description = Console.ReadLine();
            var now = DateTime.Now; 
            var _runTime = new DateTime(now.Year,now.Month, now.Day, now.Hour,now.Minute, now.Second);
            //Debugger.Launch();
            _db = new ResultContext();

            RunTimeInformation rti = new RunTimeInformation
            {
                Branch = _branch,
                Description = _description,
                RunTime = _runTime
            };
            _db.RunTimeInformation.Add(rti);
            _db.SaveChanges();

            _runTimeID = rti.Id;
        }

        public void ProcessResult(string json)
        {
            var stockFishResult = JsonConvert.DeserializeObject<StockFishResult>(json);
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
                Duration = stockFishResult.Duration,
                MoveTime = stockFishResult.MoveTime,
                RunTimeId = _runTimeID
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