using Engine.Book.Interfaces;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        
        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        var das = Boot.GetService<IDataAccessService>();
        var bookService = Boot.GetService<IBookService>();

        try
        {
            das.Connect();
        }
        finally
        {

            das.Disconnect();
        }
        timer.Stop();

        Console.WriteLine();

        Console.WriteLine(timer.Elapsed);
        Console.WriteLine();

        Console.WriteLine($"Finished !!!");

        Console.ReadLine();
    }
}