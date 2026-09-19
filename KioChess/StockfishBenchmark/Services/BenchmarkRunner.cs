using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Strategies.Base;
using StockfishBenchmark.Models;
using StockFishCore.Stockfish;
using System.Diagnostics;

namespace StockfishBenchmark.Services;

public class BenchmarkRunner : IBenchmarkRunner
{
    private const string StockfishPath =
        @"..\..\..\stockfish\stockfish-windows-x86-64-avx2.exe";

    public async Task<BenchmarkResult> RunAsync(
        BenchmarkSession session,
        IProgress<BenchmarkMoveRecord> progress = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => RunCore(session, progress, cancellationToken), cancellationToken);
    }

    private BenchmarkResult RunCore(
        BenchmarkSession session,
        IProgress<BenchmarkMoveRecord> progress,
        CancellationToken cancellationToken)
    {
        var records = new List<BenchmarkMoveRecord>();

        var stockfish = new Stockfish(StockfishPath, session.StockfishDepth, session.StockfishElo);
        var position = new Position();
        var strategyFactory = ContainerLocator.Current.Resolve<IStrategyFactory>();
        var strategy = strategyFactory.GetStrategy(session.Depth, position, session.Strategy);
        var endGameStrategy = strategyFactory.GetStrategy(2, position, "ab");

        // engine is opposite to the player colour the user wants to "be"
        // session.Color is the colour Stockfish plays: "w" = Stockfish White, "b" = Stockfish Black
        bool isStockfishTurn = session.Color == "w";

        int engineMoveCount = 0;
        IResult result = new Result();
        int plyIndex = 0;

        while (result.GameResult == GameResult.Continue
               && engineMoveCount < session.MoveCount
               && !cancellationToken.IsCancellationRequested)
        {
            plyIndex++;
            var fen = stockfish.GetFenPosition();
            var record = new BenchmarkMoveRecord { MoveNumber = plyIndex };

            if (isStockfishTurn)
            {
                record.IsEngineMove = false;
                var bestMoveUci = stockfish.GetBestMove();
                var legalMoves = position.GetAllMoves();
                MoveBase chosenMove = null;

                foreach (var m in legalMoves)
                {
                    if (m.ToUciString() == bestMoveUci)
                    {
                        chosenMove = m;
                        break;
                    }
                }

                if (chosenMove == null)
                {
                    if (legalMoves == null || legalMoves.Count == 0)
                    {
                        result = endGameStrategy.GetResult(short.MinValue, short.MaxValue, 1);
                    }
                    break;
                }

                ApplyMove(position, stockfish, fen, chosenMove);
                FillCommonFields(record, chosenMove, position, 0, null);
            }
            else
            {
                record.IsEngineMove = true;
                engineMoveCount++;

                var sw = Stopwatch.StartNew();
                result = strategy.GetResult();
                sw.Stop();

                if (result.Move == null) break;

                ApplyMove(position, stockfish, fen, result.Move);
                FillCommonFields(record, result.Move, position, sw.ElapsedMilliseconds, strategy);
            }

            progress?.Report(record);
            records.Add(record);
            isStockfishTurn = !isStockfishTurn;
        }

        var benchmarkResult = new BenchmarkResult
        {
            Session = session,
            Moves   = records,
            Summary = BenchmarkResult.ComputeSummary(records)
        };

        return benchmarkResult;
    }

    private static void ApplyMove(Position position, Stockfish stockfish,
                                   string fen, MoveBase move)
    {
        if (position.GetHistory().Any())
            position.Make(move);
        else
            position.MakeFirst(move);

        stockfish.SetPosition(fen, move.ToUciString());
    }

    private static void FillCommonFields(BenchmarkMoveRecord record, MoveBase move,
                                          Position position, long durationMs,
                                          StrategyBase strategy)
    {
        record.MoveKey      = move.Key;
        record.UciMove      = move.ToUciString();
        record.MoveNotation = move.ToLightString();
        record.DurationMs   = durationMs;
        record.MaterialValue = position.GetValue();
        record.StaticValue   = position.GetStaticValue();
        record.TtCount       = strategy?.Size ?? 0;
        record.ProcessMemoryMB = Process.GetCurrentProcess().WorkingSet64 / 1_048_576.0;
    }
}
