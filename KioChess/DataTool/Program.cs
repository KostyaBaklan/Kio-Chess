using DataAccess.Entities;
using DataAccess.Interfaces;
using Engine.Dal.Interfaces;
using Engine.Models.Helpers;
using System.Diagnostics;

internal class Program
{
    private static IOpeningDbService _openingDbService;
    private static IGameDbService _gameDbService;
    private static IBulkDbService _bulkDbService;
    private static ILocalDbService _localDbService;

    private static void Main(string[] args)
    {
        Boot.SetUp();
        var timer = Stopwatch.StartNew();

        _openingDbService = Boot.GetService<IOpeningDbService>();
        _gameDbService = Boot.GetService<IGameDbService>();
        //var inMemory = Boot.GetService<IMemoryDbService>();
        _bulkDbService = Boot.GetService<IBulkDbService>();
        _localDbService = Boot.GetService<ILocalDbService>();

        try
        {
            //inMemory.Connect();
            _openingDbService.Connect();
            _gameDbService.Connect();
            _bulkDbService.Connect();
            _localDbService.Connect();

            ProcessPositionTotalDifferences();
        }
        finally
        {
            // inMemory.Disconnect();
            _openingDbService.Disconnect();
            _gameDbService.Disconnect();
            _bulkDbService?.Disconnect();
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