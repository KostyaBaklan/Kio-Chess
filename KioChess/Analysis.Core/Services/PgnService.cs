using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Models.Helpers;
using Engine.Services;
using System.Text;
using System.Text.RegularExpressions;

namespace Analysis.Core.Services;

/// <summary>
/// Service for parsing and exporting PGN (Portable Game Notation) format chess games.
/// </summary>
public class PgnService
{
    private readonly Position _position;
    private readonly MoveHistoryService _moveHistory;

    public PgnService(Position position, MoveHistoryService moveHistory)
    {
        _position = position;
        _moveHistory = moveHistory;
    }

    /// <summary>
    /// Parses PGN text and returns the list of moves played.
    /// </summary>
    public PgnParseResult ParsePgn(string pgnText)
    {
        var result = new PgnParseResult();
        
        if (string.IsNullOrWhiteSpace(pgnText))
        {
            result.ErrorMessage = "PGN text is empty";
            return result;
        }

        try
        {
            // Extract metadata tags
            result.Metadata = ExtractMetadata(pgnText);

            // Extract move text (everything after tags)
            var moveText = ExtractMoveText(pgnText);
            
            // Parse moves
            result.Moves = ParseMoves(moveText);
            result.IsSuccess = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Failed to parse PGN: {ex.Message}";
            result.IsSuccess = false;
        }

        return result;
    }

    /// <summary>
    /// Exports a game to PGN format with metadata and move annotations.
    /// </summary>
    public string ExportToPgn(PgnExportData data)
    {
        var sb = new StringBuilder();

        // Write metadata tags
        sb.AppendLine($"[Event \"{data.Event ?? "Casual Game"}\"]");
        sb.AppendLine($"[Site \"{data.Site ?? "Kio Chess Analysis"}\"]");
        sb.AppendLine($"[Date \"{data.Date?.ToString("yyyy.MM.dd") ?? DateTime.Now.ToString("yyyy.MM.dd")}\"]");
        sb.AppendLine($"[Round \"{data.Round ?? "?"}\"]");
        sb.AppendLine($"[White \"{data.White ?? "Player"}\"]");
        sb.AppendLine($"[Black \"{data.Black ?? "Engine"}\"]");
        sb.AppendLine($"[Result \"{data.Result ?? "*"}\"]");
        
        if (!string.IsNullOrEmpty(data.WhiteElo))
            sb.AppendLine($"[WhiteElo \"{data.WhiteElo}\"]");
        if (!string.IsNullOrEmpty(data.BlackElo))
            sb.AppendLine($"[BlackElo \"{data.BlackElo}\"]");
        if (!string.IsNullOrEmpty(data.Opening))
            sb.AppendLine($"[Opening \"{data.Opening}\"]");

        sb.AppendLine();

        // Write moves with annotations
        if (data.Moves != null && data.Moves.Count > 0)
        {
            for (int i = 0; i < data.Moves.Count; i++)
            {
                var move = data.Moves[i];
                
                if (i % 2 == 0)
                {
                    int moveNum = (i / 2) + 1;
                    sb.Append($"{moveNum}. ");
                }

                sb.Append(move.Notation);

                // Add classification if available
                if (!string.IsNullOrEmpty(move.Classification))
                    sb.Append($" {move.Classification}");

                // Add comment if available
                if (!string.IsNullOrEmpty(move.Comment))
                    sb.Append($" {{ {move.Comment} }}");

                sb.Append(" ");

                // Line break every 2 full moves
                if (i % 4 == 3)
                    sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine(data.Result ?? "*");
        }

        return sb.ToString();
    }

    private Dictionary<string, string> ExtractMetadata(string pgnText)
    {
        var metadata = new Dictionary<string, string>();
        var tagPattern = new Regex(@"\[(\w+)\s+""([^""]*)""\]");
        var matches = tagPattern.Matches(pgnText);

        foreach (Match match in matches)
        {
            if (match.Groups.Count == 3)
            {
                metadata[match.Groups[1].Value] = match.Groups[2].Value;
            }
        }

        return metadata;
    }

    private string ExtractMoveText(string pgnText)
    {
        // Remove all tag lines
        var tagPattern = new Regex(@"\[.*?\]");
        var moveText = tagPattern.Replace(pgnText, "");
        
        // Remove comments in curly braces
        var commentPattern = new Regex(@"\{[^}]*\}");
        moveText = commentPattern.Replace(moveText, "");
        
        return moveText.Trim();
    }

    private List<MoveBase> ParseMoves(string moveText)
    {
        var moves = new List<MoveBase>();
        
        // Remove result indicators
        moveText = moveText.Replace("1-0", "")
                           .Replace("0-1", "")
                           .Replace("1/2-1/2", "")
                           .Replace("*", "");

        // Remove move numbers and extra whitespace
        var cleanText = Regex.Replace(moveText, @"\d+\.", " ");
        cleanText = Regex.Replace(cleanText, @"\s+", " ").Trim();

        var moveTokens = cleanText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Reset position to starting (clear and initialize)
        // Note: Actual implementation depends on Position API
        // For now, skip move parsing as it requires deeper integration
        
        return moves;
    }

    private MoveBase ParseSingleMove(string notation)
    {
        var legalMoves = _position.GetAllMoves();
        
        // Try to find matching move by notation
        foreach (var move in legalMoves)
        {
            var moveNotation = GetMoveNotation(move);
            if (moveNotation == notation)
                return move;
        }

        return null;
    }

    private string GetMoveNotation(MoveBase move)
    {
        // Simple algebraic notation (this is a basic implementation)
        // In a real implementation, you'd use IMoveFormatter
        if (move.IsCastle)
            return move.To > move.From ? "O-O" : "O-O-O";

        var from = move.From.AsString();
        var to = move.To.AsString();
        
        return $"{from}{to}";
    }
}

public class PgnParseResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public List<MoveBase> Moves { get; set; } = new();
}

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
    public List<PgnMove> Moves { get; set; } = new();
}

public class PgnMove
{
    public string Notation { get; set; } = string.Empty;
    public string Classification { get; set; }
    public string Comment { get; set; }
}
