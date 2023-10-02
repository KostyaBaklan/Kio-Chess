using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Tools;

public abstract class Sorter
{
    protected int[] _array;
    protected Sorter(int[] arr)
    {
        _array = new int[arr.Length];
        Array.Copy(arr,_array,arr.Length);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Swap(int i, int j)
    {
        var t = _array[i];
        _array[i] = _array[j];
        _array[j] = t;
    }

    public TimeSpan Sort()
    {
        Stopwatch timer = Stopwatch.StartNew();
        SortInternal();
        timer.Stop();
        return timer.Elapsed;
    }

    protected abstract void SortInternal();
}

public class InsertionSorter : Sorter
{
    public InsertionSorter(int[] arr) : base(arr)
    {
    }

    protected override void SortInternal()
    {
        int n = _array.Length;
        for (int i = 1; i < n; ++i)
        {
            int key = _array[i];
            int j = i - 1;

            // Move elements of arr[0..i-1],
            // that are greater than key,
            // to one position ahead of
            // their current position
            while (j >= 0 && _array[j] > key)
            {
                _array[j + 1] = _array[j];
                j--;
            }
            _array[j + 1] = key;
        }
    }
}

public class SelectionSorter : Sorter
{
    public SelectionSorter(int[] arr) : base(arr)
    {
    }

    protected override void SortInternal()
    {
        int n = _array.Length;

        // One by one move boundary of unsorted subarray
        for (int i = 0; i < 6; i++)
        {
            // Find the minimum element in unsorted array
            int min_idx = i;
            for (int j = i + 1; j < n; j++)
                if (_array[j] < _array[min_idx])
                    min_idx = j;

            // Swap the found minimum element with the first
            // element
            Swap(min_idx, i);
        }
    }
}

public class BubbleSorter : Sorter
{
    public BubbleSorter(int[] arr) : base(arr)
    {
    }

    protected override void SortInternal()
    {
        int n = _array.Length;
        for (int i = 0; i < n - 1; i++)
            for (int j = 0; j < n - i - 1; j++)
                if (_array[j] > _array[j + 1])
                {
                    // swap temp and arr[i]
                    Swap(j, j + 1);
                }
    }
}

public class QuickSorter : Sorter
{
    public QuickSorter(int[] arr) : base(arr)
    {
    }

    protected override void SortInternal()
    {
        quickSort(0, _array.Length - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void quickSort(int low, int high)
    {
        if (low < high)
        {

            // pi is partitioning index, arr[p]
            // is now at right place
            int pi = partition(low, high);

            // Separately sort elements before
            // partition and after partition
            quickSort(low, pi - 1);
            quickSort(pi + 1, high);
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int partition(int low, int high)
    {

        // pivot
        int pivot = _array[high];

        // Index of smaller element and
        // indicates the right position
        // of pivot found so far
        int i = low - 1;

        for (int j = low; j <= high - 1; j++)
        {

            // If current element is smaller
            // than the pivot
            if (_array[j] < pivot)
            {

                // Increment index of
                // smaller element
                i++;
                Swap(i, j);
            }
        }
        Swap(i + 1, high);
        return i + 1;
    }
}

public class ArraySorter : Sorter
{
    public ArraySorter(int[] arr) : base(arr)
    {
    }

    protected override void SortInternal()
    {
        Array.Sort(_array);
    }
}
