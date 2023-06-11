namespace Engine.Interfaces
{
    public interface IEvaluationServiceFactory
    {
        IEvaluationService[] GetEvaluationServices();
        IEvaluationService GetEvaluationService(byte phase);
    }
}