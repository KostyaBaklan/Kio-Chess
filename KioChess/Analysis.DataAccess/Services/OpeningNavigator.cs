using Analysis.DataAccess.Interfaces;
using Analysis.DataAccess.Models;

namespace Analysis.DataAccess.Services;

/// <summary>
/// Provides tree-based navigation through chess openings.
/// Tracks current position and allows forward/backward movement through opening theory.
/// Uses order-independent hash-based queries for fast position lookup.
/// </summary>
public class OpeningNavigator
{
    private readonly IOpeningExplorerService _explorerService;
    private readonly List<short> _currentMoveKeys = new();
    private readonly Stack<OpeningNode> _navigationStack = new();
    private OpeningNode _currentNode;
    private OpeningNode _lastKnownNode;

    public OpeningNavigator(IOpeningExplorerService explorerService)
    {
        _explorerService = explorerService;
    }

    /// <summary>Current opening at this position (null if out of book)</summary>
    public OpeningNode CurrentNode => _currentNode;

    /// <summary>Last known opening before going out of book</summary>
    public OpeningNode LastKnownNode => _lastKnownNode;

    /// <summary>Current opening name or "Unknown Opening"</summary>
    public string CurrentOpeningName => _currentNode?.FullName ?? "Unknown Opening";

    /// <summary>Current ECO code or empty</summary>
    public string ECOCode => _currentNode?.ECO ?? string.Empty;

    /// <summary>Move history as SAN notation</summary>
    public string MoveHistorySAN => _currentNode?.MovesSAN ?? string.Empty;

    /// <summary>Current depth in opening (number of moves)</summary>
    public int Depth => _currentMoveKeys.Count;

    /// <summary>Are we still in known opening theory?</summary>
    public bool IsInBook => _currentNode != null;

    /// <summary>
    /// Move forward in the opening book by playing a move.
    /// Uses order-independent hash-based queries.
    /// </summary>
    /// <param name="moveKey">Move key extracted from Position's move history</param>
    /// <returns>True if move leads to a known opening position</returns>
    public async Task<bool> MoveNextAsync(short moveKey)
    {
        _currentMoveKeys.Add(moveKey);

        // Query using order-independent move key hash
        var opening = await _explorerService.GetOpeningByMoveKeysAsync(_currentMoveKeys.ToArray());

        // Update navigation stack
        if (_currentNode != null)
        {
            _navigationStack.Push(_currentNode);
        }

        _currentNode = opening != null ? OpeningNode.FromEntity(opening) : null;

        // Track last known opening for when we go out of book
        if (_currentNode != null)
        {
            _lastKnownNode = _currentNode;

            // Load possible next moves - only get variations at next depth (current + 1)
            var variations = await _explorerService.GetVariationsAsync(_currentNode.Id, _currentMoveKeys.Count + 1);
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
        if (_currentMoveKeys.Count == 0) return false;

        _currentMoveKeys.RemoveAt(_currentMoveKeys.Count - 1);

        // Pop from navigation stack
        if (_navigationStack.Count > 0)
        {
            _currentNode = _navigationStack.Pop();

            if (_currentNode != null)
            {
                _lastKnownNode = _currentNode;

                // Load possible next moves - only get variations at next depth
                var variations = await _explorerService.GetVariationsAsync(_currentNode.Id, _currentMoveKeys.Count + 1);
                _currentNode.NextMoves = variations.Select(OpeningNode.FromEntity).ToList();
            }
        }
        else
        {
            // Back to starting position
            _currentNode = null;
            _lastKnownNode = null;
        }

        return true;
    }

    /// <summary>
    /// Reset to starting position.
    /// </summary>
    public void Reset()
    {
        _currentMoveKeys.Clear();
        _navigationStack.Clear();
        _currentNode = null;
        _lastKnownNode = null;
    }

    /// <summary>
    /// Update possible moves display - used to show next move options.
    /// </summary>
    public async Task<List<OpeningMove>> GetPossibleMovesAsync()
    {
        // Determine which node to use for getting variations
        // If we're out of book but have a last known opening, use that
        var nodeToQuery = _currentNode ?? _lastKnownNode;

        if (nodeToQuery == null)
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

        // Get variations from current position - only immediate next moves (depth + 1)
        var variations = await _explorerService.GetVariationsAsync(nodeToQuery.Id, _currentMoveKeys.Count + 1);

        return variations.Select(v =>
        {
            var nextMove = v.MovesUCI.Split(' ').Skip(_currentMoveKeys.Count).FirstOrDefault() ?? "";
            var nextMoveSAN = v.MovesSAN.Split(' ').Skip(_currentMoveKeys.Count).FirstOrDefault() ?? "";

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
}