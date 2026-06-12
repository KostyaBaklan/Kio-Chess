using Engine.DataStructures.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Models.Sort
{
    /// <summary>
    /// Unstable sorting algorithms for MoveHistory collections.
    /// Unstable algorithms may change the relative order of elements with equal keys,
    /// but often provide better performance characteristics.
    /// </summary>
    public static class UnstableMoveHistorySortExtensions
    {
        public const byte One = 1;
        public const byte Zero = 0;

        // QuickSort with optimizations for small arrays - O(n log n) average, very fast for random data
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QuickSort(this Span<MoveHistory> items)
        {
            QuickSortInternal(items, 0, items.Length - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void QuickSortInternal(Span<MoveHistory> items, int low, int high)
        {
            // Use insertion sort for small subarrays (< 16 elements)
            // Note: Using stable insertion sort here for small arrays
            if (high - low < 16)
            {
                items.Slice(low, high - low + 1).InsertionSortUnstable();
                return;
            }

            int pivot = Partition(items, low, high);
            QuickSortInternal(items, low, pivot - 1);
            QuickSortInternal(items, pivot + 1, high);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Partition(Span<MoveHistory> items, int low, int high)
        {
            // Use median-of-three for pivot selection
            int mid = low + (high - low) / 2;
            if (items[mid].IsGreater(items[low]))
                Swap(items, low, mid);
            if (items[high].IsGreater(items[low]))
                Swap(items, low, high);
            if (items[high].IsGreater(items[mid]))
                Swap(items, mid, high);

            MoveHistory pivot = items[mid];
            Swap(items, mid, high);

            int i = low - 1;
            for (int j = low; j < high; j++)
            {
                if (items[j].IsGreater(pivot))
                {
                    i++;
                    Swap(items, i, j);
                }
            }
            Swap(items, i + 1, high);
            return i + 1;
        }

        // HeapSort - O(n log n) guaranteed, good for worst-case scenarios (unstable, descending order)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void HeapSort(this Span<MoveHistory> items)
        {
            int n = items.Length;

            // Build max heap (largest at root)
            for (int i = n / 2 - 1; i >= 0; i--)
                Heapify(items, n, i);

            // Extract elements from heap one by one
            for (int i = n - 1; i > 0; i--)
            {
                Swap(items, 0, i); // Move current root to end
                Heapify(items, i, 0); // call max heapify on the reduced heap
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Heapify(Span<MoveHistory> items, int n, int i)
        {
            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            // For descending order, maintain max-heap (largest at root)
            if (left < n && items[left].IsGreater(items[largest]))
                largest = left;

            if (right < n && items[right].IsGreater(items[largest]))
                largest = right;

            if (largest != i)
            {
                Swap(items, i, largest);
                Heapify(items, n, largest);
            }
        }

        // WeakHeapSort - Improved heap sort with fewer comparisons and better cache performance (unstable, descending order)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WeakHeapSort(this Span<MoveHistory> items)
        {
            int n = items.Length;

            // Calculate the number of bytes needed for the reverse bit array
            // Each node in the weak heap needs 1 bit, so we need (n+7)/8 bytes
            int reverseBytes = (n + 7) / 8;
            Span<byte> reverse = stackalloc byte[reverseBytes];
            reverse.Clear();

            // Build weak heap from the bottom up
            for (int i = n - 1; i > 0; i--)
            {
                WeakHeapify(items, reverse, n, i);
            }

            // Extract elements from the weak heap one by one
            for (int i = n - 1; i > 0; i--)
            {
                // Move current root (largest element) to the end
                Swap(items, 0, i);

                // Restore weak heap property by merging along the right path
                WeakHeapify(items, reverse, i, 0);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WeakHeapify(Span<MoveHistory> items, Span<byte> reverse, int n, int i)
        {
            while (true)
            {
                int j = 2 * i + GetReverseBit(reverse, i);
                if (j >= n) break;

                if (items[j].IsGreater(items[i]))
                {
                    Swap(items, i, j);
                    FlipReverseBit(reverse, i);
                    i = j;
                }
                else
                {
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetReverseBit(Span<byte> reverse, int index)
        {
            int byteIndex = index / 8;
            int bitIndex = index % 8;
            return reverse[byteIndex] >> bitIndex & 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FlipReverseBit(Span<byte> reverse, int index)
        {
            int byteIndex = index / 8;
            int bitIndex = index % 8;
            reverse[byteIndex] ^= (byte)(1 << bitIndex);
        }

        // ShellSort with Ciura gap sequence - Better O(n^1.25) performance with good cache locality (unstable)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ShellSort(this Span<MoveHistory> items)
        {
            // Ciura gap sequence: 1, 4, 10, 23, 57, 132, 301, 701, 1750, ...
            // Formula: next = 2.25 * previous, but we use precomputed values for better performance
            ReadOnlySpan<int> gaps = stackalloc int[] { 701, 301, 132, 57, 23, 10, 4, 1 };

            // Start with the largest gap that's smaller than array length
            int gapIndex = 0;
            while (gapIndex < gaps.Length && gaps[gapIndex] >= items.Length)
                gapIndex++;

            // Perform gapped insertion sort for each gap
            for (; gapIndex < gaps.Length; gapIndex++)
            {
                int gap = gaps[gapIndex];

                // Perform insertion sort on elements separated by gap
                for (int i = gap; i < items.Length; i++)
                {
                    MoveHistory temp = items[i];
                    int j = i;

                    // Shift elements that are greater than temp by gap positions
                    while (j >= gap && temp.IsGreater(items[j - gap]))
                    {
                        items[j] = items[j - gap];
                        j -= gap;
                    }

                    items[j] = temp;
                }
            }
        }

        // CombSort - Improved bubble sort with shrinking gap, very fast for partially sorted data (unstable)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CombSort(this Span<MoveHistory> items)
        {
            int gap = items.Length;
            bool swapped = true;
            const double shrink = 1.3;

            while (gap > 1 || swapped)
            {
                gap = (int)(gap / shrink);
                if (gap < 1) gap = 1;

                swapped = false;
                for (int i = 0; i + gap < items.Length; i++)
                {
                    if (items[i + gap].IsGreater(items[i]))
                    {
                        Swap(items, i, i + gap);
                        swapped = true;
                    }
                }
            }
        }

        // SelectionSort - Simple O(n²) algorithm, good for very small arrays (unstable)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SelectionSort(this Span<MoveHistory> items)
        {
            for (int i = 0; i < items.Length - 1; i++)
            {
                int maxIndex = i;
                for (int j = i + 1; j < items.Length; j++)
                {
                    if (items[j].IsGreater(items[maxIndex]))
                        maxIndex = j;
                }
                if (maxIndex != i)
                    Swap(items, i, maxIndex);
            }
        }

        // Simple insertion sort variant that doesn't guarantee stability
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InsertionSortUnstable(this Span<MoveHistory> items)
        {
            for (byte i = One; i < items.Length; i++)
            {
                var key = items[i];
                int j = i - 1;

                while (j > -1 && key.IsGreater(items[j]))
                {
                    items[j + 1] = items[j];
                    j--;
                }
                items[j + 1] = key;
            }
        }

        // Utility method for swapping elements
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(Span<MoveHistory> items, int i, int j)
        {
            (items[i], items[j]) = (items[j], items[i]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(ref MoveHistory a, ref MoveHistory b)
        {
            (a, b) = (b, a);
        }


        // Performance-focused Chess Sort - Adaptive unstable sorting for maximum speed
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FastChessSort(this Span<MoveHistory> items)
        {
            // Optimized specifically for chess engine performance where stability is not critical
            // Prioritizes speed over stability for move ordering

            if (items.Length <= 8)
            {
                items.SelectionSort(); // Simple and fast for very small arrays
            }
            else if (items.Length <= 18)
            {
                items.InsertionSortUnstable(); // Fast for small arrays
            }
            else if (items.Length <= 35)
            {
                items.ShellSort(); // Good gap-based performance
            }
            else if (items.Length <= 65)
            {
                items.CombSort(); // Excellent for partially ordered chess moves
            }
            else if (items.Length <= 85)
            {
                items.WeakHeapSort(); // Better cache performance than HeapSort
            }
            else
            {
                items.QuickSort(); // Fastest for larger arrays
            }
        }

        // Speed-optimized adaptive algorithm
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SpeedSort(this Span<MoveHistory> items)
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
                    items.SelectionSort(); // Simple for very small arrays
                    break;
                case < 16:
                    items.InsertionSortUnstable(); // Fast for small arrays
                    break;
                case < 35:
                    items.ShellSort(); // Good gap-based performance
                    break;
                case < 60:
                    items.CombSort(); // Fast for partially sorted data
                    break;
                case < 80:
                    items.WeakHeapSort(); // Better cache performance
                    break;
                default:
                    items.QuickSort(); // Fastest for larger arrays
                    break;
            }
        }
    }
}