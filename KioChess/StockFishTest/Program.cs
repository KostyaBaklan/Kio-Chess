using Engine.Dal.Interfaces;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Services;
using Newtonsoft.Json;
using StockFishCore.Models;
using Tools.Common;

internal class Program
{
    private static void Main(string[] args)
    {
        Boot.SetUp();

        //StockFishDbService stockFishDbService = new StockFishDbService();

        //try
        //{
        //    stockFishDbService.Connect();

        //    stockFishDbService.GenerateReports();
        //}
        //finally
        //{
        //    stockFishDbService.Disconnect();
        //}

        ProcessGameLog();

        Console.WriteLine("Hello, World!");

        Console.ReadLine();
    }

    private static void ProcessGameLog()
    {
        var gameDbservice = Boot.GetService<IGameDbService>();

        try
        {
            gameDbservice.Connect();

            gameDbservice.LoadAsync();

            var text = File.ReadAllText(Path.Combine("Log", "2024_07_16_02_09_37_6073.json"));
            StockFishLog log = JsonConvert.DeserializeObject<StockFishLog>(text);

            Position position = new Position();

            var moveProvider = Boot.GetService<MoveProvider>();
            var moveHistory = Boot.GetService<MoveHistoryService>();

            position.MakeFirst(moveProvider.Get(log.Opening[0]));

            for (int i = 1; i < log.Opening.Length; i++)
            {
                position.Make(moveProvider.Get(log.Opening[i]));
            }

            var strategyFactory = Boot.GetService<IStrategyFactory>();
            var strategy = strategyFactory.GetStrategy(log.Depth, position, log.Strategy);

            bool strategyMove = (log.Color == "w" && position.GetTurn() == Engine.Models.Enums.Turn.Black) ||
                (log.Color == "b" && position.GetTurn() == Engine.Models.Enums.Turn.White);

            gameDbservice.WaitToData();

            foreach (var move in log.History.Skip(log.Opening.Length).Select(moveProvider.Get))
            {
                if (strategyMove)
                {
                    var result = strategy.GetResult();
                    //if (result.Move != move)
                    //{
                    //    throw new ApplicationException("Pizdets");
                    //}

                }
                strategyMove = !strategyMove;
                position.Make(move);
                Console.WriteLine($"{moveHistory.GetPly()} - {move}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToFormattedString());
            throw;
        }
        finally
        {
            gameDbservice.Disconnect();
        }
    }
}