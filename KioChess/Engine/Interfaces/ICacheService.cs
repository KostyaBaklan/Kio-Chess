namespace Engine.Interfaces
{
    public interface ICacheService
    {
        int Size { get; }
        void Clear();
    }
}