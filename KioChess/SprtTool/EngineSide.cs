// Everything that touches the chess engine is in this file.
using Engine.Dal.Interfaces;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Config;
using Engine.Models.Enums;
using Engine.Models.Hash;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Pgn.Models;
using Engine.Pgn.Streaming;
using System.Diagnostics;
using System.Text;
using Engine.Services;
using Engine.Strategies.Base;
using DataAccess.Contexts;
using DataAccess.Services.EntityServices;
using Microsoft.EntityFrameworkCore;
using TuningCommon.Data;   // TexelPgnConverter, PositionHash (TuningCommon)

namespace SprtTool;

internal static class EngineHost
{
    public static Position Position = null!;
    public static Board Board = null!;
    public static MoveHistoryService History = null!;
    public static MoveProvider Moves = null!;
    public static int StartPly;

    private static bool _initialized;

    // Same start-up sequence as TexelTool.Main. Safe to call again: the menu can build openings more than once.
    public static void Init()
    {
        if (_initialized) return;
        Boot.SetUp();
        var db = Boot.GetService<DataAccess.Interfaces.IAppDbService>();
        db.Connect();
        History = Boot.GetService<MoveHistoryService>();
        Moves = ContainerLocator.Current.Resolve<MoveProvider>();
        if (!MoveHashSequenceHasher.IsInitialized)
        {
            MoveHashSequenceHasher.Initialize(db.GetAllMoveHashValues());
            ContainerLocator.Current.Resolve<ICacheLoaderService>().LoadAsync().Wait();
        }
        Position = new Position();
        Board = Position.GetBoard();
        StartPly = History.GetPly();
        _initialized = true;
    }

    public static void Make(MoveBase move)
    {
        if (Position.GetHistory().Any()) Position.Make(move);
        else Position.MakeFirst(move);
    }

    // Same cleanup as the finally block in TexelTool.Extract.
    public static void Reset()
    {
        while (History.GetPly() > StartPly) Position.UnMake();
        Position.SetWhiteTurn();
    }

    // Same hash as TexelTool (PositionHash in TuningCommon). Both players must return the same value after every move.
    // Before the first move, castle rights are not stored yet; 0xF0 is MoveHistoryService's private ALL_CASTLE_MASK.
    public static ulong Hash() =>
        PositionHash.Compute(Board, History.GetPly() < 0 ? (byte)0xF0 : History.GetCastleRights(), Position.GetTurn() == Turn.White, PositionHash.EnPassantFile(History));
}

/// <summary>
/// Engine process. Protocol (one command per line on stdin, replies start with "@@" on stdout):
///   NEW              -> @@OK hash
///   APPLY key        -> @@OK hash
///   GO depth         -> @@MOVE key score hash text   |   @@END GameResultName
///   EXIT
/// Moves travel as MoveBase.Key (short), so no UCI or FEN is needed.
/// </summary>
internal static class PlayerMode
{
    // Same factory and configured code as the app (KioChess.App StrategyProvider), without its fallback:
    // an unknown code must fail instead of testing a different search.
    private static StrategyBase CreateStrategy(int depth, Position position)
    {
        var code = Boot.GetService<IConfigurationProvider>().GeneralConfiguration.Strategy;
        return Boot.GetService<IStrategyFactory>().GetStrategy((short)depth, position, code);
    }

