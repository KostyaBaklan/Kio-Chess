using System.Runtime.CompilerServices;
using Engine.Interfaces;
using Engine.Interfaces.Config;

namespace Engine.Services.Evaluation;

public class EvaluationServiceFactory : IEvaluationServiceFactory
{
    private readonly EvaluationServiceBase[] _evaluationServices;
    public EvaluationServiceFactory(IConfigurationProvider configuration, IStaticValueProvider staticValueProvider)
    {
        _evaluationServices = new EvaluationServiceBase[]
        {
            new EvaluationServiceOpening(configuration, staticValueProvider),
            new EvaluationServiceMiddle(configuration, staticValueProvider),
            new EvaluationServiceEnd(configuration, staticValueProvider)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EvaluationServiceBase GetEvaluationService(byte phase) => _evaluationServices[phase];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EvaluationServiceBase[] GetEvaluationServices() => _evaluationServices;
}
