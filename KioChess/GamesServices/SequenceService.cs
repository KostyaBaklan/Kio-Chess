using CoreWCF;
using DataAccess.Entities;
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

    private readonly IGameDbService _gameDbService;

    public SequenceService()
    {
        _queue = new ConcurrentQueue<List<Book>>();
        Boot.SetUp();
        _gameDbService = Boot.GetService<IGameDbService>();
        _gameDbService.Connect();
    }

    public void ProcessSequence(string sequences)
    {
        List<Book> records = JsonConvert.DeserializeObject<List<Book>>(sequences);

        _queue.Enqueue(records);
    }

    public void CleanUp()
    {
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
                    _gameDbService.Upsert(records);
                }
            }
            finally
            {
                Console.WriteLine($"Total = {_gameDbService.GetTotalGames()}   {timer.Elapsed}");
                timer.Stop();
                _gameDbService.Disconnect();
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
                _gameDbService.Upsert(records);
            }
            else
            {
                Thread.Sleep(TimeSpan.FromMicroseconds(10));
            }
        }
    }
}
