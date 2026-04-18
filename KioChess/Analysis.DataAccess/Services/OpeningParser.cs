using Analysis.DataAccess.Entities;
using Analysis.DataAccess.Services;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Analysis.DataAccess.Services;

/// <summary>
/// Parses chess opening data from various formats (TSV, PGN) and converts to database entities.
/// Handles SAN to UCI conversion using the chess engine.
/// This parser is stateless and requires a Position to be passed in for parsing.
/// </summary>
public class OpeningParser
{
    /// <summary>
    /// Parse a single TSV line (Lichess format: ECO, Name, PGN)
    /// </summary>
    /// <param name="line">The TSV line to parse</param>
    /// <param name="lineNumber">Line number for error tracking</param>
    /// <param name="position">Position instance to use for move validation and conversion</param>
    public OpeningEntry ParseTSVLine(string line, int lineNumber, Position position)
    {
        var parts = line.Split('\t');
        if (parts.Length < 3) 
        {
            return null;
        }

        var eco = parts[0].Trim();
        var name = parts[1].Trim();
        var san = parts[2].Trim();

        try
        {
            var (uciMoves, sanMoves, moveCount, moveKeys) = ConvertSANToUCI(san, position);
            if (string.IsNullOrEmpty(uciMoves)) 
            {
                return null;
            }

            var (openingName, variation, subVar) = ParseFullName(name);

            // Compute sequence hash from move keys
            var sequenceHash = SequenceHashHelper.ComputeSequenceHash(moveKeys);

            return new OpeningEntry
            {
                ECO = eco,
                Name = openingName,
                Variation = variation,
                SubVariation = subVar,
                FullName = name,
                MovesUCI = uciMoves,
                MovesSAN = sanMoves,
                MoveCount = moveCount,
                MoveKeys = moveKeys.ToArray(),
                SequenceHash = sequenceHash,
                Popularity = CalculatePopularity(eco, moveCount),
                IsMainLine = IsMainLineOpening(name)
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Convert SAN moves to UCI using the chess engine.
    /// Returns (UCI moves, cleaned SAN, move count, move keys list)
    /// Carefully manages board state - saves and restores board reference after use.
    /// </summary>
    private (string uci, string san, int count, List<short> keys) ConvertSANToUCI(string sanMoves, Position position)
    {
        // Save board state in case it's being used elsewhere (MoveBase.Board = this)
        var savedBoard = MoveBase.Board;

        try
        {
            position.Clear();

            var uciList = new List<string>();
            var sanList = new List<string>();
            var moveKeyList = new List<short>();

            // Parse SAN notation
            var tokens = sanMoves.Split([' ', '.'], StringSplitOptions.RemoveEmptyEntries)
                .Where(t => !int.TryParse(t, out _) && t != "*")
                .ToList();

            foreach (var token in tokens)
            {
                try
                {
                    // Clean the SAN move
                    var cleanSan = token.Replace("+", "").Replace("#", "");

                    // Get legal moves
                    var legalMoves = position.GetAllMoves().ToList();

                    // Find matching move
                    var move = FindMoveFromSAN(cleanSan, legalMoves);

                    if (move == null) 
                    {
                        break;
                    }

                    // Convert to UCI and add (using built-in ToUciString method)
                    var uci = move.ToUciString();
                    uciList.Add(uci);
                    sanList.Add(cleanSan);
                    moveKeyList.Add(move.Key);  // ? Extract move key

                    // Make the move to advance position
                    if (uciList.Count == 1)
                        position.MakeFirst(move);
                    else
                        position.Make(move);
                }
                catch (Exception)
                {
                    break;
                }
            }

            var result = (string.Join(" ", uciList), string.Join(" ", sanList), uciList.Count, moveKeyList);
            return result;
        }
        finally
        {
            // Restore board state for position/move operations
            MoveBase.Board = savedBoard;
        }
    }

    private MoveBase FindMoveFromSAN(string san, List<MoveBase> legalMoves)
    {
        // Handle castling
        if (san == "O-O" || san == "00")
            return legalMoves.FirstOrDefault(m => m.IsCastle && m.To > m.From);
        if (san == "O-O-O" || san == "000")
            return legalMoves.FirstOrDefault(m => m.IsCastle && m.To < m.From);

        // Extract components from SAN
        var to = ExtractDestinationSquare(san);
        if (to == null) return null;

        var piece = ExtractPieceType(san);
        var fromFile = ExtractFromFile(san);
        var fromRank = ExtractFromRank(san);

        // Find matching move
        foreach (var move in legalMoves)
        {
            if (!string.Equals(move.To.AsString(), to, StringComparison.OrdinalIgnoreCase)) continue;
            
            // Check piece type matches
            if (piece != null)
            {
                // Piece move specified (e.g., "Nf3", "Bc4")
                if (!PieceMatches(move, piece.Value)) continue;
            }
            else
            {
                // Pawn move (e.g., "e4", "exd5")
                // Verify the moving piece is actually a pawn (type 0)
                var pieceType = move.Piece % 6;
                if (pieceType != 0) continue; // Not a pawn, skip
            }
            
            // Compare file: convert char to index (a=0, b=1, ..., h=7)
            if (fromFile != null && (move.From % 8) != (fromFile.Value - 'a')) continue;
            
            // Compare rank: already an int (1-8), convert to index (0-7)
            if (fromRank != null && (move.From / 8) != (fromRank.Value - 1)) continue;

            return move;
        }

        return null;
    }

    private string ExtractDestinationSquare(string san)
    {
        // Last 2 characters are usually the destination (unless promotion)
        // Format: lowercase file (a-h) + digit rank (1-8)
        // Examples: e4, Nf3, exd4, Rad1, e8=Q
        
        if (san.Length < 2) return null;
        
        // Search backwards for pattern: letter(a-h) followed by digit(1-8)
        for (int i = san.Length - 1; i >= 1; i--)
        {
            char current = san[i];
            char previous = san[i - 1];
            
            // Check for: file(a-h) + rank(1-8)
            if (previous >= 'a' && previous <= 'h' && current >= '1' && current <= '8')
            {
                return $"{previous}{current}";
            }
        }
        
        return null;
    }

    private char? ExtractPieceType(string san)
    {
        if (char.IsUpper(san[0]) && san[0] != 'O')
            return san[0]; // K, Q, R, B, N
        return null; // Pawn move
    }

    private char? ExtractFromFile(string san)
    {
        // Look for disambiguation file in various formats:
        // - Piece moves: "Nbd7", "Rad1" (piece + file + destination)
        // - Pawn captures: "exd4" (file + x + destination)
        
        if (san.Length < 2) return null;
        
        // Check second character for file disambiguation (Nbd7, Rad1)
        // Must have length > 3 because format is: Piece + disambiguation + destination (2 chars)
        // Also verify san[2] is a letter (destination file), not a digit
        if (san.Length > 3 && char.IsUpper(san[0]) && san[1] >= 'a' && san[1] <= 'h' && san[2] >= 'a' && san[2] <= 'h')
            return san[1];
        
        // Check first character for pawn captures (exd4, axb5)
        if (san[0] >= 'a' && san[0] <= 'h' && san.Contains('x'))
            return san[0];
            
        return null;
    }

    private int? ExtractFromRank(string san)
    {
        // Look for disambiguation rank (e.g., "R1e1", "N3f5")
        if (san.Length > 2 && char.IsDigit(san[1]))
            return san[1] - '0';
        return null;
    }

    private bool PieceMatches(MoveBase move, char pieceChar)
    {
        // Get piece type without color
        // White pieces: 0-5, Black pieces: 6-11
        // Use modulo 6 to normalize to 0-5 range
        var pieceType = move.Piece % 6;
        
        return pieceChar switch
        {
            'K' => pieceType == 5, // King = 5 (WhiteKing) or 11 (BlackKing) % 6 = 5
            'Q' => pieceType == 4, // Queen = 4 or 10 % 6 = 4
            'R' => pieceType == 3, // Rook = 3 or 9 % 6 = 3
            'B' => pieceType == 2, // Bishop = 2 or 8 % 6 = 2
            'N' => pieceType == 1, // Knight = 1 or 7 % 6 = 1
            _ => false
        };
    }

    private (string name, string variation, string subVariation) ParseFullName(string fullName)
    {
        var parts = fullName.Split(':', StringSplitOptions.TrimEntries);
        
        if (parts.Length == 1)
            return (parts[0], null, null);
        
        if (parts.Length == 2)
            return (parts[0], parts[1], null);
        
        return (parts[0], parts[1], string.Join(": ", parts.Skip(2)));
    }

    private int CalculatePopularity(string eco, int moveCount)
    {
        var firstChar = eco.Length > 0 ? eco[0] : 'Z';
        
        int baseScore = firstChar switch
        {
            'B' => 80, // Sicilian, French, Caro-Kann
            'C' => 75, // Spanish, Italian, Scotch
            'D' => 70, // Queen's Gambit, Indian
            'E' => 60, // Indian Defenses
            'A' => 40, // Rare openings
            _ => 20
        };
        
        int depthPenalty = Math.Max(0, (moveCount - 10) * 2);
        
        return Math.Clamp(baseScore - depthPenalty, 0, 100);
    }

    private bool IsMainLineOpening(string name)
    {
        return !name.Contains(':') || 
               name.Contains("Classical") || 
               name.Contains("Main Line") ||
               name.Contains("Modern") ||
               name.Contains("Accepted") ||
               name.Contains("Defense");
    }
}
