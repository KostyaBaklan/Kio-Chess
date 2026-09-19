namespace StockfishBenchmark.Models;

public class BenchmarkMoveRecord
{
    public int    MoveNumber      { get; set; }  // 1-based sequential ply
    public short  MoveKey         { get; set; }  // MoveBase.Key for fast lookup via MoveProvider
    public bool   IsEngineMove    { get; set; }  // false = Stockfish played this ply
    public string UciMove         { get; set; }  // e.g. "e2e4"
    public string MoveNotation    { get; set; }  // e.g. "[P e2e4]" (from ToLightString())
    public double DurationMs      { get; set; }  // elapsed ms; 0 for Stockfish plies
    public TimeSpan Duration      => TimeSpan.FromMilliseconds(DurationMs);  // display
    public double ProcessMemoryMB { get; set; }  // WorkingSet64 / 1M after the move
    public int    TtCount         { get; set; }  // strategy.TtCount; 0 for Stockfish plies
    public int    MaterialValue   { get; set; }  // Position.GetValue()
    public int    StaticValue     { get; set; }  // Position.GetStaticValue()
}
