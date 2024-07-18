using Engine.Interfaces.Config;

namespace Engine.Models.Config;

public class GeneralConfiguration : IGeneralConfiguration
{
    #region Implementation of IGeneralConfiguration

    public int GameDepth { get; set; }
    public int MaxMoveCount { get; set; }
    public double BlockTimeout { get; set; }
    public int FutilityDepth { get; set; }

    public string Strategy { get; set; }

    #endregion
}