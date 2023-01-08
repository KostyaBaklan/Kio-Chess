namespace Engine.DataStructures.Hash
{
    public interface IDynamicHash<T>
    {
        int Count { get; }
        void Add(ulong key, T item);
        T Get(ulong key);
        bool TryGet(ulong key, out T item);
        bool Remove(ulong key);
        void Clear();
    }
}