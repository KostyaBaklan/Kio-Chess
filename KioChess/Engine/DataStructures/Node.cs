using System.Runtime.CompilerServices;

namespace Engine.DataStructures;

class Node<T>
{
    public Node(T value)
    {
        Value = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Node(T item, Node<T> root)
    {
        Value = item;
        Next = root;
    }

    public T Value;
    public Node<T> Next;
}