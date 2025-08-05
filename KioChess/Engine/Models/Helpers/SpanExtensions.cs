using Engine.DataStructures.Moves;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Models.Helpers;

public static class SpanExtensions
{
    public static byte One = 1;
    public static byte Zero = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Order(this Span<short> items)
    {
        for (byte i = One; i < items.Length; i++)
        {
            var key = items[i];
            int j = i - 1;

            while (j > -1 && key < items[j])
            {
                items[j + 1] = items[j];
                j--;
            }
            items[j + 1] = key;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InsertionSort(this Span<MoveHistory> items)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void BinaryInsertionSort(this Span<MoveHistory> items)
    {
        for (int i = 1; i < items.Length; i++)
        {
            MoveHistory key = items[i];
            int lo = 0;
            int hi = i;

            // Binary search to find insertion point
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (key.IsGreater(items[mid]))
                    hi = mid;
                else
                    lo = mid + 1;
            }

            // Move elements to make space for key
            for (int j = i; j > lo; j--)
                items[j] = items[j - 1];

            items[lo] = key;
        }
    }

    // ShellSort with Ciura gap sequence - Better O(n^1.25) performance with good cache locality
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

    // Adaptive sorting: chooses optimal algorithm based on array size
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AdaptiveSort(this Span<MoveHistory> items)
    {
        if (items.Length < 2) return;

        // For very small arrays (≤16), insertion sort is fastest due to low overhead
        if (items.Length <= 16)
        {
            items.InsertionSort();
        }
        // For small to medium arrays (17-32), binary insertion sort reduces comparisons
        else if (items.Length <= 32)
        {
            items.BinaryInsertionSort();
        }
        // For medium arrays (33-80), ShellSort provides good O(n^1.25) performance
        else if (items.Length <= 80)
        {
            items.ShellSort();
        }
        // For large arrays (>80), Introsort provides optimal O(n log n) with worst-case guarantee
        else
        {
            items.IntroSort();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InsertionSort(this Span<AttackBase> items)
    {
        for (int i = One; i < items.Length; i++)
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

    // Introsort (Introspective Sort) - Hybrid algorithm with O(n log n) worst-case guarantee
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IntroSort(this Span<MoveHistory> items)
    {
        // Calculate max depth: 2 * log2(n)
        int maxDepth = 2 * (int)Math.Log2(items.Length);
        IntroSortRecursive(items, 0, items.Length - 1, maxDepth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void IntroSortRecursive(Span<MoveHistory> items, int low, int high, int maxDepth)
    {
        while (high > low)
        {
            int size = high - low + 1;

            // Use insertion sort for small arrays (≤16 elements)
            if (size <= 16)
            {
                InsertionSortRange(items, low, high);
                return;
            }

            // Switch to heapsort if recursion depth exceeds threshold
            if (maxDepth == 0)
            {
                HeapSortRange(items, low, high);
                return;
            }

            // Use quicksort partition
            int pivot = PartitionHoare(items, low, high);
            maxDepth--;

            // Recursively sort the smaller partition first (tail recursion optimization)
            if (pivot - low < high - pivot)
            {
                IntroSortRecursive(items, low, pivot, maxDepth);
                low = pivot + 1; // Continue with larger partition
            }
            else
            {
                IntroSortRecursive(items, pivot + 1, high, maxDepth);
                high = pivot; // Continue with smaller partition
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InsertionSortRange(Span<MoveHistory> items, int low, int high)
    {
        for (int i = low + 1; i <= high; i++)
        {
            MoveHistory key = items[i];
            int j = i - 1;

            while (j >= low && key.IsGreater(items[j]))
            {
                items[j + 1] = items[j];
                j--;
            }

            items[j + 1] = key;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int PartitionHoare(Span<MoveHistory> items, int low, int high)
    {
        // Choose median-of-three as pivot for better performance
        int mid = low + (high - low) / 2;
        if (items[mid].IsGreater(items[low]))
            Swap(ref items[low], ref items[mid]);
        if (items[high].IsGreater(items[low]))
            Swap(ref items[low], ref items[high]);
        if (items[high].IsGreater(items[mid]))
            Swap(ref items[mid], ref items[high]);

        MoveHistory pivot = items[mid];
        int i = low - 1;
        int j = high + 1;

        while (true)
        {
            do i++; while (pivot.IsGreater(items[i]));
            do j--; while (items[j].IsGreater(pivot));

            if (i >= j) return j;

            Swap(ref items[i], ref items[j]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void HeapSortRange(Span<MoveHistory> items, int low, int high)
    {
        int size = high - low + 1;

        // Build max heap
        for (int i = size / 2 - 1; i >= 0; i--)
            Heapify(items, low, size, i);

        // Extract elements from heap one by one
        for (int i = size - 1; i > 0; i--)
        {
            Swap(ref items[low], ref items[low + i]);
            Heapify(items, low, i, 0);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Heapify(Span<MoveHistory> items, int offset, int size, int root)
    {
        int largest = root;
        int left = 2 * root + 1;
        int right = 2 * root + 2;

        // Find largest among root, left child and right child
        if (left < size && items[offset + left].IsGreater(items[offset + largest]))
            largest = left;

        if (right < size && items[offset + right].IsGreater(items[offset + largest]))
            largest = right;

        // If largest is not root, swap and continue heapifying
        if (largest != root)
        {
            Swap(ref items[offset + root], ref items[offset + largest]);
            Heapify(items, offset, size, largest);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Swap(ref MoveHistory a, ref MoveHistory b)
    {
        (a, b) = (b, a);
    }

    // TimSort - Stable hybrid sorting algorithm optimized for real-world data
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TimSort(this Span<MoveHistory> items)
    {
        // Use binary insertion sort for small arrays
        if (items.Length < 32)
        {
            items.BinaryInsertionSort();
            return;
        }

        // For larger arrays, use a simplified TimSort approach
        // In practice, we'll use ShellSort as it's simpler and performs well for our use case
        items.ShellSort();
    }

    // Optimized sorting specifically for chess move evaluation
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ChessSort(this Span<MoveHistory> items)
    {
        // Chess move lists have specific characteristics:
        // - Usually small to medium size (5-80 moves)
        // - History values often have patterns (captures vs quiet moves)
        // - Performance is critical due to frequent sorting

        if (items.Length <= 12)
        {
            // Very small arrays: use optimized insertion sort
            items.InsertionSort();
        }
        else if (items.Length <= 32)
        {
            // Small arrays: binary insertion sort reduces comparisons
            items.BinaryInsertionSort();
        }
        else if (items.Length <= 64)
        {
            // Medium arrays: ShellSort excels here
            items.ShellSort();
        }
        else
        {
            // Large arrays: Introsort for guaranteed performance
            items.IntroSort();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FindIndex(this Span<MoveBase> span, short key)
    {
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i].Key == key) return i;
        }
        return -1;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Join(this Span<short> span, char separator)
    {
        StringBuilder builder = new();

        for (byte i = Zero; i < span.Length - 1; i++)
        {
            builder.Append($"{span[i]}{separator}");
        }

        builder.Append(span[span.Length - 1]);

        return builder.ToString();
    }
}
