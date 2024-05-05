using System.Diagnostics;

namespace StockFishTool
{

    public class ParallelExecutor
    {
        private readonly double _factor;
        private readonly Task[] _tasks;
        private readonly Queue<IExecutable> _queue;

        public ParallelExecutor(int degreeOfParallelism, IEnumerable<IExecutable> items)
        {
            _tasks = new Task[degreeOfParallelism];
            for (int i = 0; i < degreeOfParallelism; i++)
            {
                _tasks[i] = Task.CompletedTask;
            }
            _queue = new Queue<IExecutable>(items);
            _factor = 100.0/_queue.Count; 
            
            Console.WriteLine($"Total items: {_queue.Count}");
        }

        public void Execute()
        {
            int index = 0;
            var timer = Stopwatch.StartNew();

            while (_queue.Count > 0)
            {
                for (int i = 0; i < _tasks.Length; i++)
                {
                    if (_tasks[i].IsCompleted)
                    {
                        var executable = _queue.Dequeue();

                        var task = new Task(executable.Execute, TaskCreationOptions.LongRunning);

                        executable.Log(index++, timer, Math.Round(index * _factor, 3));

                        _tasks[i] = task;

                        task.Start();

                        break;
                    }
                }

                Thread.Sleep(1);
            }

            Task.WaitAll(_tasks);
        }
    }
}
