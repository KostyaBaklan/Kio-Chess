using Engine.Interfaces.Config;

namespace Engine.Models.Config;

public class AlgorithmConfiguration : IAlgorithmConfiguration
{
    #region Implementation of IAlgorithmConfiguration

    public int DepthOffset { get; set; }
    public int DepthReduction { get; set; }
    public ExtensionConfiguration ExtensionConfiguration { get; set; }
    public IterativeDeepingConfiguration IterativeDeepingConfiguration { get; set; }
    public AspirationConfiguration AspirationConfiguration { get; set; }
    public NullConfiguration NullConfiguration { get; set; }
    public LateMoveConfiguration LateMoveConfiguration { get; set; }
    public SortingConfiguration SortingConfiguration { get; set; }

    public MarginConfiguration MarginConfiguration { get; set; }

    #endregion
}