    public static void Run()
    {
        EngineHost.Init();
        StrategyBase strategy = null;
        int strategyDepth = -1;
        Console.WriteLine("@@READY");

        string line;
        while ((line = Console.ReadLine()) != null)
        {
            var parts = line.Split(' ');
            try
            {
                if (parts[0] == "EXIT") break;
                if (parts[0] == "NEW")
                {
                    strategy?.Clear();                       // transposition tables
                    EngineHost.Reset();
                    Console.WriteLine($"@@OK {EngineHost.Hash()}");
                }
                else if (parts[0] == "APPLY")
                {
                    EngineHost.Make(EngineHost.Moves.Get(short.Parse(parts[1])));
                    Console.WriteLine($"@@OK {EngineHost.Hash()}");
                }
                else if (parts[0] == "GO")
                {
                    int depth = int.Parse(parts[1]);
                    if (strategy == null || strategyDepth != depth)
                    {
                        strategy = CreateStrategy(depth, EngineHost.Position);
                        strategyDepth = depth;
                    }
                    var result = strategy.GetResult();
                    strategy.ExecuteAsyncAction();                    // same post-search calls as GameViewModel
                    strategy.ExecuteNullAsyncAction();
                    var move = result.Move;
                    if (move == null)
                    {
                        Console.WriteLine($"@@END {result.GameResult}");   // Mate, Pat, ThreefoldRepetition, FiftyMoves, Draw
                        continue;
                    }
                    EngineHost.Make(move);
                    // A single legal move is returned without search, so Value keeps its int.MinValue sentinel.
                    int value = result.Value == int.MinValue ? 0 : result.Value;
                    Console.WriteLine($"@@MOVE {move.Key} {value} {EngineHost.Hash()} {move.From.AsString()}{move.To.AsString()}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"@@ERR {e.GetType().Name}: {e.Message.Replace('\n', ' ')}");
            }
        }
    }
}

// Read-only games.db context. The base class hardcodes the path, so this subclass supplies the configured one.
internal sealed class GamesDbPopularity : GamesDbContext
{
    private readonly string _path;

    public GamesDbPopularity(string path) => _path = path;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlite($"Data Source={_path};Mode=ReadOnly");
}

// Replays stored move keys on the engine board and prints them as PGN movetext (menu option 4).
internal static class OpeningNotation
{
    public static void Print(string file, int rows)
    {
        EngineHost.Init();
        int shown = 0;
        foreach (var line in File.ReadLines(file).Where(l => l.Trim().Length > 0).Take(rows))
            Console.WriteLine($"{++shown,4}  {ToPgn(line)}");
    }

    private static string ToPgn(string line)
    {
        var sb = new StringBuilder();
        try
        {
            var keys = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => short.Parse(s)).ToList();
            for (int ply = 0; ply < keys.Count; ply++)
            {
                bool white = ply % 2 == 0;
                var moves = EngineHost.Position.GetAllMoves();
                var move = moves.FirstOrDefault(m => m.Key == keys[ply])
                           ?? throw new InvalidOperationException($"move key {keys[ply]} is not legal at ply {ply + 1}");
                var san = San(move, moves);
                EngineHost.Make(move);
                if (white ? EngineHost.Board.IsCheckToBlack() : EngineHost.Board.IsCheckToWhite()) san += "+";
                if (ply > 0) sb.Append(' ');
                if (white) sb.Append(ply / 2 + 1).Append(". ");
                sb.Append(san);
            }
        }
        catch (Exception e)
        {
            sb.Append(" ?? ").Append(e.Message);
        }
        finally
        {
            EngineHost.Reset();
        }
        return sb.ToString();
    }

    // Standard Algebraic Notation for one legal move. `moves` is every move in the position, before it is made.
    private static string San(MoveBase m, List<MoveBase> moves)
    {
        if (m is SmallCastle) return "O-O";
        if (m is BigCastle) return "O-O-O";

        var from = m.From.AsString().ToLower();
        var to = m.To.AsString().ToLower();
        var piece = m.Piece.AsName();   // "P" for pawns
        bool isPawn = piece == "P";
        bool isCapture = m is AttackBase && m is not PromotionMove;
        var sb = new StringBuilder();

        if (!isPawn) sb.Append(piece).Append(Disambiguation(m, moves, from));
        else if (isCapture) sb.Append(from[0]);
        if (isCapture) sb.Append('x');
        sb.Append(to);

        byte? promotion = m switch { PromotionMove pm => pm.PromotionPiece, PromotionAttack pa => pa.PromotionPiece, _ => null };
        if (promotion.HasValue) sb.Append('=').Append(promotion.Value.AsName());
        return sb.ToString();
    }

    // Adds the file, rank, or both when another piece of the same kind could also reach the square.
    private static string Disambiguation(MoveBase m, List<MoveBase> moves, string from)
    {
        var rivals = moves.Where(o => o.Key != m.Key && o.Piece == m.Piece && o.To == m.To && o.From != m.From).ToList();
        if (rivals.Count == 0) return "";
        bool sameFile = rivals.Any(o => o.From.AsString().ToLower()[0] == from[0]);
        bool sameRank = rivals.Any(o => o.From.AsString().ToLower()[1] == from[1]);
        if (!sameFile) return from[0].ToString();
        if (!sameRank) return from[1].ToString();
        return from;
    }
}

/// <summary>Builds the openings file from PGN and .zst games (menu option 1).</summary>
internal static class OpeningBuilder
{
    private static bool IsRealMove(PgnMove m) =>
        !(m.Castling == CastlingType.None && string.IsNullOrEmpty(m.TargetSquare)
          && !string.IsNullOrEmpty(m.Notation) && m.Notation.Contains('.'));

