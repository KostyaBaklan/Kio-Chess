using StockFishCore.Execution;
using StockFishCore.Net;
using System.Diagnostics;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Boot.SetUp();
        StockFishClient.StartServer();

        if (!Directory.Exists("Log"))
        {
            Directory.CreateDirectory("Log");
        }

        Thread.Sleep(2000);

        var branch = BranchFactory.Create();

        BranchExecutor branchExecutor = new BranchExecutor(branch);

        branchExecutor.Execute();

        StockFishClient client = new StockFishClient();
        var serviceClient = client.GetClient();

        await serviceClient.CallAsync("Save");

        Console.WriteLine(" ----- Please enter branch ID to compare:");
        var id = Console.ReadLine();

        Process process = Process.Start("StockFishComparer.exe", $"{id} {branch.Id}");

        process.WaitForExit();

        Console.WriteLine();
        Console.WriteLine(" ----- ");
        Console.WriteLine();
        Console.WriteLine("Yalla");
        Console.WriteLine("^C");
        //Console.ReadLine();
    }
}