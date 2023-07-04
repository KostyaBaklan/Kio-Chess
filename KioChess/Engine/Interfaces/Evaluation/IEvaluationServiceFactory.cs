namespace Engine.Interfaces.Evaluation
{
    public interface IEvaluationServiceFactory
    {
        IEvaluationService[] GetEvaluationServices();
        IEvaluationService GetEvaluationService(byte phase);
    }
}