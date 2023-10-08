using CoreWCF;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Engine.Dal.Interfaces;
using Engine.Interfaces.Config;
using ProtoBuf;
using System.Collections.Concurrent;
using System.Diagnostics;

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

            Console.WriteLine($"Before = {before}, After = {after}, Total = {after - before}   {timer.Elapsed}");

            game.UpdateTotal(_bulkDbService);

            Console.WriteLine($"UpdateTotal   {timer.Elapsed}");
        }
        finally
        {
            _memoryDbService.Disconnect();
            _bulkDbService.Disconnect();
            game.Disconnect();
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
