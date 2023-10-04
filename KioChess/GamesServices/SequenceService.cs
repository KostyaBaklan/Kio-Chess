using CoreWCF;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Engine.Dal.Interfaces;
using Newtonsoft.Json;
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

    public void ProcessSequence(string sequences)
    {
        List<Book> records = JsonConvert.DeserializeObject<List<Book>>(sequences);

        _queue.Enqueue(records);
    }

    public void Save()
    {
        Debugger.Launch();

        var game = Boot.GetService<IGameDbService>();
        game.Connect();

        var before = game.GetTotalGames();

        _inProgress = false;

        _updateTask.Wait();

        Task.Factory.StartNew(() => 
        {
            Console.WriteLine($"Queue = {_queue.Count}");
            var timer = Stopwatch.StartNew();
            try
            {
                while (_queue.Count > 0 && _queue.TryDequeue(out List<Book> records))
                {
                    _memoryDbService.Upsert(records);
                }

                Console.WriteLine($"Total = {_memoryDbService.GetTotalGames()}   {timer.Elapsed}");
                timer.Stop();

                timer = Stopwatch.StartNew();

                var chunks = _memoryDbService.GetBooks().Chunk(10000);

                int count = 0;

                foreach (var chunk in chunks)
                {
                    _bulkDbService.Upsert(chunk);
                    Console.WriteLine($"{++count}   {timer.Elapsed}");
                }

                var after = game.GetTotalGames();

                Console.WriteLine($"Before = {before}, After = {after}, Total = {after - before}   {timer.Elapsed}");
            }
            finally
            {
                _memoryDbService.Disconnect();
                _bulkDbService.Disconnect();
                game.Disconnect();
            }
        });
        
        //Thread.Sleep(TimeSpan.FromMinutes(Config.TIMEOUT - 1));
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
