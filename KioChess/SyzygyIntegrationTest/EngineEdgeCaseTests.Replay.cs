using System.Text.RegularExpressions;

internal static partial class EngineEdgeCaseTests
{
    private const string ReplayFile = @"C:\Dev\Kio-Chess\KioChess\KioChess.App\bin\Debug\net10.0-windows7.0\History\2026_10_01_03_10_00.txt";

    private static List<string> ParseHistory(string path)
    {
        var moves = new List<string>();
        var rx = new Regex(@"([WB])=\s*[NBRQK]?\s*(?:(0 - 0)|([A-H][1-8]) [x-] ([A-H][1-8]))");
        foreach (Match m in rx.Matches(File.ReadAllText(path)))
        {
            bool white = m.Groups[1].Value == "W";
            if (m.Groups[2].Success) moves.Add(white ? "e1g1" : "e8g8");
            else moves.Add((m.Groups[3].Value + m.Groups[4].Value).ToLower());
        }
        return moves;
    }

    private static void ReplayTests()
    {
        if (!File.Exists(ReplayFile)) return;
        var moves = ParseHistory(ReplayFile);
        const short depth = 8;
        Setup("Ra1 Nb1 Bc1 Qd1 Ke1 Bf1 Ng1 Rh1 Pa2 Pb2 Pc2 Pd2 Pe2 Pf2 Pg2 Ph2 ra8 nb8 bc8 qd8 ke8 bf8 ng8 rh8 pa7 pb7 pc7 pd7 pe7 pf7 pg7 ph7");
        int sameMove = 0, blackTurns = 0, previous = int.MinValue;
        Console.WriteLine($"Replay: {moves.Count} plies, lmrd depth {depth}");
        for (int ply = 0; ply < moves.Count; ply++)
        {
            if (!WhiteToMove)
            {
                blackTurns++;
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var r = NewStrategy("lmrd", depth).GetResult();
                sw.Stop();
                string engine = r.Move == null ? "-" : $"{Name(r.Move.From)}{Name(r.Move.To)}";
                if (engine == moves[ply]) sameMove++;
                string flag = previous != int.MinValue && r.Value - previous <= -150 ? "  <-- score drop" : "";
                Console.WriteLine($"  ply {ply + 1,3} black: played {moves[ply]} engine {engine} value {r.Value,6} reversible {_history.GetReversibleMovesCount()} {sw.ElapsedMilliseconds} ms{flag}");
                previous = r.Value;
            }
            try { Play(moves[ply]); }
            catch (InvalidOperationException e) { Check($"Replay: ply {ply + 1} {moves[ply]} is replayable ({e.Message})", false); return; }
        }
        Console.WriteLine($"Replay: engine reproduces its own move {sameMove}/{blackTurns}");
        Check("Replay: whole game replayed", true);
    }

    private static string Name(byte sq) => $"{(char)(97 + sq % 8)}{(char)(49 + sq / 8)}";
}
