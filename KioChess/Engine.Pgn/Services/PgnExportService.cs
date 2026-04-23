using Engine.Pgn.Models;
using System.Text;

namespace Engine.Pgn.Services;

/// <summary>
/// Service for exporting games to PGN format with metadata and annotations.
/// Moved from Analysis.Core for better organization.
/// </summary>
public class PgnExportService
{
    /// <summary>
    /// Exports a game to PGN format with metadata tags and move annotations.
    /// </summary>
    public string ExportToPgn(PgnExportData data)
    {
        var sb = new StringBuilder();

        // Write metadata tags
        sb.AppendLine($"[Event \"{data.Event ?? "Casual Game"}\"]");
        sb.AppendLine($"[Site \"{data.Site ?? "Chess Analysis"}\"]");
        sb.AppendLine($"[Date \"{data.Date?.ToString("yyyy.MM.dd") ?? DateTime.Now.ToString("yyyy.MM.dd")}\"]");
        sb.AppendLine($"[Round \"{data.Round ?? "?"}\"]");
        sb.AppendLine($"[White \"{data.White ?? "White"}\"]");
        sb.AppendLine($"[Black \"{data.Black ?? "Black"}\"]");
        sb.AppendLine($"[Result \"{data.Result ?? "*"}\"]");
        
        if (!string.IsNullOrEmpty(data.WhiteElo))
            sb.AppendLine($"[WhiteElo \"{data.WhiteElo}\"]");
        if (!string.IsNullOrEmpty(data.BlackElo))
            sb.AppendLine($"[BlackElo \"{data.BlackElo}\"]");
        if (!string.IsNullOrEmpty(data.Opening))
            sb.AppendLine($"[Opening \"{data.Opening}\"]");
        if (!string.IsNullOrEmpty(data.ECO))
            sb.AppendLine($"[ECO \"{data.ECO}\"]");

        // Add custom tags
        if (data.CustomTags != null)
        {
            foreach (var tag in data.CustomTags)
            {
                sb.AppendLine($"[{tag.Key} \"{tag.Value}\"]");
            }
        }

        sb.AppendLine();

        // Write moves with annotations
        if (data.Moves != null && data.Moves.Count > 0)
        {
            for (int i = 0; i < data.Moves.Count; i++)
            {
                var move = data.Moves[i];
                
                // Add move number for white's moves
                if (i % 2 == 0)
                {
                    int moveNum = (i / 2) + 1;
                    sb.Append($"{moveNum}. ");
                }

                sb.Append(move.Notation);

                // Add NAG (Numeric Annotation Glyph) if available
                if (!string.IsNullOrEmpty(move.Classification))
                    sb.Append($" {move.Classification}");

                // Add comment if available
                if (!string.IsNullOrEmpty(move.Comment))
                    sb.Append($" {{ {move.Comment} }}");

                sb.Append(" ");

                // Line break every 2 full moves (4 half-moves) for readability
                if (i % 4 == 3)
                    sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine(data.Result ?? "*");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Creates PGN export data from a PgnGame object.
    /// Useful for re-exporting with modifications.
    /// </summary>
    public string ExportGame(PgnGame game)
    {
        var data = new PgnExportData
        {
            Event = game.GetTag("Event"),
            Site = game.GetTag("Site"),
            Date = ParseDate(game.GetTag("Date")),
            Round = game.GetTag("Round"),
            White = game.GetTag("White"),
            Black = game.GetTag("Black"),
            Result = game.ResultString ?? "*",
            WhiteElo = game.GetTag("WhiteElo"),
            BlackElo = game.GetTag("BlackElo"),
            Opening = game.GetTag("Opening"),
            ECO = game.GetTag("ECO"),
            CustomTags = game.Tags.Where(t => !IsStandardTag(t.Key))
                .ToDictionary(t => t.Key, t => t.Value),
            Moves = game.Moves.Select(m => new PgnMoveAnnotated
            {
                Notation = m.Notation
            }).ToList()
        };

        return ExportToPgn(data);
    }

    private bool IsStandardTag(string tagName)
    {
        var standardTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Event", "Site", "Date", "Round", "White", "Black", "Result",
            "WhiteElo", "BlackElo", "Opening", "ECO"
        };
        return standardTags.Contains(tagName);
    }

    private DateTime? ParseDate(string dateStr)
    {
        if (string.IsNullOrEmpty(dateStr) || dateStr == "????.??.??")
            return null;

        // PGN date format: "yyyy.MM.dd"
        var parts = dateStr.Split('.');
        if (parts.Length == 3 &&
            int.TryParse(parts[0], out var year) &&
            int.TryParse(parts[1], out var month) &&
            int.TryParse(parts[2], out var day))
        {
            try
            {
                return new DateTime(year, month, day);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }
}

/// <summary>
/// Data structure for PGN export.
/// </summary>
public class PgnExportData
{
    public string Event { get; set; }
    public string Site { get; set; }
    public DateTime? Date { get; set; }
    public string Round { get; set; }
    public string White { get; set; }
    public string Black { get; set; }
    public string Result { get; set; }
    public string WhiteElo { get; set; }
    public string BlackElo { get; set; }
    public string Opening { get; set; }
    public string ECO { get; set; }
    public Dictionary<string, string> CustomTags { get; set; }
    public List<PgnMoveAnnotated> Moves { get; set; } = new();
}

/// <summary>
/// PGN move with annotations (comments, classifications).
/// </summary>
public class PgnMoveAnnotated
{
    public string Notation { get; set; } = string.Empty;
    public string Classification { get; set; }  // NAG like "!!", "?", etc.
    public string Comment { get; set; }         // Comment in {}
}
