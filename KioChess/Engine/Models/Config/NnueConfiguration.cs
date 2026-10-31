using Engine.Interfaces.Config;

namespace Engine.Models.Config;

public class NnueConfiguration : INnueConfiguration
{
    public bool IsEnabled { get; set; }
    public string NetworkPath { get; set; }
    public NnueNetworkConfiguration Network { get; set; }
    public NnueDataConfiguration Data { get; set; }
    public NnueTrainingConfiguration Training { get; set; }
}
