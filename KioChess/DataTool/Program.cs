using Engine.Book.Interfaces;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        var das = Boot.GetService<IDataAccessService>();

        try
        {
            das.Connect();
        }
        finally
        {
            das.Disconnect();
        }

        timer.Stop();

        Console.WriteLine(timer.Elapsed);

        Console.ReadLine();
    }
}