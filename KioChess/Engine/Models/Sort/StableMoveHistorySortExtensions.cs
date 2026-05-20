using Engine.DataStructures.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Models.Sort
{
    /// <summary>
    /// Stable sorting algorithms for MoveHistory collections.
    /// Stable algorithms preserve the relative order of elements with equal keys.
    /// </summary>
    public static class StableMoveHistorySortExtensions
    {
        public const byte One = 1;
        public const byte Zero = 0;
        private const int RUN = 32;

        // TimSort - Stable hybrid sorting algorithm optimized for real-world data
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TimSort(this Span<MoveHistory> items)
        {
            int n = items.Length;

            // Sort small runs with Insertion Sort
            for (int i = 0; i < n; i += RUN)
            {
                items.Slice(i, Math.Min(RUN, n - i)).InsertionSort();
            }

            // Merge runs
            for (int size = RUN; size < n; size *= 2)
            {
                for (int left = 0; left < n; left += 2 * size)
                {
                    int mid = left + size - 1;
                    int right = Math.Min(left + 2 * size - 1, n - 1);

                    if (mid < right)
                    {
                        Merge(items, left, mid, right);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Merge(Span<MoveHistory> items, int left, int mid, int right)
        {
            int len1 = mid - left + 1;
            int len2 = right - mid;

            Span<MoveHistory> leftSpan = stackalloc MoveHistory[len1];
            Span<MoveHistory> rightSpan = stackalloc MoveHistory[len2];

            for (int i = 0; i < len1; i++)
                leftSpan[i] = items[left + i];
            for (int i = 0; i < len2; i++)
                rightSpan[i] = items[mid + 1 + i];

            int iLeft = 0, iRight = 0, k = left;

            while (iLeft < len1 && iRight < len2)
            {
                // <= ensures stability (left element comes first when equal)
                if (leftSpan[iLeft].CompareTo(rightSpan[iRight]) <= 0)
                    items[k++] = leftSpan[iLeft++];
                else
                    items[k++] = rightSpan[iRight++];
            }

            while (iLeft < len1)
                items[k++] = leftSpan[iLeft++];
            while (iRight < len2)
                items[k++] = rightSpan[iRight++];
        }

        // InsertionSort - Stable O(n²) algorithm, optimal for small arrays
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InsertionSort(this Span<MoveHistory> items)
        {
            for (byte i = One; i < items.Length; i++)
            {
                var key = items[i];
                int j = i - 1;

                // Only move elements that are strictly greater (maintains stability)
                while (j > -1 && key.IsGreater(items[j]))
                {
                    items[j + 1] = items[j];
                    j--;
                }
                items[j + 1] = key;
            }
        }

        // BinaryInsertionSort - Stable variant with binary search for insertion point
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
                    int mid = lo + hi >> 1;
                    if (key.IsGreater(items[mid]))
                        hi = mid;
                    else
                        lo = mid + 1;  // Ensures stability by placing equal elements after existing ones
                }

                // Move elements to make space for key
                for (int j = i; j > lo; j--)
                    items[j] = items[j - 1];

                items[lo] = key;
            }
        }

        // CocktailSort (Bidirectional Bubble Sort) - Stable algorithm, good for nearly sorted data
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CocktailSort(this Span<MoveHistory> items)
        {
            bool swapped = true;
            int start = 0;
            int end = items.Length - 1;

            while (swapped)
            {
                swapped = false;

                // Forward pass
                for (int i = start; i < end; i++)
                {
                    if (items[i + 1].IsGreater(items[i]))
                    {
                        Swap(items, i, i + 1);
                        swapped = true;
                    }
                }
                end--;

                if (!swapped) break;

                swapped = false;

                // Backward pass
                for (int i = end; i > start; i--)
                {
                    if (items[i].IsGreater(items[i - 1]))
                    {
                        Swap(items, i, i - 1);
                        swapped = true;
                    }
                }
                start++;
            }
        }

        // NetworkSort - Stable sorting networks for very small arrays (2-8 elements)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NetworkSort(this Span<MoveHistory> items)
        {
            switch (items.Length)
            {
                case 0:
                case 1:
                    return;
                case 2:
                    NetworkSort2(items);
                    break;
                case 3:
                    NetworkSort3(items);
                    break;
                case 4:
                    NetworkSort4(items);
                    break;
                case 5:
                    NetworkSort5(items);
                    break;
                case 6:
                    NetworkSort6(items);
                    break;
                case 7:
                    NetworkSort7(items);
                    break;
                case 8:
                    NetworkSort8(items);
                    break;
                default:
                    items.InsertionSort(); // Fall back to stable insertion sort for larger arrays
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void NetworkSort2(Span<MoveHistory> items)
        {
            if (items[1].IsGreater(items[0]))
                Swap(items, 0, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void NetworkSort3(Span<MoveHistory> items)
        {
            if (items[1].IsGreater(items[0])) Swap(items, 0, 1);
            if (items[2].IsGreater(items[1])) Swap(items, 1, 2);
            if (items[1].IsGreater(items[0])) Swap(items, 0, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void NetworkSort4(Span<MoveHistory> items)
        {
            if (items[1].IsGreater(items[0])) Swap(items, 0, 1);
            if (items[3].IsGreater(items[2])) Swap(items, 2, 3);
            if (items[2].IsGreater(items[0])) Swap(items, 0, 2);
            if (items[3].IsGreater(items[1])) Swap(items, 1, 3);
            if (items[2].IsGreater(items[1])) Swap(items, 1, 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void NetworkSort5(Span<MoveHistory> items)
        {
            if (items[1].IsGreater(items[0])) Swap(items, 0, 1);
            if (items[3].IsGreater(items[2])) Swap(items, 2, 3);
            if (items[2].IsGreater(items[0])) Swap(items, 0, 2);
            if (items[3].IsGreater(items[1])) Swap(items, 1, 3);
            if (items[4].IsGreater(items[2])) Swap(items, 2, 4);
            if (items[2].IsGreater(items[1])) Swap(items, 1, 2);
            if (items[4].IsGreater(items[3])) Swap(items, 3, 4);
            if (items[3].IsGreater(items[2])) Swap(items, 2, 3);
            if (items[4].IsGreater(items[3])) Swap(items, 3, 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void NetworkSort6(Span<MoveHistory> items)
        {
            if (items[1].IsGreater(items[0])) Swap(items, 0, 1);
            if (items[3].IsGreater(items[2])) Swap(items, 2, 3);
            if (items[5].IsGreater(items[4])) Swap(items, 4, 5);
            if (items[2].IsGreater(items[0])) Swap(items, 0, 2);
            if (items[3].IsGreater(items[1])) Swap(items, 1, 3);
            if (items[4].IsGreater(items[2])) Swap(items, 2, 4);
            if (items[5].IsGreater(items[3])) Swap(items, 3, 5);
            if (items[2].IsGreater(items[1])) Swap(items, 1, 2);
            if (items[4].IsGreater(items[3])) Swap(items, 3, 4);
            if (items[3].IsGreater(items[2])) Swap(items, 2, 3);
            if (items[5].IsGreater(items[4])) Swap(items, 4, 5);
            if (items[4].IsGreater(items[3])) Swap(items, 3, 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void NetworkSort7(Span<MoveHistory> items)
        {
            if (items[1].IsGreater(items[0])) Swap(items, 0, 1);
            if (items[3].IsGreater(items[2])) Swap(items, 2, 3);
            if (items[5].IsGreater(items[4])) Swap(items, 4, 5);
            if (items[2].IsGreater(items[0])) Swap(items, 0, 2);
            if (items[3].IsGreater(items[1])) Swap(items, 1, 3);
            if (items[6].IsGreater(items[4])) Swap(items, 4, 6);
            if (items[5].IsGreater(items[2])) Swap(items, 2, 5);
            if (items[4].IsGreater(items[0])) Swap(items, 0, 4);
            if (items[6].IsGreater(items[3])) Swap(items, 3, 6);
            if (items[2].IsGreater(items[1])) Swap(items, 1, 2);
            if (items[5].IsGreater(items[3])) Swap(items, 3, 5);
            if (items[4].IsGreater(items[2])) Swap(items, 2, 4);
            if (items[6].IsGreater(items[5])) Swap(items, 5, 6);
            if (items[3].IsGreater(items[2])) Swap(items, 2, 3);
            if (items[5].IsGreater(items[4])) Swap(items, 4, 5);
            if (items[4].IsGreater(items[3])) Swap(items, 3, 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void NetworkSort8(Span<MoveHistory> items)
        {
            if (items[1].IsGreater(items[0])) Swap(items, 0, 1);
            if (items[3].IsGreater(items[2])) Swap(items, 2, 3);
            if (items[5].IsGreater(items[4])) Swap(items, 4, 5);
            if (items[7].IsGreater(items[6])) Swap(items, 6, 7);
            if (items[2].IsGreater(items[0])) Swap(items, 0, 2);
            if (items[3].IsGreater(items[1])) Swap(items, 1, 3);
            if (items[6].IsGreater(items[4])) Swap(items, 4, 6);
            if (items[7].IsGreater(items[5])) Swap(items, 5, 7);
            if (items[4].IsGreater(items[0])) Swap(items, 0, 4);
            if (items[5].IsGreater(items[1])) Swap(items, 1, 5);
            if (items[6].IsGreater(items[2])) Swap(items, 2, 6);
            if (items[7].IsGreater(items[3])) Swap(items, 3, 7);
            if (items[2].IsGreater(items[1])) Swap(items, 1, 2);
            if (items[4].IsGreater(items[3])) Swap(items, 3, 4);
            if (items[6].IsGreater(items[5])) Swap(items, 5, 6);
            if (items[3].IsGreater(items[2])) Swap(items, 2, 3);
            if (items[5].IsGreater(items[4])) Swap(items, 4, 5);
            if (items[4].IsGreater(items[3])) Swap(items, 3, 4);
        }

        // Utility method for swapping elements
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(Span<MoveHistory> items, int i, int j)
        {
            (items[i], items[j]) = (items[j], items[i]);
        }

        // Stable Chess Sort - Adaptive stable sorting specifically for chess move ordering
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StableChessSort(this Span<MoveHistory> items)
        {
            // Chess move lists have specific characteristics:
            // - Usually small to medium size (5-80 moves)
            // - History values often have patterns (captures vs quiet moves)
            // - Stability is important to preserve move ordering for equal history values

            if (items.Length <= 8)
            {
                // Very small arrays: use stable network sort
                items.NetworkSort();
            }
            else if (items.Length <= 22)
            {
                // Small arrays: use stable insertion sort
                items.InsertionSort();
            }
            else if (items.Length <= 42)
            {
                // Medium arrays: binary insertion sort reduces comparisons while staying stable
                items.BinaryInsertionSort();
            }
            else
            {
                // Large arrays: TimSort for guaranteed stable performance
                items.TimSort();
            }
        }
    }
}