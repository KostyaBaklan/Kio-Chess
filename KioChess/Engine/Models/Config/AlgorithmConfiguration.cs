using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class AlgorithmConfiguration : IAlgorithmConfiguration
    {
        #region Implementation of IAlgorithmConfiguration

        public int DepthOffset { get; set; }
        public int DepthReduction { get; set; }
        public int[] ExtensionDepthDifference { get; set; }
        public int[] EndExtensionDepthDifference { get; set; }
        public IterativeDeepingConfiguration IterativeDeepingConfiguration { get; set; }
        public AspirationConfiguration AspirationConfiguration { get; set; }
        public NullConfiguration NullConfiguration { get; set; }
        public MultiCutConfiguration MultiCutConfiguration { get; set; }
        public LateMoveConfiguration LateMoveConfiguration { get; set; }
        public PvsConfiguration PvsConfiguration { get; set; }
        public SortingConfiguration SortingConfiguration { get; set; }

        public SubSearchConfiguration SubSearchConfiguration { get; set; }

        public MarginConfiguration MarginConfiguration { get; set; }

        #endregion
    }
}