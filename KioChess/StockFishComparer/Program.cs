using StockFishCore.Services;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        Boot.SetUp();

        if (args[0] == "-t")
        {
            CompareResults(args.Skip(1).ToArray());
        }
        else if (args[0] == "-c")
        {
            CompareResults(int.Parse(args.Skip(1).FirstOrDefault()));
        }
        else
        {
            ComparePairResults(args); 
        }
    }

    private static void CompareResults(int id)
    {
        StockFishDbService stockFishDbService = new StockFishDbService();
        string file = null;

        try
        {
            stockFishDbService.Connect();

            file = stockFishDbService.Compare(id);

            if (!string.IsNullOrWhiteSpace(file))
            {
                FileInfo fileInfo = new FileInfo(file);

                Console.WriteLine($"Comparision result is ready, file = '{fileInfo.FullName}'");

                if (fileInfo.Exists)
                {
                    Process.Start(@"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE", fileInfo.FullName);
                }
            }
            else
            {
                Console.WriteLine($"Comparision result is ready");
            }
        }
        finally
        {
            stockFishDbService.Disconnect();
        }
    }

    private static void CompareResults(string[] args)
    {
        StockFishDbService stockFishDbService = new StockFishDbService();
        string file = null;

        try
        {
            stockFishDbService.Connect();

            file = stockFishDbService.Compare(args);

            if (!string.IsNullOrWhiteSpace(file))
            {
                FileInfo fileInfo = new FileInfo(file);

                Console.WriteLine($"Comparision result is ready, file = '{fileInfo.FullName}'");

                if (fileInfo.Exists)
                {
                    Process.Start(@"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE", fileInfo.FullName);
                }
            }
            else
            {
                Console.WriteLine($"Comparision result is ready");
            }
        }
        finally
        {
            stockFishDbService.Disconnect();
        }
    }

    private static void ComparePairResults(string[] args)
    {
        StockFishDbService stockFishDbService = new StockFishDbService();
        string file = null;

        try
        {
            stockFishDbService.Connect();

            for (int l = 0; l < args.Length - 1; l++)
            {
                for (int r = l + 1; r < args.Length; r++)
                {
                    int left = int.Parse(args[l]);
                    int right = int.Parse(args[r]);

                    file = stockFishDbService.Compare(left, right);

                    if (!string.IsNullOrWhiteSpace(file))
                    {
                        FileInfo fileInfo = new FileInfo(file);

                        Console.WriteLine($"Comparision result is ready, file = '{fileInfo.FullName}'");

                        if (fileInfo.Exists)
                        {
                            Process.Start(@"C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE", fileInfo.FullName);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Comparision result is ready");
                    }
                }
            }
        }
        finally
        {
            stockFishDbService.Disconnect();
        }
    }
}