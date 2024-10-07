using Engine.Dal.Interfaces;
using Engine.Interfaces.Config;
using Engine.Services;
using Newtonsoft.Json;
using StockfishApp;
using StockFishCore;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();
        Boot.SetUp();

        var localDbservice = Boot.GetService<ILocalDbService>();

        var gameDbservice = Boot.GetService<IGameDbService>();

        try
        {

            localDbservice.Connect();
            gameDbservice.Connect();

            gameDbservice.LoadAsync();

            StockFishClient client = new StockFishClient();
            var service = client.GetService();

            var depth = short.Parse(args[0]);

            var stDepth = short.Parse(args[1]);

            int elo = short.Parse(args[4]);

            int runTimeId = int.Parse(args[6]);

            var mp = Boot.GetService<MoveProvider>();
            var moves = args[5].Split('-').Select(x => mp.Get(short.Parse(x))).ToList();

            StockFishGame game = new StockFishGame(depth, stDepth, args[2], args[3], elo, moves);

            var saveDepth = Boot.GetService<IConfigurationProvider>().BookConfiguration.SaveDepth;

            gameDbservice.WaitToData();

            StockFishGameResult result = game.Play();

            Console.ForegroundColor = GetColor(result.Color, result.Output);

            Console.WriteLine(result.ToShort());

            Console.ForegroundColor = ConsoleColor.White;

            StockFishResult stockFishResult = new StockFishResult
            {
                StockFishResultItem = new StockFishResultItem
                {
                    Elo = result.Elo,
                    Depth = result.Depth,
                    StockFishDepth = result.StockFishDepth,
                    Strategy = result.Strategy
                },
                Color = result.Color,
                Result = result.Output,
                Opening = string.Join('-', moves.Select(x => x.ToLightString())),
                Sequence = string.Join('-', result.History.Select(x => x.Key).Take(saveDepth)),
                Duration = result.Time,
                MoveTime = result.MoveTime,
                RunTimeId = runTimeId
            };
            var json = JsonConvert.SerializeObject(stockFishResult);
            service.ProcessResult(json);

            timer.Stop();
        }
        finally
        {
            localDbservice.Disconnect();
            gameDbservice.Disconnect();
        }

    }
    private static ConsoleColor GetColor(string color, StockFishGameResultType output)
    {
        if (output == StockFishGameResultType.Draw) return ConsoleColor.Yellow;
        if (color == "w" && output == StockFishGameResultType.White) return ConsoleColor.Red;
        if (color == "b" && output == StockFishGameResultType.Black) return ConsoleColor.Red;
        return ConsoleColor.Green;
    }
}