using Engine.Interfaces.Config;

namespace Engine.Models.Config;

public class BookConfiguration : IBookConfiguration
{
    public Dictionary<string,string> Connection { get; set; }
    public Dictionary<string, string> Connection1 { get; set; }
    public Dictionary<string, string> Connection2 { get; set; }
    public Dictionary<string, string> ConnectionT { get; set; }

    public short SuggestedThreshold { get; set; }

    public short NonSuggestedThreshold { get; set; }
    public short GamesThreshold { get; set; }

    public short SearchDepth { get; set; }
    public short SaveDepth { get; set; }
    public short Elo { get; set; }
    public int Chunk { get; set; }

    public bool UseBooking  { get; set; }
    public int PopularThreshold { get; set; }
}