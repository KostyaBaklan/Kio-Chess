using System.ServiceModel;

namespace StockFishCore.Services
{
    [ServiceContract]
    public interface IStockFishService
    {
        [OperationContract]
        void ProcessResult(string stockFishResult);

        [OperationContract]
        void Save();
    }
}