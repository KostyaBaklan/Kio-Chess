using StockFishCore.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        Boot.SetUp();

        StockFishDbService stockFishDbService = new StockFishDbService();

        try
        {
            int left = int.Parse(args[0]);
            int right = int.Parse(args[1]);

            stockFishDbService.Connect();

            stockFishDbService.Compare(left,right);
        }
        finally
        {
            stockFishDbService.Disconnect();
        }

        Console.WriteLine("Hello, World!");

        Console.ReadLine();
    }
}