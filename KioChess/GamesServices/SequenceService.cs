using CoreWCF;
using DataAccess.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Engine.Dal.Interfaces;
using Engine.Dal.Services;
using Engine.Interfaces.Config;
using Newtonsoft.Json;
using ProtoBuf;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Tools.Common;

namespace GamesServices;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
public class SequenceService : ISequenceService
{
    private bool _inProgress;
    private ConcurrentQueue<List<Book>> _queue;

    private Task _updateTask;

    private readonly IMemoryDbService _memoryDbService;
    private readonly IBulkDbService _bulkDbService;

    public SequenceService()
    {
        _queue = new ConcurrentQueue<List<Book>>();
        Boot.SetUp();

        _memoryDbService = Boot.GetService<IMemoryDbService>();
        _memoryDbService.Connect();

        _bulkDbService = Boot.GetService<IBulkDbService>();
        _bulkDbService.Connect();
    }

    public void ProcessSequence(byte[] sequences)
    {
        List<Book> records = Serializer.Deserialize<List<Book>>(sequences.AsSpan());

        _queue.Enqueue(records);
    }

    public void Save()
    {
        //Debugger.Launch();

        var config = Boot.GetService<IConfigurationProvider>();

        var game = Boot.GetService<IGameDbService>();
        game.Connect();

        var before = game.GetTotalGames();

        _inProgress = false;

        _updateTask.Wait();

        Console.WriteLine($"Queue = {_queue.Count}");
        var timer = Stopwatch.StartNew();
        try
        {
            while (_queue.Count > 0 && _queue.TryDequeue(out List<Book> records))
            {
                _memoryDbService.Upsert(records);
            }

            Console.WriteLine($"Total = {_memoryDbService.GetTotalItems()}   Games = {_memoryDbService.GetTotalGames()}   {timer.Elapsed}");
            timer.Stop();

            timer = Stopwatch.StartNew();

            var chunks = _memoryDbService.GetBooks().Chunk(config.BookConfiguration.Chunk);

            int count = 0;

            foreach (var chunk in chunks)
            {
                var t = Stopwatch.StartNew();
                _bulkDbService.Upsert(chunk);
                Console.WriteLine($"{++count}   {t.Elapsed}   {timer.Elapsed}");
                t.Stop();
            }

            var after = game.GetTotalGames();

            Console.WriteLine($"Upsert   Before = {before}, After = {after}, Total = {after - before}   {timer.Elapsed}");

            before = game.GetTotalPopularGames();

            game.UpdateTotal(_bulkDbService);

            after = game.GetTotalPopularGames();

            Console.WriteLine($"UpdateTotal   Before = {before}, After = {after}, Total = {after - before}   {timer.Elapsed}");

            Console.WriteLine();

            ProcessPositionTotalDifferences(game, config.BookConfiguration.Chunk);
        }
        finally
        {
            _memoryDbService.Disconnect();
            _bulkDbService.Disconnect();
            game.Disconnect();
        }
    }

    private void ProcessPositionTotalDifferences(IGameDbService _gameDbService, int chunkSize)
    {

        Console.WriteLine("Process PositionTotalDifferences");
        try
        {
            _gameDbService.ClearPositionTotalDifference();

            IEnumerable<PositionTotalDifference> positions = _gameDbService.LoadPositionTotalDifferences();

            var chunks = positions.Chunk(chunkSize);

            int size = 0;
            int count = 0;

            foreach (var chunk in chunks)
            {
                size += chunk.Length;
                count++;
                Console.WriteLine($"{count} - {size}");

                _gameDbService.Add(chunk);
            }

            Console.WriteLine($"Total PositionTotalDifferences = {_gameDbService.GetPositionTotalDifferenceCount()}");
        }
        catch (Exception e)
        {
            var error = e.ToFormattedString();

            Console.WriteLine(JsonConvert.SerializeObject(new
            {
                Type = GetType(),
                Method = MethodBase.GetCurrentMethod().Name,
                Error = error
            }, Formatting.Indented));
        }
    }

    public void Initialize()
    {
        _inProgress = true;
        _updateTask = Task.Factory.StartNew(UpdateRecords);
        //Debugger.Launch();
    }

    private void UpdateRecords()
    {
        while (_inProgress)
        {
            if (_queue.TryDequeue(out List<Book> records))
            {
                _memoryDbService.Upsert(records);
            }
            else
            {
                Thread.Sleep(TimeSpan.FromMicroseconds(10));
            }
        }
    }
}
