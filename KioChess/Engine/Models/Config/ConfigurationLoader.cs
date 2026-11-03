using Newtonsoft.Json;

namespace Engine.Models.Config;

public static class ConfigurationLoader
{
    public const string ConfigurationFile = @"Config\Configuration.json";
    public const string EvaluationFile = @"Config\EvaluationConfiguration.json";

    public static Configuration Load()
    {
        var configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(ConfigurationFile));
        configuration.Evaluation = JsonConvert.DeserializeObject<Evaluation>(File.ReadAllText(EvaluationFile));
        return configuration;
    }
}
