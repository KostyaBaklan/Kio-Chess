namespace StockFishCore.Services
{
    public interface IStockFishService
    {
        void ProcessResult(string stockFishResult);

        void Save();
    }
}