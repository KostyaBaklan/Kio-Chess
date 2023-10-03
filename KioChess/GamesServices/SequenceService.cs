using DataAccess.Entities;
using Engine.Dal.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace GamesServices;

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
            try
            {
                while (_queue.Count > 0 && _queue.TryDequeue(out List<Book> records))
                {
                    _gameDbService.Upsert(records);
                }
            }
            finally
            {
                _gameDbService.Disconnect();
            }
        });
        
        Thread.Sleep(900);
    }

    public void Initialize()
    {
        _inProgress = true;
        _updateTask = Task.Factory.StartNew(UpdateRecords);
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
