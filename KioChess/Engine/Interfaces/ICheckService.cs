namespace Engine.Interfaces
{
    public interface ICheckService : ICacheService
    {
        bool IsBlackCheck(ulong key, IBoard board);
        bool IsWhiteCheck(ulong key, IBoard board);
    }
}