using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Config;
using Engine.Models.Enums;
using Engine.Models.Hash;
using Engine.Pgn.Models;
using Engine.Pgn.Streaming;
using Engine.Services;
using Engine.Strategies.Base;
using System.Diagnostics;

namespace NnueTool;

/// <summary>
/// Pass 2+3: replay filtered games, select quiet positions and label them with KioChess's own fixed-depth search.
/// Worker k of n processes games with index % n == k. Progress is saved so an interrupted run resumes.
/// </summary>
internal sealed class LabelCommand
{
    private const ulong BlackToMoveKey = 0x9E3779B97F4A7C15UL;

    private readonly NnueExtractConfiguration _extract;
    private readonly NnueLabelConfiguration _label;
    private readonly MoveHistoryService _moveHistory;
    private readonly Position _position;
    private readonly StrategyBase _strategy;
    private readonly BloomFilter _bloom;
    private readonly Random _random;
    private readonly byte[] _record = new byte[TrainingRecord.Size];
    private readonly MoveProvider _moveProvider;
    private readonly DataPoolService _dataPool;
    private readonly ulong _startHash;
    private readonly ulong[] _startPieces = new ulong[12];

    private long _positions;
    private long _searched;
    private long _failedGames;

    public LabelCommand(NnueExtractConfiguration extract, NnueLabelConfiguration label, int worker)
    {
        _extract = extract;
        _label = label;

        var moveProvider = Boot.GetService<MoveProvider>();
        _moveHistory = Boot.GetService<MoveHistoryService>();

        // Book disabled: empty sequence/popular caches, all sequence hashes are zero.
        if (!MoveHashSequenceHasher.IsInitialized)
            MoveHashSequenceHasher.Initialize(new UInt128[moveProvider.MovesCount]);
        _moveHistory.CreateSequenceCache(new());
        _moveHistory.CreatePopularCache(new());

        // Board reads Zobrist keys from kioapp.db; the connection must be open before the Position is created.
        Boot.GetService<DataAccess.Interfaces.IAppDbService>().Connect();

        _position = new Position();
        _strategy = Boot.GetService<IStrategyFactory>().GetStrategy(label.Depth, _position, label.Strategy);
        _bloom = new BloomFilter(extract.BloomFilterBits);
        _random = new Random(extract.Seed + worker);

        _moveProvider = moveProvider;
        _dataPool = Boot.GetService<DataPoolService>();

        var board = _position.GetBoard();
        _startHash = board.Hash;
        for (byte p = 0; p < 12; p++)
            _startPieces[p] = board.GetPieceBits(p);

        ResetGame(-1);
    }

    public void Run(string input, string outputDirectory, int worker, int workers)
    {
        Directory.CreateDirectory(outputDirectory);
        var outputPath = Path.Combine(outputDirectory, $"labeled-{worker:D2}.bin");
        var progressPath = Path.Combine(outputDirectory, $"labeled-{worker:D2}.progress");
        var donePath = Path.Combine(outputDirectory, $"labeled-{worker:D2}.done");

        if (File.Exists(donePath))
        {
            Console.WriteLine($"W{worker}: already done");
            return;
        }

        // Progress = "<next game> <bin length>". The bin is truncated to the saved length,
        // so records written after the last save are discarded and regenerated exactly once.
        long resumeFrom = 0, resumeLength = 0;
        if (File.Exists(progressPath))
        {
            var parts = File.ReadAllText(progressPath).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            resumeFrom = long.Parse(parts[0]);
            resumeLength = parts.Length > 1 ? long.Parse(parts[1]) : -1;
        }
        var maxDuration = TimeSpan.FromHours(_label.MaxHours);
        var stopwatch = Stopwatch.StartNew();
        long nextFlush = _label.FlushPositions;
        var reportInterval = TimeSpan.FromSeconds(Math.Max(1, _label.ReportSeconds));
        var nextReport = reportInterval;
        long gameIndex = -1;
        long games = 0;
        bool timeLimit = false;

        using var output = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 1 << 20);
        if (resumeLength >= 0 && output.Length != resumeLength)
            output.SetLength(Math.Min(resumeLength, output.Length));
        output.Seek(0, SeekOrigin.End);
        using var reader = new PgnStreamReader(input);

        Console.WriteLine($"Worker {worker}/{workers}: depth={_label.Depth} resume from game {resumeFrom:N0}");

        foreach (var game in reader.ReadGames())
        {
            gameIndex++;
            if (gameIndex < resumeFrom || gameIndex % workers != worker)
                continue;

            ProcessGame(game, output);
            ResetGame(gameIndex);
            games++;

            if (_positions >= nextFlush)
            {
                nextFlush += _label.FlushPositions;
                SaveProgress(output, progressPath, gameIndex + 1);
                Console.WriteLine($"W{worker} {stopwatch.Elapsed:hh\\:mm\\:ss} saved progress at game {gameIndex + 1:N0}");
            }

            if (stopwatch.Elapsed >= nextReport)
            {
                nextReport = stopwatch.Elapsed + reportInterval;
                SaveProgress(output, progressPath, gameIndex + 1);
                Console.WriteLine($"W{worker} {stopwatch.Elapsed:hh\\:mm\\:ss} {reader.ProgressPercent:F2}% games={games:N0} searched={_searched:N0} written={_positions:N0} failed={_failedGames:N0} pos/s={_searched / Math.Max(1, stopwatch.Elapsed.TotalSeconds):F1}");
            }

            if (stopwatch.Elapsed > maxDuration)
            {
                Console.WriteLine($"W{worker}: time limit {_label.MaxHours} h reached");
                timeLimit = true;
                break;
            }
        }

