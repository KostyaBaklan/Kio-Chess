using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class GeneralConfiguration : IGeneralConfiguration
    {
        #region Implementation of IGeneralConfiguration

        public bool UseEvaluationCache { get; set; }
        public int GameDepth { get; set; }
        public double BlockTimeout { get; set; }
        public bool UseFutility { get; set; }
        public int FutilityDepth { get; set; }
        public bool UseHistory { get; set; }
        public int KillerCapacity { get; set; }
        public bool UseAging { get; set; }

        #endregion
    }
}