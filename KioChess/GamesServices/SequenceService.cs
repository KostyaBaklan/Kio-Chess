using CoreWCF;
using Engine.Book.Interfaces;
using Engine.Book.Models;
using System.Collections.Concurrent;

namespace GamesServices
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SequenceService : ISequenceService
    {
        private bool _inProgress;
        private ConcurrentQueue<List<HistoryRecord>> _queue;

        private Task _updateTask;

        private readonly IGameDbService _gameDbService;

        public SequenceService()
        {
            _queue = new ConcurrentQueue<List<HistoryRecord>>();
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
            if (_queue.TryDequeue(out List<HistoryRecord> records))
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

            List<HistoryRecord> records = new List<HistoryRecord>(sequences.Count);

            records.AddRange(sequences.Select(s => new HistoryRecord
            {
                Sequence = s.Sequence,
                Move = s.Move,
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
}
