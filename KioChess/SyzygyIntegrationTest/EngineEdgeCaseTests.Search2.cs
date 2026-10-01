using DataAccess.Syzygy;
using System.Diagnostics;

internal static partial class EngineEdgeCaseTests
{
    private const int ProvenScore = 20000;
    private const long DepthBudgetMs = 20000;

    private static void SearchBenchmarkTests()
    {
        (string Name, string Pieces)[] positions =
        [
            ("KQP-v-KPP", "Ka1 Qd1 Pb2 ke8 pa7 pg7"),
            ("KRR-v-KPP", "Ka1 Rd1 Rh1 ke8 pa7 pg7"),
            ("KRP-v-KRP", "Ka1 Rd1 Pb4 ke8 rh8 pa7"),
            ("KQ-v-KRPP", "Kb1 Qd1 ke8 rh8 pa7 pb7"),
            ("KBN-v-KPP", "Ka1 Bc1 Nb1 ke8 pa7 pg7"),
            ("KPP-v-KPP", "Ka1 Pb5 Pc5 ke8 pb7 pc7"),
            ("KQP-v-KRPP", "Ka1 Qd1 Pb2 ke8 rh8 pa7 pg7"),
            ("KRPP-v-KPP", "Ka1 Rd1 Pb2 Pc2 ke8 pa7 pg7"),
            ("KRB-v-KRPP", "Ka1 Rd1 Bc1 ke8 rh8 pb7 pg7"),
        ];
        short[] depths = [8, 9, 10, 11];

        long totalOn = 0, totalOff = 0;
        int runs = 0, sameMove = 0, sameValueClass = 0, conflicts = 0, provenOn = 0;

        Console.WriteLine($"{"Position",-11} {"D",2} {"on ms",8} {"off ms",8} {"on val",7} {"off val",7}  on move / off move");

        foreach (var (name, pieces) in positions)
        {
            Setup(pieces, false, false, false, false);
            if (!Shuffle4())
            {
                Check($"SearchBench {name}: shuffle set-up", false);
                continue;
            }

            bool deadline = false;
            foreach (short depth in depths)
            {
                if (deadline) break;

                SetSyzygy(true);
                var onStrategy = NewStrategy("id", depth);
                onStrategy.Clear();
                var sw = Stopwatch.StartNew();
                var on = onStrategy.GetResult();
                sw.Stop();
                long onMs = sw.ElapsedMilliseconds;

                SetSyzygy(false);
                var offStrategy = NewStrategy("id", depth);
                offStrategy.Clear();
                sw.Restart();
                var off = offStrategy.GetResult();
                sw.Stop();
                long offMs = sw.ElapsedMilliseconds;
                SetSyzygy(true);

                Console.WriteLine($"{name,-11} {depth,2} {onMs,8} {offMs,8} {on.Value,7} {off.Value,7}  {on.Move} / {off.Move}");

                Check($"SearchBench {name} d{depth}: both modes return a move", on.Move != null && off.Move != null);
                if (on.Move == null || off.Move == null) continue;

                runs++;
                totalOn += onMs;
                totalOff += offMs;

                if (on.Move.From == off.Move.From && on.Move.To == off.Move.To) sameMove++;

                int onClass = Math.Sign(on.Value) * (Math.Abs(on.Value) > ProvenScore ? 2 : 1);
                int offClass = Math.Sign(off.Value) * (Math.Abs(off.Value) > ProvenScore ? 2 : 1);
                if (onClass == offClass) sameValueClass++;
                if (Math.Abs(on.Value) > ProvenScore) provenOn++;

                bool conflict = Math.Abs(on.Value) > ProvenScore && Math.Abs(off.Value) > ProvenScore
                    && Math.Sign(on.Value) != Math.Sign(off.Value);
                if (conflict) conflicts++;
                Check($"SearchBench {name} d{depth}: no proven-win/proven-loss contradiction ({on.Value} vs {off.Value})", !conflict);
                Check($"SearchBench {name} d{depth}: returned move is legal", on.Move.IsLegal());

                if (Math.Max(onMs, offMs) > DepthBudgetMs)
                {
                    Console.WriteLine($"INFO: {name} deeper searches skipped (budget {DepthBudgetMs} ms exceeded)");
                    deadline = true;
                }
            }
        }

        Console.WriteLine($"SUMMARY: runs={runs} on={totalOn} ms off={totalOff} ms ratio={(totalOff == 0 ? 0 : (double)totalOn / totalOff):F2}");
        Console.WriteLine($"SUMMARY: same move {sameMove}/{runs}, same score class {sameValueClass}/{runs}, tablebase-proven results {provenOn}, contradictions {conflicts}");

        Check("SearchBench: positions were searched", runs >= positions.Length);
        Check($"SearchBench: probing does not slow the search more than 1.5x ({totalOn} vs {totalOff} ms)", totalOn <= totalOff * 1.5 + 200);
        Check("SearchBench: no contradictions overall", conflicts == 0);
    }
}
