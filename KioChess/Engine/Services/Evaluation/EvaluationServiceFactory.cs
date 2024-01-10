using System.Runtime.CompilerServices;
using Engine.Interfaces.Config;
using Engine.Interfaces.Evaluation;

namespace Engine.Services.Evaluation;

public class EvaluationServiceFactory : IEvaluationServiceFactory
{
    private readonly IEvaluationService[] _evaluationServices;
    public EvaluationServiceFactory(IConfigurationProvider configuration, IStaticValueProvider staticValueProvider)
    {
        _evaluationServices = new IEvaluationService[]
        {
            new EvaluationServiceOpening(configuration, staticValueProvider),
            new EvaluationServiceMiddle(configuration, staticValueProvider),
            new EvaluationServiceEnd(configuration, staticValueProvider)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEvaluationService GetEvaluationService(byte phase) => _evaluationServices[phase];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEvaluationService[] GetEvaluationServices() => _evaluationServices;
}
