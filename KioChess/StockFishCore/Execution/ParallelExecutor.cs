using System.Collections.Concurrent;
using System.Diagnostics;

namespace StockFishCore.Execution
{
    public class ParallelExecutor
    {
        private readonly double _factor;
        private readonly ConcurrentQueue<IExecutable> _queue;
        private readonly int _degreeOfParallelism;

        // Removed invalid 'readonly' field declaration here.

        public ParallelExecutor(int degreeOfParallelism, IEnumerable<IExecutable> items)
        {
            _degreeOfParallelism = degreeOfParallelism;

            _queue = new ConcurrentQueue<IExecutable>(items);
            _factor = 100.0 / _queue.Count;

            Console.WriteLine($"Total items: {_queue.Count}");
        }

        public void Execute()
        {
            int totalItems = _queue.Count;
            var timer = Stopwatch.StartNew();
            Action[] actions = new Action[_degreeOfParallelism];
            for (int i = 0; i < _degreeOfParallelism; i++)
            {
                actions[i] = () =>
                {
                    while(_queue.TryDequeue(out IExecutable executable))
                    {
                        int currentIndex = totalItems - _queue.Count;
                        executable.Log(currentIndex, timer, Math.Round(currentIndex * _factor, 3));
                        executable.Execute();
                    }
                };
            }
            Parallel.Invoke(actions);
            timer.Stop();
        }
    }
}
