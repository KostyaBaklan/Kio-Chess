using System.ServiceModel;

namespace StockFishCore.Services
{
    [ServiceContract]
    public interface IStockFishService
    {
        [OperationContract]
        void ProcessResult(StockFishResult stockFishResult);

        [OperationContract]
        void Save();
    }
}