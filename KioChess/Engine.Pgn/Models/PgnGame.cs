namespace Engine.Pgn.Models;

public class PgnGame
{
    public Dictionary<string, string> Tags { get; set; }
    public List<PgnMove> Moves { get; set; }
    public GameResult Result { get; set; }
    public string ResultString { get; set; }

    public PgnGame()
    {
        Tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Moves = new List<PgnMove>();
        Result = GameResult.None;
        ResultString = "*";
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
}
