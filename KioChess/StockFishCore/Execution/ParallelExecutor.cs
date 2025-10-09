using System.Diagnostics;

namespace StockFishCore.Execution
{
    public class ParallelExecutor
    {
        private readonly double _factor;
        private readonly List<IExecutable> _queue;
        private readonly int _degreeOfParallelism;

        // Removed invalid 'readonly' field declaration here.

        public ParallelExecutor(int degreeOfParallelism, IEnumerable<IExecutable> items)
        {
            _degreeOfParallelism = degreeOfParallelism;

            _queue = [.. items];
            _factor = 100.0 / _queue.Count;

            Console.WriteLine($"Total items: {_queue.Count}");
        }

        public void Execute()
        {
            using (SemaphoreSlim semaphore = new(_degreeOfParallelism, _degreeOfParallelism))
            {
                var task = ExecuteInternal(semaphore);

                task.Wait();
            }
        }

        private async Task ExecuteInternal(SemaphoreSlim semaphore)
        {
            var timer = Stopwatch.StartNew();
            var tasks = new List<Task>();

            for (int i = 0; i < _queue.Count; i++)
            {
                await semaphore.WaitAsync();

                IExecutable executable = _queue[i];

                var currentIndex = i + 1; // capture for closure

                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        executable.Log(currentIndex, timer, Math.Round(currentIndex * _factor, 3));
                        executable.Execute();
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);

            timer.Stop();
        }
    }
}
