using CoreWCF;
using DataAccess.Entities;
using Engine.Dal.Interfaces;
using System.Collections.Concurrent;

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

        Console.WriteLine("Service is UP");
    }

    public bool IsFinished { get; set; } = false;

    private void UpdateRecords()
    {
        while (_inProgress)
        {
            Upsert();
        }
    }

    private void Upsert()
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

    public void CleanUp()
    {
        _inProgress = false;

        _updateTask.Wait();

        Thread.Sleep(TimeSpan.FromSeconds(10));

        try
        {
            while (_queue.Count > 0)
            {
                Upsert();
            }
        }
        finally
        {
            _gameDbService.Disconnect();
            IsFinished = true;
        }
    }

    public void ProcessSequence(List<SequenceModel> sequences)
    {
        var count = sequences.Count;

        List<Book> records = new List<Book>(sequences.Count);

        records.AddRange(sequences.Select(s => new Book
        {
            History = s.Sequence,
            NextMove = s.Move,
            White = s.White,
            Draw = s.Draw,
            Black = s.Black
        }));

        _queue.Enqueue(records);
    }

    public void Initialize()
    {
        _inProgress = true;
        _updateTask = Task.Factory.StartNew(UpdateRecords);
    }
}
