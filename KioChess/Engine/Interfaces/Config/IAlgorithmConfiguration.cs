using Engine.Models.Config;

namespace Engine.Interfaces.Config;

public interface IAlgorithmConfiguration
{
    int DepthOffset { get; }
    int DepthReduction { get; }
    ExtensionConfiguration ExtensionConfiguration { get; }
    IterativeDeepingConfiguration IterativeDeepingConfiguration { get; }
    AspirationConfiguration AspirationConfiguration { get; }
    NullConfiguration NullConfiguration { get; }
    LateMoveConfiguration LateMoveConfiguration { get; }
    PvsConfiguration PvsConfiguration { get; }
    SortingConfiguration SortingConfiguration { get; }
    MarginConfiguration MarginConfiguration { get; }
}