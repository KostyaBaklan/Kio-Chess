using Engine.Interfaces.Config;
using Engine.Models.Enums;

namespace Engine.Services.Evaluation;

public class EvaluationServiceEnd : EvaluationServiceBase
{
    public EvaluationServiceEnd(IConfigurationProvider configuration, IStaticValueProvider staticValueProvider)
        : base(configuration, staticValueProvider)
    {
        Initialize(configuration, staticValueProvider, Phase.End);
    }
}
