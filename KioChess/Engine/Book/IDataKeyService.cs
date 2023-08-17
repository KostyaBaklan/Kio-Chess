namespace Engine.Book
{
    public interface IDataKeyService
    {
        void Add(short key);
        void Delete();
        string Get();

        void Reset();
    }
}
