using Engine.Interfaces.Config;

namespace Engine.Models.Config;

public class BookConfiguration : IBookConfiguration
{
    public short GamesThreshold { get; set; }

    public short SearchDepth { get; set; }
    public short SaveDepth { get; set; }
    public short Elo { get; set; }
    public int Chunk { get; set; }

    public int PopularThreshold { get; set; }
    public int MinimumPopular { get; set; }
    public int MinimumPopularThreshold { get; set; }
    public int MaximumPopularThreshold { get; set; }
    public int PopularDepth { get; set; }
}