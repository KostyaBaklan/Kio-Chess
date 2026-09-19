using Engine.Models.Moves;

namespace Analysis.Core.Interfaces;

/// <summary>
/// Service for parsing PGN (Portable Game Notation) strings.
/// </summary>
public interface IPgnParserService
{
    /// <summary>
    /// Parses a PGN string and returns the list of moves.
    /// </summary>
    List<MoveBase> ParseMoves(string pgn);
    
    /// <summary>
    /// Extracts PGN headers from a PGN string.
    /// </summary>
    Dictionary<string, string> ParseHeaders(string pgn);
}
