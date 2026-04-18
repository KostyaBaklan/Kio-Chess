using Analysis.Core.Models;
using Analysis.Core.Services;
using Analysis.DataAccess.Interfaces;
using Engine.Models.Moves;

namespace Analysis.Core.Interfaces;

/// <summary>
/// Service for analyzing chess games and positions.
/// </summary>
public interface IAnalysisService
{
    /// <summary>
    /// Analyzes a single position at the specified depth.
    /// </summary>
    Task<PositionAnalysis> AnalysePositionAsync(string fen, int depth, CancellationToken ct = default);

    /// <summary>
    /// Analyzes an entire game move-by-move and returns a comprehensive report.
    /// </summary>
    Task<GameAnalysisReport> AnalyseGameAsync(
        List<MoveBase> moves,
        int depth = 18,
        IProgress<AnalysisProgress> progress = null,
        CancellationToken ct = default);
    
    /// <summary>
    /// Analyzes an entire game move-by-move with opening book detection.
    /// </summary>
    Task<GameAnalysisReport> AnalyseGameAsync(
        List<MoveBase> moves,
        IOpeningExplorerService openingExplorer,
        int depth = 18,
        IProgress<AnalysisProgress> progress = null,
        CancellationToken ct = default);
}