        SaveProgress(output, progressPath, Math.Max(resumeFrom, gameIndex + 1));
        if (!timeLimit)
            File.WriteAllText(donePath, string.Empty);
        Console.WriteLine($"W{worker} done {stopwatch.Elapsed:hh\\:mm\\:ss}: games={games:N0} searched={_searched:N0} written={_positions:N0} failed={_failedGames:N0} -> {outputPath}");
    }

    private void ProcessGame(PgnGame game, FileStream output)
    {
        if (game.Result == GameResult.None || game.Moves.Count == 0)
            return;

        bool isWhite = true;
        int ply = 0;

        try
        {
            foreach (var pgnMove in game.Moves)
            {
                if (ply >= _extract.MaxPly)
                    break;

                var move = PgnMoveResolver.Resolve(_position, pgnMove, isWhite);
                if (move == null)
                {
                    _failedGames++;
                    break;
                }

                if (ply >= _extract.MinPly && IsCandidate(move.IsAttack || move.IsPromotion, isWhite))
                {
                    Label(game.Result, isWhite, ply, output);
                }

                if (ply == 0)
                    _position.MakeFirst(move);
                else
                    _position.Make(move);

                isWhite = !isWhite;
                ply++;
            }
        }
        catch (Exception)
        {
            _failedGames++;
        }
        finally
        {
            _position.Clear();
        }
    }

    /// <summary>
    /// Restores the start position and clears TT and all per-game search state, then validates it.
    /// Throws on any mismatch so contaminated labels are never written.
    /// </summary>
    private void ResetGame(long gameIndex)
    {
        _position.Clear();
        _strategy.Clear();
        _moveHistory.ResetGame(_moveProvider.GetAll());
        _dataPool.ResetGame();

        var board = _position.GetBoard();
        string error = null;

        if (_moveHistory.GetPly() != -1)
            error = $"history ply={_moveHistory.GetPly()}";
        else if (_position.GetHistory().Any())
            error = "history not empty";
        else if (_moveHistory.IsNullPlayed)
            error = "null move flag set";
        else if (_position.GetTurn() != Turn.White)
            error = $"turn={_position.GetTurn()}";
        else if (board.Hash != _startHash)
            error = $"hash={board.Hash:X16} expected={_startHash:X16}";
        else if (!ReferenceEquals(Engine.Models.Moves.MoveBase.Board, board))
            error = "MoveBase.Board does not reference the worker board";
        else if (_strategy.Size != 0)
            error = $"TT count={_strategy.Size}";
        else
        {
            for (byte p = 0; p < 12; p++)
            {
                if ((ulong)board.GetPieceBits(p) != _startPieces[p])
                {
                    error = $"piece {p} bitboard mismatch";
                    break;
                }
            }
        }

        if (error != null)
            throw new InvalidOperationException($"Reset validation failed after game {gameIndex}: {error}");
    }

    private bool IsCandidate(bool isNextMoveTactical, bool isWhite)
    {
        if (_extract.SkipNonQuiet && isNextMoveTactical)
            return false;
        if (_extract.SkipInCheck && _moveHistory.IsLastMoveWasCheck())
            return false;
        if (_random.NextDouble() >= _extract.SampleRate)
            return false;

        ulong key = _position.GetBoard().Hash ^ (isWhite ? 0UL : BlackToMoveKey);
        return _bloom.TryAdd(key);
    }

    private void Label(GameResult result, bool isWhite, int ply, FileStream output)
    {
        _searched++;
        var searchResult = _strategy.GetResult();

        // Value is side-to-move relative (negamax root).
        int score = searchResult.Value;
        if (searchResult.Move == null || Math.Abs(score) > _label.MaxAbsScore)
            return;
        if (_label.SkipCaptureBestMove && (searchResult.Move.IsAttack || searchResult.Move.IsPromotion))
            return;

        byte wdl = result switch
        {
            GameResult.Draw => 1,
            GameResult.White => isWhite ? (byte)2 : (byte)0,
            _ => isWhite ? (byte)0 : (byte)2
        };

        if (!TrainingRecord.Encode(_position.GetBoard(), isWhite, (short)score, wdl, (byte)Math.Min(ply, 255), (byte)_label.Depth, _record))
            return;

        output.Write(_record);
        _positions++;
    }

    private static void SaveProgress(FileStream output, string progressPath, long nextGame)
    {
        output.Flush(true);
        var temp = progressPath + ".tmp";
        File.WriteAllText(temp, $"{nextGame} {output.Length}");
        File.Move(temp, progressPath, true);
    }
}
