namespace Analysis.Core.Models;

/// <summary>
/// Represents one parsed <c>info depth … score cp … pv …</c> line from Stockfish.
/// </summary>
public sealed record StockfishInfo
{
    public int    Depth      { get; init; }
    public int    Centipawns { get; init; }
    public bool   IsMate     { get; init; }
    public int    MateIn     { get; init; }
    public string BestMove   { get; init; } = string.Empty;
    /// <summary>Full principal variation as a space-separated list of UCI moves.</summary>
    public string Pv         { get; init; } = string.Empty;
    public long   Nodes      { get; init; }
    public int    Time       { get; init; }
    /// <summary>Which PV line this is (1-based), used in MultiPV mode.</summary>
    public int    MultiPvIndex { get; init; } = 1;
}
