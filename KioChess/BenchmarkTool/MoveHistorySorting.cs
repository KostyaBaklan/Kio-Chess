using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Engine.DataStructures.Moves;
using Engine.Models.Helpers;
using Engine.Models.Sort;

namespace EngineBenchmark
{
    [MemoryDiagnoser]
    [SimpleJob]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)] // Sort by performance (fastest first)
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)] // Group by ArraySize parameter
    public class MoveHistorySorting
    {
        private string _name;
        private MoveHistory[] _testData;
        private MoveHistory[] _workingArray;
        private MoveHistory[] _sortedWorkingArray;
        private MoveHistory[] _lastResult; // Added field for verification

        [Params(5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100)]
        public int ArraySize { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            // Generate test data with random history values
            _testData = new MoveHistory[ArraySize];
            for (int i = 0; i < ArraySize; i++)
            {
                _testData[i] = new MoveHistory((short)i, BenchmarkHelper.Random.Next(1, 100000));
            }

            _workingArray = new MoveHistory[ArraySize];
            _sortedWorkingArray = new MoveHistory[ArraySize];
            _lastResult = new MoveHistory[ArraySize];
        }

        [IterationSetup]
        public void IterationSetup()
        {
            // Copy test data to working array for each iteration
            Array.Copy(_testData, _workingArray, ArraySize);
            Array.Copy(_testData, _sortedWorkingArray, ArraySize);
            Array.Sort(_sortedWorkingArray); // Sort the array for comparison
        }

        [Benchmark(Baseline = true)] // Mark InsertionSort as baseline for comparison
        public void InsertionSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.InsertionSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(InsertionSort);
        }

        [Benchmark]
        public void BinaryInsertionSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.BinaryInsertionSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(BinaryInsertionSort);
        }

        //[Benchmark]
        public void ShellSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.ShellSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(ShellSort);
        }

        //[Benchmark]
        public void QuickSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.QuickSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(QuickSort);
        }

        //[Benchmark]
        public void HeapSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.HeapSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(HeapSort);
        }

        //[Benchmark]
        public void WeakHeapSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.WeakHeapSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(WeakHeapSort);
        }

        //[Benchmark]
        public void CombSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.CombSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(CombSort);
        }

        [Benchmark]
        public void CocktailSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.CocktailSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(CocktailSort);
        }

        [Benchmark]
        public void NetworkSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.NetworkSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(NetworkSort);
        }

        [Benchmark]
        public void TimSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.TimSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(TimSort);
        }

       [Benchmark]
        public void AdaptiveSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.AdaptiveSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(AdaptiveSort);
        }

        //[Benchmark]
        public void ChessSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.ChessSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(ChessSort);
        }

        //[Benchmark]
        public void SmartSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.SmartSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(SmartSort);
        }

        //[Benchmark]
        public void ChessOptimizedSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.ChessOptimizedSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(ChessOptimizedSort);
        }

        //[Benchmark]
        public void UltraSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.UltraSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(UltraSort);
        }

        //[Benchmark]
        public void SelectionSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.SelectionSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(SelectionSort);
        }

        [Benchmark]
        public void StableChessSort()
        {
            Span<MoveHistory> history = stackalloc MoveHistory[_workingArray.Length];
            _workingArray.AsSpan().CopyTo(history);
            history.StableChessSort();
            _lastResult = history.ToArray(); // Store the result for verification
            _name = nameof(SelectionSort);
        }

        [IterationCleanup]
        public void VerifySorting()
        {
            for (int i = 0; i < ArraySize; i++)
            {
                if (!_lastResult[i].Equals(_sortedWorkingArray[i]))
                {
                    var msg = $"[ERROR] {DateTime.Now}: Sorting {_name} verification failed at index {i}. Expected {_sortedWorkingArray[i]}, got {_lastResult[i]}\n";

                    //BenchmarkHelper.LogError($"Benchmark sorting error: {msg}");
                    Console.WriteLine(msg);
                    throw new Exception(msg);
                }
            }
        }
    }
}
