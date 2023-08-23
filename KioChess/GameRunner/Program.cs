using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        object sync = new object();
        int count = 0;

        int size = 1;

        int dataSize = size * Environment.ProcessorCount;

        Parallel.ForEach(Enumerable.Range(0, dataSize), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (x) =>
        {
            var process = Process.Start(@"GameTool.exe");

            process.WaitForExit();

            var time = timer.Elapsed;

            lock (sync)
            {
                Console.WriteLine($"{++count} {Math.Round(100.0 * count / dataSize, 4)}% {time}");
            }
        });

        timer.Stop();

        Console.WriteLine();
        Console.WriteLine($"Total: {timer.Elapsed}");

        Console.WriteLine();
        Console.WriteLine();

        Console.WriteLine("Finished !!!");
        Console.ReadLine();
    }
}