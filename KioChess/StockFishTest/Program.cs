using Engine.Dal.Interfaces;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Services;
using Newtonsoft.Json;
using StockFishCore.Data;
using StockFishCore.Models;
using StockFishCore.Services;
using System.Collections.Generic;
using Tools.Common;

internal class Program
{
    private static void Main(string[] args)
    {
        Boot.SetUp();

        StockFishDbService stockFishDbService = new StockFishDbService();

        try
        {
            stockFishDbService.Connect();

            List<ResultEntity> results462 = stockFishDbService.GetResults(462).ToList();

            List<ResultEntity> results472 = stockFishDbService.GetResults(490).ToList();

            List<ResultEntity> notRuntests = FindNotRunTest(results462, results472);

            foreach (ResultEntity result in notRuntests)
            {
                var sequence = string.Join('-',result.Sequence.Split('-').Take(2));
                Console.WriteLine($"{result.Depth} {result.StockFishDepth} {result.Strategy} {result.Color} {result.Elo} {sequence} {result.RunTimeId}");
            }

            //stockFishDbService.GenerateReports();
        }
        finally
        {
            stockFishDbService.Disconnect();
        }

        //ProcessGameLog();

        Console.WriteLine("Hello, World!");

        Console.ReadLine();
    }

    private static List<ResultEntity> FindNotRunTest(List<ResultEntity> full, List<ResultEntity> stuck)
    {
        List<ResultEntity> notRun = new List<ResultEntity>();
        foreach (var item in full)
        {
            var candidates = stuck.Where(s => s.Depth == item.Depth &&
                        s.Color == item.Color &&
                        s.Opening == item.Opening &&
                        s.Strategy == item.Strategy)
                .ToList();

            if (candidates.Count == 0)
            {                 
                notRun.Add(item);
            }
            else if (candidates.Count > 1)
            {
                throw new ApplicationException("Pizdets");
            }
        }

        return notRun;
    }

    private static void ProcessGameLog()
    {
        var gameDbservice = Boot.GetService<IGameDbService>();

        try
        {
            gameDbservice.Connect();

            gameDbservice.LoadAsync();

            var text = File.ReadAllText(Path.Combine("Log", "2024_07_17_10_11_28_8046.json"));
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