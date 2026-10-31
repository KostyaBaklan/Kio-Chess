using Engine.Models.Config;
using Engine.Pgn.Streaming;
using System.Diagnostics;
using System.Text;

namespace NnueTool;

/// <summary>Pass 1: tag-only filter of a decompressed Lichess dump. Kept games are written unchanged.</summary>
internal static class FilterCommand
{
    public static void Run(string input, string output, NnueFilterConfiguration filter)
    {
        var terminations = new HashSet<string>(filter.AllowedTerminations ?? [], StringComparer.OrdinalIgnoreCase);
        long total = 0, kept = 0;
        var stopwatch = Stopwatch.StartNew();

        using var reader = new PgnStreamReader(input);
        using var writer = new StreamWriter(output, false, new UTF8Encoding(false), 1 << 20);

        foreach (var text in reader.ReadGameText())
        {
            total++;
            if (Keep(text, filter, terminations))
            {
                writer.Write(text);
                writer.WriteLine();
                kept++;
            }

            if (total % 1_000_000 == 0)
            {
                Console.WriteLine($"{stopwatch.Elapsed:hh\\:mm\\:ss} {reader.ProgressPercent:F2}% games={total:N0} kept={kept:N0}");
            }
        }

        Console.WriteLine($"Filter done in {stopwatch.Elapsed:hh\\:mm\\:ss}: games={total:N0} kept={kept:N0} -> {output}");
    }

    private static bool Keep(string text, NnueFilterConfiguration filter, HashSet<string> terminations)
    {
        if (filter.ExcludeBots && (GetTag(text, "WhiteTitle") == "BOT" || GetTag(text, "BlackTitle") == "BOT"))
            return false;

        if (!int.TryParse(GetTag(text, "WhiteElo"), out var whiteElo) || whiteElo < filter.MinElo)
            return false;
        if (!int.TryParse(GetTag(text, "BlackElo"), out var blackElo) || blackElo < filter.MinElo)
            return false;

        if (GetTag(text, "Result") is not ("1-0" or "0-1" or "1/2-1/2"))
            return false;

        if (terminations.Count > 0 && !terminations.Contains(GetTag(text, "Termination") ?? string.Empty))
            return false;

        var timeControl = GetTag(text, "TimeControl");
        int plus = timeControl?.IndexOf('+') ?? -1;
        return plus > 0
            && int.TryParse(timeControl.AsSpan(0, plus), out var baseSeconds)
            && baseSeconds >= filter.MinBaseTimeSeconds;
    }

    private static string GetTag(string text, string name)
    {
        var prefix = $"[{name} \"";
        int start = text.IndexOf(prefix, StringComparison.Ordinal);
        if (start < 0)
            return null;

        start += prefix.Length;
        int end = text.IndexOf('"', start);
        return end < 0 ? null : text[start..end];
    }
}