    private static bool EloOk(PgnGame game, int minElo)
    {
        if (game.Tags == null || game.Tags.ContainsKey("FEN")) return false;
        if (!game.Tags.TryGetValue("WhiteElo", out var w) || !int.TryParse(w, out var white)) return false;
        if (!game.Tags.TryGetValue("BlackElo", out var b) || !int.TryParse(b, out var black)) return false;
        return Math.Min(white, black) >= minElo;
    }

    // Games in games.db that reached this opening, found with the prefix hash that GameEntityFactory stores.
    private static long Popularity(GameEntityService db, List<short> keys)
    {
        UInt128 prefix = UInt128.Zero;
        for (int i = 0; i < keys.Count - 1; i++) prefix ^= MoveHashSequenceHasher.GetMoveHash(keys[i]);
        var record = db.GetByHashAndMove(prefix, keys[^1]);
        return record == null ? 0 : (long)record.White + record.Draw + record.Black;
    }

    // Scans every eligible game and keeps every distinct opening of exactly `plies` plies.
    // Output is sorted by games.db popularity, most played first. Ties keep a shuffled order.
    public static int Build(string dir, string output, string gamesDb, int plies, int minElo, int maxEval)
    {
        EngineHost.Init();
        var rng = new Random(12345);
        var seen = new HashSet<ulong>();
        var openings = new List<(string Line, long Games)>();
        using var popularityContext = new GamesDbPopularity(gamesDb);
        var popularity = new GameEntityService(popularityContext);
        var files = Directory.GetFiles(dir, "*.zst").Concat(Directory.GetFiles(dir, "*.pgn")).OrderBy(f => f).ToArray();
        long games = 0, failures = 0;
        var clock = Stopwatch.StartNew();
        long totalBytes = files.Sum(f => new FileInfo(f).Length), doneBytes = 0;
        var nextReport = TimeSpan.FromSeconds(10);

        foreach (var file in files)
        {
            using var reader = new PgnStreamReader(file);
            foreach (var game in reader.ReadGames())
            {
                if (!EloOk(game, minElo)) continue;
                games++;
                if (clock.Elapsed >= nextReport)
                {
                    nextReport = clock.Elapsed + TimeSpan.FromSeconds(10);
                    Console.WriteLine(Progress.Line(clock.Elapsed, doneBytes + reader.Position, totalBytes, games, openings.Count));
                }
                var moves = game.Moves.Where(IsRealMove).Take(plies).ToList();
                if (moves.Count < plies) continue;
                try
                {
                    var keys = new List<short>();
                    bool isWhite = true;
                    foreach (var m in moves)
                    {
                        TexelPgnConverter.ProcessMove(EngineHost.Position, m, isWhite);
                        keys.Add(EngineHost.History.GetLastMove().Key);
                        isWhite = !isWhite;
                    }
                    int eval = Math.Abs(EngineHost.Board.Evaluate());   // White to move
                    if (eval <= maxEval && seen.Add(EngineHost.Hash()))
                        openings.Add((string.Join(' ', keys), Popularity(popularity, keys)));
                }
                catch { failures++; }
                finally { EngineHost.Reset(); }
            }
            doneBytes += reader.Length;
        }

        var ranked = openings.OrderBy(_ => rng.Next()).OrderByDescending(o => o.Games).ToList();
        File.WriteAllLines(output, ranked.Select(o => o.Line));
        long matched = ranked.Count(o => o.Games > 0);
        long top = ranked.Count == 0 ? 0 : ranked[0].Games;
        Console.WriteLine($"games.db: {matched} of {ranked.Count} openings found; most popular has {top} games.");
        Console.WriteLine($"Wrote {ranked.Count} openings to {output} (games scanned {games}, failures {failures}) in {Progress.Clock(clock.Elapsed)}");
        return ranked.Count;
    }
}

/// <summary>Time and progress formatting shared by the builder and the referee.</summary>
internal static class Progress
{
    public static string Clock(TimeSpan t) => $"{(int)t.TotalHours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";

    // Extrapolates the time left from the share of work done so far.
    public static string Remaining(double fraction, TimeSpan elapsed) =>
        fraction <= 0 ? "--:--:--" : Clock(TimeSpan.FromSeconds(elapsed.TotalSeconds * (1 - fraction) / fraction));

    public static string Line(TimeSpan elapsed, long done, long total, long games, int openings)
    {
        double fraction = total > 0 ? (double)done / total : 0;
        double rate = games / Math.Max(elapsed.TotalSeconds, 1);
        return $"[{Clock(elapsed)}] {100 * fraction:F1}% of input | {games:N0} games checked | {openings:N0} openings | {rate:N0} games/s | ETA {Remaining(fraction, elapsed)}";
    }
}
