using DataAccess.Interfaces;
using Engine.Dal.Interfaces;
using System.Diagnostics;
using System.Text;

internal class Program
{
    private static IOpeningDbService _openingDbService;
    private static IGameDbService _gameDbService;
    private static ILocalDbService _localDbService;

    private static void Main(string[] args)
    {
        Boot.SetUp();
        var timer = Stopwatch.StartNew();

        _openingDbService = Boot.GetService<IOpeningDbService>();
        _gameDbService = Boot.GetService<IGameDbService>();
        _localDbService = Boot.GetService<ILocalDbService>();

        try
        {
            //inMemory.Connect();
            _openingDbService.Connect();
            _gameDbService.Connect();
            _localDbService.Connect();

            Dictionary<int, int> lengthCount = Enumerable.Range(0, 50).ToDictionary(i => i, i => 0);



            //ProcessPositionTotalDifferences();

            //var differentPositions = new HashSet<string>();
            int count = 0;
            string sql = $@"SELECT distinct History
                        from Books";

            var sequences = _gameDbService.Execute(sql, r =>
            {
                return Encoding.Unicode.GetString(r[0] as byte[]);
            }, timeout: 300);

            foreach (var chunk in sequences.Chunk(25000))
            {
                foreach (var sequence in chunk)
                {
                    //differentPositions.Add(sequence);

                    lengthCount[sequence.Length]++;
                }

                count += chunk.Length;

                Console.WriteLine($"{count} {timer.Elapsed}");
            }

            Console.WriteLine();
            foreach (var kvp in lengthCount)
            {
                Console.WriteLine($"{kvp.Key} - {kvp.Value}");
            }
            Console.WriteLine();
        }
        finally
        {
            // inMemory.Disconnect();
            _openingDbService.Disconnect();
            _gameDbService.Disconnect();
            _localDbService?.Disconnect();
        }

        timer.Stop();
        Console.WriteLine();
        Console.WriteLine(timer.Elapsed);
        Console.WriteLine();
        Console.WriteLine($"Finished !!!");
        Console.ReadLine();
    }

    private static void ProcessPositionTotalDifferences()
    {
        Console.WriteLine("Clear Positions");
        _localDbService.ClearPositions();

        _localDbService.Shrink();

        var positions = _gameDbService.LoadPositions();

        var chunks = positions.Chunk(25000);

        int size = 0;
        int count = 0;

        foreach (var chunk in chunks)
        {
            size += chunk.Length;
            count++;
            Console.WriteLine($"{count} - {size}");

            _localDbService.Add(chunk);
        }

        Console.WriteLine($"Total Positions = {_localDbService.GetPositionsCount()}");
    }
}