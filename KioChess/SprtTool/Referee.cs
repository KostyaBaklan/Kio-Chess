using System.Diagnostics;
using System.Text;

namespace SprtTool;

internal static class Referee
{
    private static readonly object Sync = new();
    private static readonly long[] Ptn = new long[5];
    private static long _win, _draw, _loss, _pairs;
    private static int _next;
    private static volatile bool _stop;
    private static SprtState _final = SprtState.Continue;
    private static StreamWriter _log = null;
    private static readonly Stopwatch _clock = new();
    private static int _planned;   // pairs this run can play: the lesser of MaxPairs and the openings count
    private static int _discarded, _consecutiveFailures;
    private static string _abort;  // set when the run stops because pairs keep failing
    private const int MaxConsecutiveFailures = 10;

    public static int Run(SprtConfig c, SprtPaths p)
    {
        // The file is sorted by popularity. Keep the top MaxPairs, then shuffle once with a fixed seed
        // so an early stop does not test only the most popular lines.
        var all = File.ReadAllLines(p.OpeningsFile)
            .Where(l => l.Length > 0)
            .Select(l => l.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(short.Parse).ToArray())
            .ToArray();
        var openings = all.Take(c.MaxPairs).ToArray();
        new Random(c.OpeningShuffleSeed).Shuffle(openings);
        ResetState();
        _planned = openings.Length;
        _clock.Restart();
        using var log = new StreamWriter(p.LogFile, true) { AutoFlush = true };
        _log = log;
        ConsoleCancelEventHandler onCancel = (_, e) => { e.Cancel = true; _stop = true; Both("Stopping after the current pairs..."); };
        Console.CancelKeyPress += onCancel;

        Log($"--- Run started {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---");
        Both($"SPRT H0 elo0={c.Elo0}, H1 elo1={c.Elo1}, alpha={c.Alpha}, beta={c.Beta}; depth {c.Depth}; {openings.Length:N0} openings (shuffled, seed {c.OpeningShuffleSeed}); {c.Concurrency} openings at a time, 2 games each = {c.Concurrency * 4} engine processes");
        Both($"Status is printed after every opening (2 games). ETA assumes all {_planned:N0} planned pairs, so it is an upper bound; SPRT may stop earlier.");
        if (openings.Length < c.MaxPairs)
            Both($"Warning: only {openings.Length:N0} openings available, fewer than MaxPairs ({c.MaxPairs:N0}). The test stops when they run out and may be inconclusive. No opening is repeated.");

        try
        {
            var tasks = Enumerable.Range(0, Math.Max(1, c.Concurrency))
                .Select(slot => Task.Factory.StartNew(() => Slot(slot, c, p, openings), TaskCreationOptions.LongRunning))
                .ToArray();
            Task.WaitAll(tasks);
        }
        finally
        {
            Console.CancelKeyPress -= onCancel;
        }

        PrintStatus(c);
        if (_abort != null) Both(_abort);
        string verdict = _final switch
        {
            SprtState.AcceptH1 => "H1 accepted: candidate is stronger. Promote it.",
            SprtState.AcceptH0 => "H0 accepted: no gain. Reject it.",
            _ => "Inconclusive (pair cap or stopped). Treat as neutral."
        };
        Both(verdict);
        if (_discarded > 0) Both($"Discarded pairs (failed, not scored): {_discarded:N0}. Details are in the log.");
        Both($"Finished in {Progress.Clock(_clock.Elapsed)}");
        return _final == SprtState.AcceptH1 ? 0 : _final == SprtState.AcceptH0 ? 1 : 2;
    }

    // The menu can run the test more than once in the same process, so every run starts from zero.
    private static void ResetState()
    {
        Array.Clear(Ptn);
        _win = _draw = _loss = _pairs = 0;
        _next = 0;
        _stop = false;
        _final = SprtState.Continue;
        _discarded = _consecutiveFailures = 0;
        _abort = null;
    }

    private static void Slot(int slot, SprtConfig c, SprtPaths p, short[][] openings)
    {
        var timeout = TimeSpan.FromSeconds(c.ReplyTimeoutSeconds);
        while (!_stop)
        {
            int i = Interlocked.Increment(ref _next) - 1;
            if (i >= openings.Length) break;
            Log($"opening {i}: started");
            // Both games of one opening run at the same time: candidate as White and as Black.
            var asWhite = Task.Factory.StartNew(() => PlayFreshGame(slot, c, p, timeout, openings[i], candidateWhite: true), TaskCreationOptions.LongRunning);
            var asBlack = Task.Factory.StartNew(() => PlayFreshGame(slot, c, p, timeout, openings[i], candidateWhite: false), TaskCreationOptions.LongRunning);
            try
            {
                Task.WaitAll(asWhite, asBlack);   // wait for both, so no engine is left running after a failure
                Record(c, i, asWhite.Result, asBlack.Result);
            }
            catch (Exception e)
            {
                Discard(i, e is AggregateException ae ? ae.InnerException ?? ae : e);
            }
        }
    }

    // A failed pair is logged and not scored. Failures in a row mean the engines or setup are broken, so stop.
    private static void Discard(int opening, Exception e)
    {
        lock (Sync)
        {
            _discarded++;
            Log($"opening {opening}: DISCARDED {e.GetType().Name}: {e.Message}");
            if (++_consecutiveFailures >= MaxConsecutiveFailures && _final == SprtState.Continue && _abort == null)
            {
                _abort = $"Stopped: {MaxConsecutiveFailures} pairs in a row failed. Last error: {e.Message}";
                _stop = true;
            }
        }
    }

