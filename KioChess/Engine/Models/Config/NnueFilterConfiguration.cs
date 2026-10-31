namespace Engine.Models.Config;

public class NnueFilterConfiguration
{
    public int MinElo { get; set; }
    public int MinBaseTimeSeconds { get; set; }
    public bool ExcludeBots { get; set; }
    public string[] AllowedTerminations { get; set; }
}
