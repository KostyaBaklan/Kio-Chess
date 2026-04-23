namespace Engine.Pgn.Parsers;

public static class TagParser
{
    public static bool TryParseTag(ReadOnlySpan<char> line, out string tagName, out string tagValue)
    {
        tagName = string.Empty;
        tagValue = string.Empty;

        if (line.IsEmpty || line[0] != '[')
        {
            return false;
        }

        int nameEnd = line.IndexOf(' ');
        if (nameEnd <= 1)
        {
            return false;
        }

        tagName = line.Slice(1, nameEnd - 1).ToString();

        int valueStart = line.IndexOf('"');
        if (valueStart < 0)
        {
            return false;
        }

        int valueEnd = line.LastIndexOf('"');
        if (valueEnd <= valueStart)
        {
            return false;
        }

        tagValue = line.Slice(valueStart + 1, valueEnd - valueStart - 1).ToString();
        return true;
    }

    public static bool IsTag(ReadOnlySpan<char> line)
    {
        return !line.IsEmpty && line[0] == '[';
    }
}
