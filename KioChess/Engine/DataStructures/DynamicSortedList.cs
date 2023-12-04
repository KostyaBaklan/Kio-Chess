using System.Runtime.CompilerServices;

namespace Engine.DataStructures;

public class DynamicSortedList<T>
{
    private Node<T> _root;
    private readonly IComparer<T> _comparer;

    public DynamicSortedList(IComparer<T> comparer, T root)
    {
        _comparer = comparer;
        _root = new Node<T>(root);
        Count = 1;
    }

    public int Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Pop()
    {
        if (Count <= 0) return default(T);
        Node<T> temp = null;
        temp = _root;
        _root = temp.Next;
        Count--;
        return temp.Value;

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(T item)
    {
        var current = _root;
        Node<T> previous = null;
        while (current != null)
        {
            if (_comparer.Compare(item, current.Value) < 0)
            {
                if (previous == null)
                {
                    _root = new Node<T>(item) { Next = _root };
                }
                else
                {
                    var node = new Node<T>(item) { Next = current };
                    previous.Next = node;
                }

                break;
            }

            previous = current;
            current = current.Next;
        }

        if (current == null)
        {
            previous.Next = new Node<T>(item);
        }

        Count++;
    }
}
