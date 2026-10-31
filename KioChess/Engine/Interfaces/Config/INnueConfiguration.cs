using Engine.Models.Config;

namespace Engine.Interfaces.Config;

public interface INnueConfiguration
{
    bool IsEnabled { get; }
    string NetworkPath { get; }
    NnueNetworkConfiguration Network { get; }
    NnueDataConfiguration Data { get; }
    NnueTrainingConfiguration Training { get; }
}
