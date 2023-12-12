using Engine.Interfaces.Config;
using Engine.Models.Enums;

namespace Engine.Services.Evaluation;

public class EvaluationServiceEnd : EvaluationServiceBase
{
    public EvaluationServiceEnd(IConfigurationProvider configuration, IStaticValueProvider staticValueProvider)
        : base(configuration)
    {
        Initialize(configuration, staticValueProvider, Phase.End);
    }
}
