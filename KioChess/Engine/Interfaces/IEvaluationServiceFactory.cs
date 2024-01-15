using Engine.Services.Evaluation;

namespace Engine.Interfaces;

public interface IEvaluationServiceFactory
{
    EvaluationServiceBase[] GetEvaluationServices();
    EvaluationServiceBase GetEvaluationService(byte phase);
}