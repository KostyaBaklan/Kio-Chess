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
        Check("TT: tablebase win is unchanged by ToTTValue/FromTTValue", Call(strategy, "ToTTValue", win) == win && Call(strategy, "FromTTValue", win) == win);
        Check("TT: tablebase loss is unchanged by ToTTValue/FromTTValue", Call(strategy, "ToTTValue", loss) == loss && Call(strategy, "FromTTValue", loss) == loss);
        Check("TT: stored value fits a short", win < short.MaxValue && loss > short.MinValue);

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
    }
}
