using Analysis.Core.Models;

namespace Analysis.Core.Interfaces;

public interface IStockfishService : IDisposable
{
    /// <summary>True when Stockfish process is running and responded to the UCI handshake.</summary>
    bool IsReady { get; }

    /// <summary>
    /// Starts the Stockfish process from <paramref name="executablePath"/> and performs the UCI
    /// handshake (<c>uci</c> / <c>isready</c>). Throws <see cref="InvalidOperationException"/>
    /// if the process cannot be started.
    /// </summary>
    Task InitialiseAsync(string executablePath, CancellationToken ct = default);
    
    /// <summary>
    /// Configure Stockfish options (Hash, Threads, MultiPV, etc.).
    /// </summary>
    Task ConfigureAsync(StockfishOptions options, CancellationToken ct = default);

    /// <summary>
    /// Asks Stockfish for its best move from the given position sequence.
    /// <paramref name="moves"/> is the space-separated list of UCI moves from the start
    /// position, e.g. <c>"e2e4 e7e5 g1f3"</c>. Pass an empty string for the starting position.
    /// </summary>
    Task<string> GetBestMoveAsync(string moves, EloProfile profile,
                                   CancellationToken ct = default);

    /// <summary>
    /// Analyses a position at a given depth and returns the deepest <see cref="StockfishInfo"/>.
    /// </summary>
    Task<StockfishInfo> AnalysePositionAsync(string moves, int depth,
                                              CancellationToken ct = default);
    
    /// <summary>
    /// Analyzes a position and returns all MultiPV lines at the given depth.
    /// </summary>
    Task<IReadOnlyList<StockfishInfo>> AnalysePositionMultiPVAsync(string moves, int depth,
                                                                     CancellationToken ct = default);
    
    /// <summary>
    /// Evaluates a move by comparing it to the best move in the position.
    /// Returns detailed evaluation including classification (Brilliant, Good, Blunder, etc.).
    /// </summary>
    Task<MoveEvaluation> EvaluateMoveAsync(string movesBefore, string playedMove, 
                                            int depth = 18, CancellationToken ct = default);

    /// <summary>Sends <c>stop</c> to abort a running search.</summary>
    void Stop();
    
    /// <summary>Clears the position history, starting fresh.</summary>
    Task ClearHistoryAsync(CancellationToken ct = default);
}
