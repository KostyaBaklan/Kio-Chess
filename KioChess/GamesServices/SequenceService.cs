using CoreWCF;
using Engine.Book.Interfaces;
using Engine.Book.Models;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace GamesServices
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,ConcurrencyMode = CoreWCF.ConcurrencyMode.Multiple)]
    public class SequenceService : ISequenceService
    {
        private readonly object _sync = new object();

        private bool _inProgress = true;
        private int _received;
        private int _transfered;
        private readonly int _bufferSize = 1000;
        private int _nextReceive = 1000;
        private int _nextTransfer = 1000;
        private Stopwatch _timer;
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
                if(_queue.TryDequeue(out List<HistoryRecord> records))
                {
                    _gameDbService.Upsert(records);
                    _transfered += records.Count;

                    if (_transfered > _nextTransfer)
                    {
                        Console.WriteLine($"Transfered = {_transfered}    {_timer.Elapsed}");
                        _nextTransfer += _bufferSize;
                    }
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMicroseconds(10));
                }
            }
        }

        public void CleanUp()
        {
            _inProgress = false;

            _updateTask.Wait();

            //if(_records.Count > 0)
            //{
            //    var records = new List<HistoryRecord>(_records.Count);

            //    records.AddRange(_records.Select(s => new HistoryRecord
            //    {
            //        Sequence = s.Sequence,
            //        Move = s.Move,
            //        White = s.White,
            //        Draw = s.Draw,
            //        Black = s.Black
            //    }));

            //    _records.Clear();

            //    _queue.Enqueue(records);
            //}

            try
            {
                while (_queue.Count > 0)
                {
                    if (_queue.TryDequeue(out List<HistoryRecord> records))
                    {
                        _gameDbService.Upsert(records);
                        _transfered += records.Count;

                        Console.WriteLine($"Transfered = {_transfered}    {_timer.Elapsed}");
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromMicroseconds(10));
                    }
                }
            }
            finally
            {
                Console.WriteLine($"Received = {_received}    {_timer.Elapsed}");
                Console.WriteLine($"Transfered = {_transfered}    {_timer.Elapsed}");
                _timer.Stop();
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

            lock (_sync)
            {
                _received += count;
                if (_received > _nextReceive)
                {
                    Console.WriteLine($"Received = {_received}    {_timer.Elapsed}");
                    _nextReceive += _bufferSize;
                }
            }
        }

        public void Initialize()
        {
            _timer = Stopwatch.StartNew();

            _updateTask = Task.Factory.StartNew(UpdateRecords);
        }
    }
}
