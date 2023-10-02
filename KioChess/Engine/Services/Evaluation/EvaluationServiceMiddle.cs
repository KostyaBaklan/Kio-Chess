using Engine.Interfaces.Config;
using Engine.Models.Enums;

namespace Engine.Services.Evaluation;

public class EvaluationServiceMiddle : EvaluationServiceBase
{
    public EvaluationServiceMiddle(IConfigurationProvider configuration, IStaticValueProvider staticValueProvider)
        : base(configuration, staticValueProvider)
    {
        Initialize(configuration, staticValueProvider, Phase.Middle);
    }
}
