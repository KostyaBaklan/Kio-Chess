using Analysis.DataAccess.Interfaces;
using Analysis.DataAccess.Models;

namespace Analysis.DataAccess.Services;

/// <summary>
/// Provides tree-based navigation through chess openings.
/// Tracks current position and allows forward/backward movement through opening theory.
/// </summary>
public class OpeningNavigator
{
    private readonly IOpeningExplorerService _explorerService;
    private readonly List<string> _currentMoves = new();
    private OpeningNode _currentNode;

    public OpeningNavigator(IOpeningExplorerService explorerService)
    {
        _explorerService = explorerService;
    }

    /// <summary>Current opening at this position (null if out of book)</summary>
    public OpeningNode CurrentNode => _currentNode;
    
    /// <summary>Current opening name or "Unknown Opening"</summary>
    public string CurrentOpeningName => _currentNode?.FullName ?? "Unknown Opening";
    
    /// <summary>Current ECO code or empty</summary>
    public string ECOCode => _currentNode?.ECO ?? string.Empty;
    
    /// <summary>Move history as SAN notation</summary>
    public string MoveHistorySAN => _currentNode?.MovesSAN ?? string.Empty;
    
    /// <summary>Current depth in opening (number of moves)</summary>
    public int Depth => _currentMoves.Count;
    
    /// <summary>Are we still in known opening theory?</summary>
    public bool IsInBook => _currentNode != null;

    /// <summary>
    /// Move forward in the opening book by playing a move.
    /// </summary>
    /// <param name="moveUCI">Move in UCI format (e.g., "e2e4")</param>
    /// <returns>True if move leads to a known opening position</returns>
    public async Task<bool> MoveNextAsync(string moveUCI)
    {
        _currentMoves.Add(moveUCI);
        var posKey = string.Join("_", _currentMoves);
        
        var opening = await _explorerService.GetOpeningByPositionAsync(posKey);
        _currentNode = opening != null ? OpeningNode.FromEntity(opening) : null;
        
        if (_currentNode != null)
        {
            // Load possible next moves
            var variations = await _explorerService.GetVariationsAsync(_currentNode.Id);
            _currentNode.NextMoves = variations.Select(OpeningNode.FromEntity).ToList();
        }
        
        return _currentNode != null;
    }

    /// <summary>
    /// Move backward in the opening book (undo last move).
    /// </summary>
    /// <returns>True if successfully moved back</returns>
    public async Task<bool> MovePreviousAsync()
    {
        if (_currentMoves.Count == 0) return false;
        
        _currentMoves.RemoveAt(_currentMoves.Count - 1);
        
        if (_currentMoves.Count == 0)
        {
            _currentNode = null;
            return true;
        }
        
        var posKey = string.Join("_", _currentMoves);
        var opening = await _explorerService.GetOpeningByPositionAsync(posKey);
        _currentNode = opening != null ? OpeningNode.FromEntity(opening) : null;
        
        if (_currentNode != null)
        {
            var variations = await _explorerService.GetVariationsAsync(_currentNode.Id);
            _currentNode.NextMoves = variations.Select(OpeningNode.FromEntity).ToList();
        }
        
        return true;
    }

    /// <summary>
    /// Reset to starting position.
    /// </summary>
    public void Reset()
    {
        _currentMoves.Clear();
        _currentNode = null;
    }

    /// <summary>
    /// Get all possible next moves from current position.
    /// </summary>
    public async Task<List<OpeningMove>> GetPossibleMovesAsync()
    {
        if (_currentNode == null)
        {
            // At root - get all first moves
            var roots = await _explorerService.GetRootOpeningsAsync();
            return roots.Select(r => new OpeningMove
            {
                MoveUCI = r.MovesUCI.Split(' ').FirstOrDefault() ?? "",
                MoveSAN = r.MovesSAN.Split(' ').FirstOrDefault() ?? "",
                OpeningName = r.FullName,
                ECO = r.ECO,
                Popularity = r.Popularity,
                IsMainLine = r.IsMainLine,
                OpeningId = r.Id
            }).ToList();
        }
        
        // Get variations from current position
        var variations = await _explorerService.GetVariationsAsync(_currentNode.Id);
        
        return variations.Select(v =>
        {
            var nextMove = v.MovesUCI.Split(' ').Skip(_currentMoves.Count).FirstOrDefault() ?? "";
            var nextMoveSAN = v.MovesSAN.Split(' ').Skip(_currentMoves.Count).FirstOrDefault() ?? "";
            
            return new OpeningMove
            {
                MoveUCI = nextMove,
                MoveSAN = nextMoveSAN,
                OpeningName = v.FullName,
                ECO = v.ECO,
                Popularity = v.Popularity,
                IsMainLine = v.IsMainLine,
                OpeningId = v.Id
            };
        }).ToList();
    }

    /// <summary>
    /// Synchronize navigator with external move list.
    /// </summary>
    public async Task SyncWithMovesAsync(IEnumerable<string> movesUCI)
    {
        Reset();
        
        foreach (var move in movesUCI)
        {
            await MoveNextAsync(move);
            if (!IsInBook) break; // Stop when out of book
        }
    }
}
