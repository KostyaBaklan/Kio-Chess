using Engine.Strategies.Base;
using System.Reflection;

internal static partial class EngineEdgeCaseTests
{
    private static readonly BindingFlags Inst = BindingFlags.NonPublic | BindingFlags.Instance;

    private static int Call(StrategyBase s, string name, int value) =>
        (int)typeof(StrategyBase).GetMethod(name, Inst)!.Invoke(s, [value])!;

    private static void TranspositionTests()
    {
        int bound = Mate - 1;

        Setup("Ka1 Qd1 ke8", false, false, false, false);
        Check("TT: shuffle set-up", Shuffle4());
        var strategy = NewStrategy();
        int threshold = (int)typeof(StrategyBase).GetMethod("GetMateThreshold", Inst)!.Invoke(strategy, null)!;
        int ply = _history.GetPly();

        int win = TablebaseWin - ply;
        int loss = ply - TablebaseWin;

        Check($"TT: tablebase win is below the mate threshold ({win} <= {threshold})", win <= threshold && loss >= -threshold);
        Check("TT: tablebase win is stored root-relative (ply removed)", Call(strategy, "ToTTValue", win) == TablebaseWin && Call(strategy, "FromTTValue", TablebaseWin) == win);
        Check("TT: tablebase loss is stored root-relative (ply removed)", Call(strategy, "ToTTValue", loss) == -TablebaseWin && Call(strategy, "FromTTValue", -TablebaseWin) == loss);
        Check("TT: tablebase win round-trips", Call(strategy, "FromTTValue", Call(strategy, "ToTTValue", win)) == win);
        Check("TT: tablebase loss round-trips", Call(strategy, "FromTTValue", Call(strategy, "ToTTValue", loss)) == loss);
        Check("TT: static scores are unchanged", Call(strategy, "ToTTValue", 123) == 123 && Call(strategy, "FromTTValue", -4567) == -4567 && Call(strategy, "ToTTValue", 0) == 0);
        Check("TT: stored value fits a short", TablebaseWin < short.MaxValue && -TablebaseWin > short.MinValue);

        int mate = Mate - ply;
        Check("TT: real mate score is still ply-normalized",
            mate > threshold && Call(strategy, "ToTTValue", mate) == Mate && Call(strategy, "FromTTValue", Mate) == mate);
        Check("TT: tablebase win is below any real mate", win < mate);
        Check("TT: tablebase win outranks a large static score", win > 5000);

        strategy.Clear();
        int first = strategy.SearchWhite(-bound, bound, 4);
        int second = strategy.SearchWhite(-bound, bound, 4);
        Check($"TT: warm-table search equals cold search ({first} vs {second})", first == second);

        Setup("Ka1 Qb2 ke8 rh8", false, false, false, false);
        Play("b2h8");
        strategy = NewStrategy();
        strategy.Clear();
        int cutoff = strategy.SearchBlack(-bound, bound, 4);
        Check($"TT: tablebase cutoff at clock 0 is independent of a cleared table ({cutoff})",
            _history.GetReversibleMovesCount() == 0 && cutoff == _history.GetPly() - TablebaseWin);

        TranspositionPlyTests();
    }

    private static void TranspositionPlyTests()
    {
        Setup("Ka1 Qd1 ke8", false, false, false, false);
        Check("TT ply: shuffle set-up", Shuffle4());
        var strategy = NewStrategy();
        int ply = _history.GetPly();
        int stored = Call(strategy, "ToTTValue", TablebaseWin - ply);
        int storedLoss = Call(strategy, "ToTTValue", ply - TablebaseWin);

        Check("TT ply: shuffle again", Shuffle4());
        int laterPly = _history.GetPly();

        Check($"TT ply: transposed tablebase win is re-based to the new ply ({ply} -> {laterPly})",
            laterPly != ply && Call(strategy, "FromTTValue", stored) == TablebaseWin - laterPly);
        Check("TT ply: transposed tablebase loss is re-based to the new ply",
            Call(strategy, "FromTTValue", storedLoss) == laterPly - TablebaseWin);
        Check("TT ply: re-based tablebase win stays below a real mate",
            Call(strategy, "FromTTValue", stored) < Mate - laterPly);
        Check("TT ply: re-based tablebase win stays above static scores",
            Call(strategy, "FromTTValue", stored) > 5000 && Call(strategy, "FromTTValue", storedLoss) < -5000);
        Check("TT ply: real mate score still round-trips at a later ply",
            Call(strategy, "FromTTValue", Call(strategy, "ToTTValue", Mate - laterPly)) == Mate - laterPly
            && Call(strategy, "ToTTValue", laterPly - Mate) == -Mate);

        strategy.Clear();
        int bound = Mate - 1;
        int cold = strategy.SearchWhite(-bound, bound, 4);
        int warm = strategy.SearchWhite(-bound, bound, 4);

    }
}
