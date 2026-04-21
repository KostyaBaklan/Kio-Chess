using Analysis.Core.Interfaces;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using System.Text.RegularExpressions;

namespace Analysis.Core.Services;

/// <summary>
/// Service for parsing PGN (Portable Game Notation) strings.
/// Creates a fresh Position instance for each parse operation to avoid conflicts.
/// </summary>
public class PgnParserService : IPgnParserService
{
    private readonly IMoveFormatter _moveFormatter;

    public PgnParserService(IMoveFormatter moveFormatter)
    {
        _moveFormatter = moveFormatter;
    }

    public List<MoveBase> ParseMoves(string pgn)
    {
        var moves = new List<MoveBase>();
        
        if (string.IsNullOrWhiteSpace(pgn))
            return moves;

        // Backup current Board reference to restore after parsing
        var previousBoard = MoveBase.Board;
        var position = new Position();

        try
        {
            // Create a fresh position for parsing
            position.Clear();

            // Remove headers (lines starting with [)
            var lines = pgn.Split('\n');
            var moveText = string.Join(" ", lines.Where(l => !l.TrimStart().StartsWith("[")));
            
            // Remove comments {}, variations (), result indicators
            moveText = Regex.Replace(moveText, @"\{[^}]*\}", " ");
            moveText = Regex.Replace(moveText, @"\([^)]*\)", " ");
            moveText = Regex.Replace(moveText, @"(1-0|0-1|1/2-1/2|\*)", " ");
            
            // Remove move numbers and dots
            moveText = Regex.Replace(moveText, @"\d+\.+", " ");
            
            // Remove NAG annotations ($1, $2, etc.)
            moveText = Regex.Replace(moveText, @"\$\d+", " ");
            
            // Remove annotation symbols (!, ?, !!, ??, !?, ?!)
            moveText = Regex.Replace(moveText, @"[!?]+", "");
            
            // Split into tokens
            var tokens = moveText.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            bool isFirstMove = true;
            foreach (var token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token))
                    continue;
                    
                // Get legal moves
                IEnumerable<MoveBase> legalMoves = isFirstMove 
                    ? position.GetFirstMoves().ToList() 
                    : position.GetAllMoves().ToList();
                
                // Try UCI format first (e.g., "e2e4", "e7e5")
                MoveBase move = null;
                if (token.Length >= 4 && token.Length <= 5)
                {
                    move = UciMoveConverter.FromUci(token, legalMoves);
                }
                
                // If UCI parsing failed, try SAN format
                if (move == null)
                {
                    move = FindMoveFromSan(token, legalMoves);
                }
                
                if (move != null)
                {
                    moves.Add(move);
                    
                    if (isFirstMove)
                    {
                        position.MakeFirst(move);
                        isFirstMove = false;
                    }
                    else
                    {
                        position.Make(move);
                    }
                }
            }
        }
        finally
        {
            // Restore the previous Board reference
            position.Clear();
            MoveBase.Board = previousBoard;
        }
        
        return moves;
    }

    public Dictionary<string, string> ParseHeaders(string pgn)
    {
        var headers = new Dictionary<string, string>();
        
        if (string.IsNullOrWhiteSpace(pgn))
            return headers;

        var headerRegex = new Regex(@"\[(\w+)\s+""([^""]*)""\]");
        var matches = headerRegex.Matches(pgn);
        
        foreach (Match match in matches)
        {
            if (match.Groups.Count >= 3)
            {
                headers[match.Groups[1].Value] = match.Groups[2].Value;
            }
        }
        
        return headers;
    }

    private MoveBase FindMoveFromSan(string san, IEnumerable<MoveBase> legalMoves)
    {
        // Clean up SAN notation
        san = san.Trim();
        if (string.IsNullOrEmpty(san))
            return null;

        // Try exact match first using move formatter
        foreach (var move in legalMoves)
        {
            var formatted = _moveFormatter.Format(move);
            
            // Compare without check/mate symbols
            var cleanFormatted = formatted.Replace("+", "").Replace("#", "").Trim();
            var cleanSan = san.Replace("+", "").Replace("#", "").Trim();
            
            if (string.Equals(cleanFormatted, cleanSan, StringComparison.OrdinalIgnoreCase))
                return move;
        }

        // Try matching by piece and destination
        foreach (var move in legalMoves)
        {
            if (MatchesSanPattern(san, move))
                return move;
        }

        return null;
    }

    private bool MatchesSanPattern(string san, MoveBase move)
    {
        san = san.Replace("+", "").Replace("#", "").Replace("x", "").Trim();
        
        // Handle castling
        if (san == "O-O" || san == "0-0")
            return move.IsCastle && move.To > move.From;
        if (san == "O-O-O" || san == "0-0-0")
            return move.IsCastle && move.To < move.From;

        // Get destination square from SAN (last 2 characters for piece moves)
        if (san.Length >= 2)
        {
            var destSquare = san.Substring(san.Length - 2, 2).ToLowerInvariant();
            
            // Handle promotion
            if (san.Length >= 3 && "QRBN".Contains(san[^1]))
            {
                destSquare = san.Substring(san.Length - 3, 2).ToLowerInvariant();
            }
            
            // Convert destination to index
            if (destSquare.Length == 2 && destSquare[0] >= 'a' && destSquare[0] <= 'h' 
                && destSquare[1] >= '1' && destSquare[1] <= '8')
            {
                int file = destSquare[0] - 'a';
                int rank = destSquare[1] - '1';
                int destIndex = rank * 8 + file;
                
                return move.To == destIndex;
            }
        }

        return false;
    }
}
