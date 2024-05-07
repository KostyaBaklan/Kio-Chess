using Engine.Dal.Interfaces;
using Engine.Interfaces.Config;
using StockfishApp;
using StockFishCore;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();
        Boot.SetUp(); 
        
        var gameDbservice = Boot.GetService<IGameDbService>();

        gameDbservice.Connect();

        gameDbservice.LoadAsync();

        StockFishClient client = new StockFishClient();
        var service = client.GetService();

        var depth = short.Parse(args[0]);

        var stDepth = short.Parse(args[1]); 
        
        int elo = short.Parse(args[4]);

        var move = short.Parse(args[5]);

        StockFishGame game = new StockFishGame(depth, stDepth, args[2], args[3],elo, move);

        var saveDepth = Boot.GetService<IConfigurationProvider>().BookConfiguration.SaveDepth;

        gameDbservice.WaitToData();

        StockFishGameResult result = game.Play(); 
        
        Console.ForegroundColor = GetColor(result.Color, result.Output);

        Console.WriteLine(result.ToShort());

        Console.ForegroundColor = ConsoleColor.White;

        service.ProcessResult(new StockFishResult
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
            Sequence = string.Join('-', result.History.Select(x => x.Key).Take(saveDepth))
        });

        timer.Stop();

        gameDbservice.Disconnect();
    }
    private static ConsoleColor GetColor(string color, StockFishGameResultType output)
    {
        if (output == StockFishGameResultType.Draw) return ConsoleColor.Yellow;
        if (color == "w" && output == StockFishGameResultType.White) return ConsoleColor.Red;
        if (color == "b" && output == StockFishGameResultType.Black) return ConsoleColor.Red;
        return ConsoleColor.Green;
    }
}