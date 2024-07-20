using StockFishCore.Services;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        Boot.SetUp();

        StockFishDbService stockFishDbService = new StockFishDbService();
        string file = null;

        try
        {
            int left = int.Parse(args[0]);
            int right = int.Parse(args[1]);

            stockFishDbService.Connect();

            file = stockFishDbService.Compare(left,right);
        }
        finally
        {
            stockFishDbService.Disconnect();
        }

        

        if(!string.IsNullOrWhiteSpace(file))
        {
            FileInfo fileInfo = new FileInfo(file);

            Console.WriteLine($"Comparision result is ready, file = '{fileInfo.FullName}'");

            if (fileInfo.Exists)
            {
                //Process.Start("explorer.exe", "/select," + fileInfo.FullName);

                Process.Start(@"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE", fileInfo.FullName);
            }
        }
        else
        {
            Console.WriteLine($"Comparision result is ready");
        }
    }
}