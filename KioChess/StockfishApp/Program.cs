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

        int skills = short.Parse(args[4]);

        StockFishGame game = new StockFishGame(depth, stDepth, args[2], args[3],skills);

        var saveDepth = Boot.GetService<IConfigurationProvider>().BookConfiguration.SaveDepth;

        gameDbservice.WaitToData();

        StockFishGameResult result = game.Play();

        Console.WriteLine(result.ToShort());

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
}