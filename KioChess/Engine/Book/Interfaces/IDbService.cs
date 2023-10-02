namespace Engine.Book.Interfaces
{
    public interface IDbService
    {
        void Connect();
        void Disconnect();
        void Execute(string sql, int timeout = 30);
        IQueryable<T> Execute<T>(string sql);
    }
}