    // Every game starts new engine processes, so no history or move-ordering state carries over between games.
    private static GameOutcome PlayFreshGame(int slot, SprtConfig c, SprtPaths p, TimeSpan timeout, short[] opening, bool candidateWhite)
    {
        string colour = candidateWhite ? "w" : "b";
        using var cand = new PlayerProc($"cand{slot}{colour}", p.CandidateDir, c.ExeName, timeout);
        using var baseline = new PlayerProc($"base{slot}{colour}", p.BaselineDir, c.ExeName, timeout);
        cand.WaitReady();       // both engines start up together; the waits overlap
        baseline.WaitReady();
        try
        {
            return candidateWhite
                ? PlayGame(cand, baseline, opening, c)
                : PlayGame(baseline, cand, opening, c);
        }
        finally
        {
            cand.RequestExit();      // both engines are asked to quit before either is waited on
            baseline.RequestExit();
        }
    }

    private static GameOutcome PlayGame(PlayerProc white, PlayerProc black, short[] opening, SprtConfig c)
    {
        white.Ask("NEW");
        black.Ask("NEW");
        var moves = new StringBuilder();
        foreach (var key in opening)
        {
            string h1 = white.Ask($"APPLY {key}")[1], h2 = black.Ask($"APPLY {key}")[1];
            if (h1 != h2) throw new InvalidOperationException($"Position hash differs after opening move {key}");
            moves.Append(key).Append(' ');
        }

        bool whiteToMove = opening.Length % 2 == 0;
        int streak = 0, streakSign = 0;
        for (int ply = opening.Length; ply < c.MaxPlies; ply++)
        {
            var mover = whiteToMove ? white : black;
            var other = whiteToMove ? black : white;
            var reply = mover.Ask($"GO {c.Depth}");

            if (reply[0] == "END")
            {
                // The side to move has no move: Mate = it lost, anything else (Pat, repetition, 50 moves, Draw) = draw.
                double score = reply[1] == "Mate" ? (whiteToMove ? 0.0 : 1.0) : 0.5;
                return new GameOutcome(score, reply[1], ply, moves.ToString());
            }

            if (reply[0] != "MOVE" || reply.Length < 5)
                throw new InvalidOperationException($"Unexpected reply to GO at ply {ply}: {string.Join(' ', reply)}");

            // MOVE <key> <score for side to move> <hash after move> <text>
            string key = reply[1];
            int value = int.Parse(reply[2]);
            string hashAfter = other.Ask($"APPLY {key}")[1];
            if (hashAfter != reply[3]) throw new InvalidOperationException($"Desync at ply {ply} after {reply[4]}");
            moves.Append(reply[4]).Append(' ');

            int whiteValue = whiteToMove ? value : -value;
            int sign = c.AdjudicatePlies > 0 && Math.Abs(whiteValue) >= c.AdjudicateScore ? Math.Sign(whiteValue) : 0;
            streak = sign == 0 ? 0 : sign == streakSign ? streak + 1 : 1;
            streakSign = sign;
            if (sign != 0 && streak >= c.AdjudicatePlies)
                return new GameOutcome(sign > 0 ? 1.0 : 0.0, "adjudicated", ply, moves.ToString());

            whiteToMove = !whiteToMove;
        }
        return new GameOutcome(0.5, "max plies", c.MaxPlies, moves.ToString());
    }

    private static void Record(SprtConfig c, int opening, GameOutcome g1, GameOutcome g2)
    {
        double s1 = g1.WhiteScore;          // candidate as White
        double s2 = 1.0 - g2.WhiteScore;    // candidate as Black
        lock (Sync)
        {
            _consecutiveFailures = 0;
            if (_final != SprtState.Continue) return;   // decision already made, ignore pairs that finished late
            _log.WriteLine($"opening {opening}: cand-white {s1} ({g1.Reason}, {g1.Plies} plies) | cand-black {s2} ({g2.Reason}, {g2.Plies} plies)");
            _log.WriteLine($"  W: {g1.Moves}");
            _log.WriteLine($"  B: {g2.Moves}");

            Ptn[(int)Math.Round((s1 + s2) * 2)]++;
            foreach (var s in new[] { s1, s2 })
            {
                if (s == 1.0) _win++; else if (s == 0.0) _loss++; else _draw++;
            }
            _pairs++;

            var (_, _, _, state) = Sprt.Evaluate(Ptn, c.Elo0, c.Elo1, c.Alpha, c.Beta);
            if (state != SprtState.Continue || _pairs >= c.MaxPairs) { _final = state; _stop = true; }
            PrintStatus(c);
        }
    }

    private static void PrintStatus(SprtConfig c)
    {
        var (llr, lower, upper, _) = Sprt.Evaluate(Ptn, c.Elo0, c.Elo1, c.Alpha, c.Beta);
        var (elo, err) = Sprt.Estimate(Ptn);
        double fraction = _planned > 0 ? (double)_pairs / _planned : 0;
        Both($"[{Progress.Clock(_clock.Elapsed)}] {_pairs:N0} of {_planned:N0} pairs ({100 * fraction:F1}%) | ETA {Progress.Remaining(fraction, _clock.Elapsed)}");
        Both($"W-D-L {_win}-{_draw}-{_loss} | ptnml [{string.Join(", ", Ptn)}] | LLR {llr:F2} [{lower:F2}, {upper:F2}] | Elo {elo:F1} +/- {err:F1}");
    }

    // Console and log get the same line. The log is AutoFlush, so each line reaches disk immediately.
    private static void Both(string text)
    {
        Console.WriteLine(text);
        Log(text);
    }

    private static void Log(string text)
    {
        lock (Sync) _log.WriteLine(text);
    }
}
