using Engine.Dal.Interfaces;
using StockfishApp;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();
        Boot.SetUp(); 

        var depth = short.Parse(args[0]);

        var stDepth = short.Parse(args[1]);

        int skills = short.Parse(args[4]);
        
        var gameDbservice = Boot.GetService<IGameDbService>();

        gameDbservice.Connect();

        gameDbservice.LoadAsync();

        StockFishGame game = new StockFishGame(depth, stDepth, args[2], args[3],skills);

        gameDbservice.WaitToData();

        StockFishGameResult result = game.Play();

        Console.WriteLine(result.ToShort());

        var dir = "Output";
        DirectoryInfo directoryInfo;

        if (!Directory.Exists(dir))
        {
            directoryInfo = Directory.CreateDirectory(dir);
        }
        else
        {
            directoryInfo = new DirectoryInfo(dir);
        }

        result.Save(directoryInfo.FullName);

        //Console.WriteLine(JsonConvert.SerializeObject(result.ToJson(), Formatting.Indented));

        //Console.WriteLine(timer.Elapsed);
        timer.Stop();

        gameDbservice.Disconnect();
    }
}