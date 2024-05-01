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

        StockFishGame game = new StockFishGame(depth, stDepth, args[2], args[3] == "w",skills);

        gameDbservice.WaitToData();

        StockFishGameResult result = game.Play();

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine(result.ToShort());
        Console.WriteLine();
        Console.WriteLine(result.Board);
        Console.WriteLine();
        Console.WriteLine();

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

        File.WriteAllText($"{directoryInfo.FullName}\\{args[2]}_{args[3]}_{args[0]}_{args[1]}_{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss_ffff")}_{new Random().Next()}.txt", result.ToString());
        //Console.WriteLine(JsonConvert.SerializeObject(result.ToJson(), Formatting.Indented));

        Console.WriteLine(timer.Elapsed);
        timer.Stop();

        gameDbservice.Disconnect();
    }
}