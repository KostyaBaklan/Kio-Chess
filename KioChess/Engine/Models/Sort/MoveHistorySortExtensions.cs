using Engine.DataStructures.Moves;
using Engine.Interfaces.Config;
using System.Runtime.CompilerServices;

namespace Engine.Models.Sort
{
    /// <summary>
    /// Adaptive sorting algorithms for MoveHistory collections.
    /// These algorithms automatically choose between stable and unstable implementations
    /// based on array characteristics and performance requirements.
    /// 
    /// For direct access to specific algorithms, use:
    /// - StableMoveHistorySortExtensions for stable algorithms (preserve relative order of equal elements)
    /// - UnstableMoveHistorySortExtensions for unstable algorithms (optimized for performance)
    /// </summary>
    public static class MoveHistorySortExtensions
    {
        public const byte One = 1;
        public const byte Zero = 0;
        private static readonly int SortThreshold = ContainerLocator.Current.Resolve<IConfigurationProvider>().AlgorithmConfiguration.SortingConfiguration.SortThreshold;

        // Adaptive sorting: chooses optimal algorithm based on array size and stability requirements
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AdaptiveSort(this Span<MoveHistory> items)
        {
            // For very small arrays (≤30), use stable insertion sort for predictable behavior
            if (items.Length < 31)
            {
                items.InsertionSort(); // Stable
            }
            else
            {
                items.TimSort(); // Stable hybrid algorithm
            }
        }

        // Original ChessSort - Balanced approach for chess engine move ordering
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChessSort(this Span<MoveHistory> items)
        {
            // Chess move lists have specific characteristics:
            // - Usually small to medium size (5-80 moves)
            // - History values often have patterns (captures vs quiet moves)
            // - Performance is critical due to frequent sorting

            if (items.Length <= 22)
            {
                // Very small arrays: use stable insertion sort
                items.InsertionSort();
            }
            else if (items.Length <= 42)
            {
                // Small arrays: binary insertion sort reduces comparisons while staying stable
                items.BinaryInsertionSort();
            }
            else
            {
                // Large arrays: TimSort for guaranteed stable performance
                items.TimSort();
            }
        }

        // Enhanced adaptive algorithm that considers array characteristics
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SmartSort(this Span<MoveHistory> items)
        {
            switch (items.Length)
            {
                case 0:
                case 1:
                    return;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                    items.NetworkSort(); // Stable for very small arrays
                    break;
                case < 16:
                    items.InsertionSort(); // Stable for small arrays
                    break;
                case < 35:
                    items.BinaryInsertionSort(); // Stable with reduced comparisons
                    break;
                case < 60:
                    items.ShellSort(); // Unstable but good performance
                    break;
                default:
                    items.QuickSort(); // Unstable but fastest for larger arrays
                    break;
            }
        }

        // Specialized sorting for chess move ordering with move history heuristics
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChessOptimizedSort(this Span<MoveHistory> items)
        {
            // Optimized specifically for chess engine move sorting
            // Balances stability for small arrays with performance for larger ones
            if (items.Length <= 8)
            {
                items.NetworkSort(); // Stable for very small move lists
            }
            else if (items.Length <= 18)
            {
                items.InsertionSort(); // Stable for typical tactical sequences
            }
            else if (items.Length <= 35)
            {
                items.BinaryInsertionSort(); // Stable for quiet position move lists
            }
            else if (items.Length <= 65)
            {
                items.CombSort(); // Unstable but excellent for partially ordered chess moves
            }
            else
            {
                items.QuickSort(); // Unstable but handles large move lists efficiently
            }
        }

        // Advanced adaptive algorithm that includes WeakHeapSort
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UltraSort(this Span<MoveHistory> items)
        {
            switch (items.Length)
            {
                case 0:
                case 1:
                    return;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                    items.NetworkSort(); // Stable for very small arrays
                    break;
                case < 12:
                    items.InsertionSort(); // Stable for small arrays
                    break;
                case < 25:
                    items.BinaryInsertionSort(); // Stable with reduced comparisons
                    break;
                case < 45:
                    items.WeakHeapSort(); // Unstable but better cache performance than HeapSort
                    break;
                case < 70:
                    items.ShellSort(); // Unstable but good gap-based performance
                    break;
                default:
                    items.QuickSort(); // Unstable but fastest for larger arrays
                    break;
            }
        }

        // Stability-preserving adaptive sort
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StableAdaptiveSort(this Span<MoveHistory> items)
        {
            // Always use stable algorithms to preserve relative order of equal elements
            items.StableChessSort();
        }

        // Performance-focused adaptive sort (may sacrifice stability for speed)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FastAdaptiveSort(this Span<MoveHistory> items)
        {
            // Prioritizes performance over stability
            items.FastChessSort();
        }

        // Performance-focused adaptive sort (may sacrifice stability for speed)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GameSort(this Span<MoveHistory> items)
        {
            if (items.Length < SortThreshold)
            {
                items.InsertionSort();
            }
            else
            {
                items.ShellSort();
            }
        }
    }
}
