using Engine.Interfaces.Config;
using Engine.Models.Enums;

namespace Engine.Services.Evaluation
{
    public class EvaluationServiceOpening : EvaluationServiceBase
    {
        public EvaluationServiceOpening(IConfigurationProvider configuration, IStaticValueProvider staticValueProvider)
            :base(configuration, staticValueProvider)
        {
            Initialize(configuration, staticValueProvider, Phase.Opening);
        }
    }
}
