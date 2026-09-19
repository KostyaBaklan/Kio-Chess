using Analysis.Core.Interfaces;
using Analysis.Core.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Analysis.Core.Services;

/// <summary>
/// Manages a long-lived Stockfish process and communicates via UCI over stdin/stdout.
/// Thread-safety: all Stockfish I/O is serialised through <see cref="_ioLock"/>.
/// Maintains position history for improved evaluation accuracy.
/// </summary>
public sealed class StockfishService : IStockfishService
{
    private Process _process;
    private StreamWriter _stdin;
    private StreamReader _stdout;
    private readonly SemaphoreSlim _ioLock = new(1, 1);
    private StockfishOptions _options = new();
    private string _currentPositionMoves = string.Empty;

    // ?? IStockfishService ????????????????????????????????????????

    public bool IsReady { get; private set; }

    public async Task InitialiseAsync(string executablePath, CancellationToken ct = default)
    {
        await _ioLock.WaitAsync(ct);
        try
        {
            ShutdownProcess();

            var psi = new ProcessStartInfo(executablePath)
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            _process = Process.Start(psi)
                ?? throw new InvalidOperationException(
                       $"Could not start Stockfish at '{executablePath}'.");

            _stdin = _process.StandardInput;
            _stdout = _process.StandardOutput;

            // UCI handshake
            await WriteAsync("uci");
            await WaitForAsync("uciok", timeoutMs: 5000, ct);

            await WriteAsync("isready");
            await WaitForAsync("readyok", timeoutMs: 5000, ct);

            // Apply default configuration
            await ConfigureInternalAsync(_options);

            IsReady = true;
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public async Task ConfigureAsync(StockfishOptions options, CancellationToken ct = default)
    {
        await _ioLock.WaitAsync(ct);
        try
        {
            EnsureReady();
            _options = options;
            await ConfigureInternalAsync(options);
        }
        finally
        {
            _ioLock.Release();
        }
    }

    private async Task ConfigureInternalAsync(StockfishOptions options)
    {
        await WriteAsync($"setoption name Hash value {options.HashSizeMB}");
        await WriteAsync($"setoption name Threads value {options.Threads}");
        await WriteAsync($"setoption name MultiPV value {options.MultiPV}");
    }

    public async Task ClearHistoryAsync(CancellationToken ct = default)
    {
        await _ioLock.WaitAsync(ct);
        try
        {
            EnsureReady();
            _currentPositionMoves = string.Empty;
            await WriteAsync("ucinewgame");
            await WriteAsync("isready");
            await WaitForAsync("readyok", timeoutMs: 2000, ct);
        }
        finally
        {
            _ioLock.Release();
        }
    }


    public async Task<string> GetBestMoveAsync(string moves, EloProfile profile,
                                                CancellationToken ct = default)
    {
        await _ioLock.WaitAsync(ct);
        try
        {
            EnsureReady();

            // Use UCI_LimitStrength + UCI_Elo for accurate ELO limiting (Stockfish 12+)
            // Stockfish's UCI_Elo range is 1320-3190
            if (profile.TargetElo >= 1320)
            {
                await WriteAsync("setoption name UCI_LimitStrength value true");
                await WriteAsync($"setoption name UCI_Elo value {profile.TargetElo}");
            }
            else
            {
                // For ELO < 1320, use minimum UCI_Elo with reduced movetime
                await WriteAsync("setoption name UCI_LimitStrength value true");
                await WriteAsync("setoption name UCI_Elo value 1320");
            }

            // Set position with history if enabled
            await SetPositionAsync(moves);

            // Use movetime for consistent strength (more reliable than depth)
            await WriteAsync($"go movetime {profile.ThinkTimeMs}");

            // Collect output until "bestmove"
            string bestMove = await ReadBestMoveAsync(ct);

            // Update position history
            if (_options.UsePositionHistory && !string.IsNullOrEmpty(bestMove))
            {
                _currentPositionMoves = string.IsNullOrWhiteSpace(moves)
                    ? bestMove
                    : $"{moves} {bestMove}";
            }

            return bestMove;
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public async Task<StockfishInfo> AnalysePositionAsync(string moves, int depth,
                                                           CancellationToken ct = default)
    {
        await _ioLock.WaitAsync(ct);
        try
        {
            EnsureReady();

            await WriteAsync("setoption name UCI_LimitStrength value false");
            await WriteAsync("setoption name Skill Level value 20");
            await WriteAsync("setoption name MultiPV value 1");

            await SetPositionAsync(moves);

            await WriteAsync($"go depth {depth}");

            StockfishInfo deepest = null;
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                var line = await _stdout!.ReadLineAsync(ct) ?? string.Empty;

                if (line.StartsWith("info depth", StringComparison.Ordinal))
                {
                    var parsed = ParseInfo(line);
                    if (parsed is not null &&
                        (deepest is null || parsed.Depth >= deepest.Depth))
                        deepest = parsed;
                }
                else if (line.StartsWith("bestmove", StringComparison.Ordinal))
                {
                    var bm = line.Split(' ');
                    var bmStr = bm.Length > 1 ? bm[1] : string.Empty;
                    return deepest is not null
                        ? deepest with { BestMove = bmStr }
                        : new StockfishInfo { BestMove = bmStr };
                }
            }
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public async Task<IReadOnlyList<StockfishInfo>> AnalysePositionMultiPVAsync(string moves, int depth,
                                                                                  CancellationToken ct = default)
    {
        await _ioLock.WaitAsync(ct);
        try
        {
            EnsureReady();

            await WriteAsync("setoption name UCI_LimitStrength value false");
            await WriteAsync("setoption name Skill Level value 20");
            await WriteAsync($"setoption name MultiPV value {_options.MultiPV}");

            await SetPositionAsync(moves);

            await WriteAsync($"go depth {depth}");

            var pvLines = new Dictionary<int, StockfishInfo>();
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                var line = await _stdout!.ReadLineAsync(ct) ?? string.Empty;

                if (line.StartsWith("info depth", StringComparison.Ordinal) && line.Contains("multipv"))
                {
                    var parsed = ParseInfo(line);
                    if (parsed is not null && parsed.Depth == depth)
                    {
                        pvLines[parsed.MultiPvIndex] = parsed;
                    }
                }
                else if (line.StartsWith("bestmove", StringComparison.Ordinal))
                {
                    var bm = line.Split(' ');
                    var bmStr = bm.Length > 1 ? bm[1] : string.Empty;

                    if (pvLines.Count > 0)
                    {
                        var firstPv = pvLines.GetValueOrDefault(1);
                        if (firstPv is not null)
                            pvLines[1] = firstPv with { BestMove = bmStr };
                    }

                    return pvLines.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToList();
                }
            }
        }
        finally
        {
            _ioLock.Release();
        }
    }

    public async Task<MoveEvaluation> EvaluateMoveAsync(string movesBefore, string playedMove,
                                                         int depth = 18, CancellationToken ct = default)
    {
        await _ioLock.WaitAsync(ct);
        try
        {
            EnsureReady();

            // Analyze position before the move with MultiPV=2 to get best move and alternatives
            await WriteAsync("setoption name UCI_LimitStrength value false");
            await WriteAsync("setoption name Skill Level value 20");
            await WriteAsync("setoption name MultiPV value 2");

            await SetPositionAsync(movesBefore);

            await WriteAsync($"go depth {depth}");

            var beforeAnalysis = new Dictionary<int, StockfishInfo>();
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                var line = await _stdout!.ReadLineAsync(ct) ?? string.Empty;

                if (line.StartsWith("info depth", StringComparison.Ordinal) && line.Contains("multipv"))
                {
                    var parsed = ParseInfo(line);
                    if (parsed is not null && parsed.Depth == depth)
                    {
                        beforeAnalysis[parsed.MultiPvIndex] = parsed;
                    }
                }
                else if (line.StartsWith("bestmove", StringComparison.Ordinal))
                {
                    break;
                }
            }

            if (beforeAnalysis.Count == 0)
            {
                return new MoveEvaluation
                {
                    PlayedMove = playedMove,
                    Classification = MoveClassification.None
                };
            }

            var bestLine = beforeAnalysis[1];
            int evalBefore = bestLine.Centipawns;
            string bestMove = bestLine.BestMove;

            // Analyze position after the played move
            string movesAfter = string.IsNullOrWhiteSpace(movesBefore)
                ? playedMove
                : $"{movesBefore} {playedMove}";

            await WriteAsync("setoption name MultiPV value 1");
            await SetPositionAsync(movesAfter);
            await WriteAsync($"go depth {depth}");

            StockfishInfo afterInfo = null;
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                var line = await _stdout!.ReadLineAsync(ct) ?? string.Empty;

                if (line.StartsWith("info depth", StringComparison.Ordinal))
                {
                    var parsed = ParseInfo(line);
                    if (parsed is not null && parsed.Depth == depth)
                    {
                        afterInfo = parsed;
                    }
                }
                else if (line.StartsWith("bestmove", StringComparison.Ordinal))
                {
                    break;
                }
            }

            int evalAfter = afterInfo?.Centipawns ?? 0;

            // Flip evaluation (opponent's turn)
            evalAfter = -evalAfter;

            var classification = ClassifyMove(evalBefore, evalAfter, playedMove == bestMove);

            // Extract PV sequence (limit to first 3-4 moves for display)
            string pvSequence = bestLine.Pv ?? string.Empty;
            if (!string.IsNullOrEmpty(pvSequence))
            {
                var moves = pvSequence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                pvSequence = string.Join(" ", moves.Take(4));
            }

            return new MoveEvaluation
            {
                EvalBefore = evalBefore,
                EvalAfter = evalAfter,
                BestMoveEval = evalBefore,
                BestMove = bestMove,
                BestMoveSequence = pvSequence,
                PlayedMove = playedMove,
                Classification = classification
            };
        }
        finally
        {
            _ioLock.Release();
        }
    }

    private static MoveClassification ClassifyMove(int bestEval, int actualEval, bool isBestMove)
    {
        if (isBestMove)
            return MoveClassification.Best;

        int loss = bestEval - actualEval;

        // Check for brilliant moves (sacrifice that improves position)
        if (actualEval > bestEval + 50 && actualEval > 100)
            return MoveClassification.Brilliant;

        if (loss <= 10)
            return MoveClassification.Best;
        if (loss <= 20)
            return MoveClassification.Excellent;
        if (loss <= 50)
            return MoveClassification.Good;
        if (loss <= 100)
            return MoveClassification.Inaccuracy;
        if (loss <= 300)
            return MoveClassification.Mistake;

        return MoveClassification.Blunder;
    }

    private async Task SetPositionAsync(string moves)
    {
        string posCmd = string.IsNullOrWhiteSpace(moves)
            ? "position startpos"
            : $"position startpos moves {moves}";
        await WriteAsync(posCmd);
    }

    public void Stop()
    {
        try { _stdin?.WriteLine("stop"); _stdin?.Flush(); }
        catch { /* ignored */ }
    }

    // ?? IDisposable ??????????????????????????????????????????????

    public void Dispose()
    {
        _ioLock.Wait(100);
        try
        {
            ShutdownProcess();
            IsReady = false;
        }
        finally
        {
            _ioLock.Release();
            _ioLock.Dispose();
        }
    }

    // ?? Private helpers ??????????????????????????????????????????

    private void EnsureReady()
    {
        if (!IsReady || _process is null || _process.HasExited)
            throw new InvalidOperationException(
                "Stockfish is not initialised. Call InitialiseAsync first.");
    }

    private async Task WriteAsync(string command)
    {
        await _stdin!.WriteLineAsync(command);
        await _stdin.FlushAsync();
    }

    private async Task WaitForAsync(string token, int timeoutMs, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeoutMs);

        while (true)
        {
            cts.Token.ThrowIfCancellationRequested();
            var line = await _stdout!.ReadLineAsync(cts.Token) ?? string.Empty;
            if (line.Contains(token, StringComparison.Ordinal))
                return;
        }
    }

    private async Task<string> ReadBestMoveAsync(CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var line = await _stdout!.ReadLineAsync(ct) ?? string.Empty;
            if (line.StartsWith("bestmove", StringComparison.Ordinal))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 1 ? parts[1] : string.Empty;
            }
        }
    }

    private void ShutdownProcess()
    {
        try
        {
            if (_process is not null && !_process.HasExited)
            {
                _stdin?.WriteLine("quit");
                _stdin?.Flush();
                _process.WaitForExit(2000);
                if (!_process.HasExited)
                    _process.Kill();
            }
        }
        catch { /* ignored */ }
        finally
        {
            _stdin?.Dispose();
            _stdin = null;
            _stdout?.Dispose();
            _stdout = null;
            _process?.Dispose();
            _process = null;
        }
    }

    // ?? UCI info line parser ?????????????????????????????????????

    private static readonly Regex _depthRe = new(@"\bdepth (\d+)", RegexOptions.Compiled);
    private static readonly Regex _cpRe = new(@"\bscore cp (-?\d+)", RegexOptions.Compiled);
    private static readonly Regex _mateRe = new(@"\bscore mate (-?\d+)", RegexOptions.Compiled);
    private static readonly Regex _pvRe = new(@"\bpv (.+)$", RegexOptions.Compiled);
    private static readonly Regex _nodesRe = new(@"\bnodes (\d+)", RegexOptions.Compiled);
    private static readonly Regex _timeRe = new(@"\btime (\d+)", RegexOptions.Compiled);
    private static readonly Regex _multiPvRe = new(@"\bmultipv (\d+)", RegexOptions.Compiled);

    private static StockfishInfo ParseInfo(string line)
    {
        var depthM = _depthRe.Match(line);
        if (!depthM.Success) return null;

        int depth = int.Parse(depthM.Groups[1].Value);

        int cp = 0;
        bool isMate = false;
        int mateIn = 0;

        var cpM = _cpRe.Match(line);
        if (cpM.Success)
        {
            cp = int.Parse(cpM.Groups[1].Value);
        }
        else
        {
            var mateM = _mateRe.Match(line);
            if (mateM.Success)
            {
                isMate = true;
                mateIn = int.Parse(mateM.Groups[1].Value);
                cp = mateIn > 0 ? 30000 : -30000;
            }
        }

        string pv = _pvRe.Match(line) is { Success: true } pm ? pm.Groups[1].Value.Trim() : string.Empty;
        long nodes = _nodesRe.Match(line) is { Success: true } nm ? long.Parse(nm.Groups[1].Value) : 0;
        int time = _timeRe.Match(line) is { Success: true } tm ? int.Parse(tm.Groups[1].Value) : 0;
        int multiPv = _multiPvRe.Match(line) is { Success: true } mp ? int.Parse(mp.Groups[1].Value) : 1;

        return new StockfishInfo
        {
            Depth = depth,
            Centipawns = cp,
            IsMate = isMate,
            MateIn = mateIn,
            Pv = pv,
            Nodes = nodes,
            Time = time,
            MultiPvIndex = multiPv,
        };
    }
}