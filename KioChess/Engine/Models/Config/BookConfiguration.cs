using Engine.Interfaces.Config;

namespace Engine.Models.Config;

public class BookConfiguration : IBookConfiguration
{
    public string DatabasePath { get; set; }

    public short GamesThreshold { get; set; }

    public short SearchDepth { get; set; }
    public short SaveDepth { get; set; }
    public int EloCount { get; set; }
    public string PgnPath { get; set; } = @"C:\Dev\PGN";
    public string FilteredPgnFile { get; set; }
    public PgnFilterRule[] PgnFilterRules { get; set; } = [];
    public int Chunk { get; set; }

    public int PopularThreshold { get; set; }
    public int MinimumPopular { get; set; }
    public int MinimumPopularThreshold { get; set; }
    public int MaximumPopularThreshold { get; set; }
    public int PopularDepth { get; set; }
}