using Engine.Interfaces.Config;
using Engine.Services.Evaluation;

namespace Engine.Interfaces;

public interface IEvaluationServiceFactory
{
    EvaluationServiceBase[] GetEvaluationServices();
    EvaluationServiceBase GetEvaluationService(byte phase);
    void Reload(IConfigurationProvider configuration, IStaticValueProvider staticValueProvider);
}