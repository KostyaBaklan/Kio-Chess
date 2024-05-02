using System.ServiceModel;

namespace StockFishCore
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