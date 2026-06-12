namespace Engine.Pgn.Models;

/// <summary>
/// Lightweight game metadata without move parsing.
/// Used for fast filtering of large PGN files by tags (ELO, players, etc.)
/// </summary>
public class PgnGameInfo
{
    public Dictionary<string, string> Tags { get; set; }
    public long FilePosition { get; set; }

    public PgnGameInfo()
    {
        Tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        FilePosition = 0;
    }

    public string GetTag(string tagName, string defaultValue = "")
    {
        return Tags.TryGetValue(tagName, out var value) ? value : defaultValue;
    }

    public int GetIntTag(string tagName, int defaultValue = 0)
    {
        if (Tags.TryGetValue(tagName, out var value) && int.TryParse(value, out var result))
        {
            return result;
        }
        return defaultValue;
    }

    public int WhiteElo => GetIntTag("WhiteElo", 0);
    public int BlackElo => GetIntTag("BlackElo", 0);
    public int MinElo => Math.Min(WhiteElo, BlackElo);
    public int MaxElo => Math.Max(WhiteElo, BlackElo);

    public string White => GetTag("White", "?");
    public string Black => GetTag("Black", "?");
    public string Event => GetTag("Event", "?");
    public string Site => GetTag("Site", "?");
    public string Date => GetTag("Date", "????.??.??");
    public string Result => GetTag("Result", "*");
}
