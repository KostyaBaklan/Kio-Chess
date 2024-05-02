using Engine.Dal.Interfaces;
using StockfishApp;
using StockFishCore;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();
        Boot.SetUp(); 
        
        StockFishClient client = new StockFishClient();
        var service = client.GetService();

        var depth = short.Parse(args[0]);

        var stDepth = short.Parse(args[1]);

        int skills = short.Parse(args[4]);
        
        var gameDbservice = Boot.GetService<IGameDbService>();

        gameDbservice.Connect();

        gameDbservice.LoadAsync();

        StockFishGame game = new StockFishGame(depth, stDepth, args[2], args[3],skills);

        gameDbservice.WaitToData();

        StockFishGameResult result = game.Play();

        //Console.WriteLine(result.ToShort());

        //var dir = "Output";
        //DirectoryInfo directoryInfo;

        //if (!Directory.Exists(dir))
        //{
        //    directoryInfo = Directory.CreateDirectory(dir);
        //}
        //else
        //{
        //    directoryInfo = new DirectoryInfo(dir);
        //}

        //result.Save(directoryInfo.FullName);

        service.ProcessResult(new StockFishResult
        {
            StockFishResultItem = new StockFishResultItem
            {
                Skill = result.Skill,
                Depth = result.Depth,
                StockFishDepth = result.StockFishDepth,
                Strategy = result.Strategy
            },
            Color = result.Color,
            Result = result.Output
        });

        //Console.WriteLine(JsonConvert.SerializeObject(result.ToJson(), Formatting.Indented));

        //Console.WriteLine(timer.Elapsed);
        timer.Stop();

        gameDbservice.Disconnect();
    }
}