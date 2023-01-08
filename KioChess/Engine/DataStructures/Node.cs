namespace Engine.DataStructures
{
    class Node<T>
    {
        public Node(T value)
        {
            Value = value;
        }

        public T Value;
        public Node<T> Next;
    }
}