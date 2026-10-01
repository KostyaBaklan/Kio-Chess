using DataAccess.Syzygy;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Services.Syzygy;
using Engine.Strategies.Base;
using System.Diagnostics;
using System.Reflection;

internal static partial class EngineEdgeCaseTests
{
    private static StrategyBase NewStrategy(string code, short depth) =>
        Boot.GetService<IStrategyFactory>().GetStrategy(depth, _position, code);

    private static void SetSyzygy(bool enabled) =>
        typeof(SyzygyService).GetField("_initialized", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(SyzygyService.Instance, enabled);

    private static void WrapperStrategyTests()
    {
        var service = SyzygyService.Instance;
        string[] codes = ["id", "asp", "lmrd"];

        // (pieces, fen used for the native probe) - the clock is read back from history after the shuffle.
        (string Pieces, string Fen)[] cases =
        [
            ("Ka1 Qd1 ke8", "4k3/8/8/8/8/8/8/K2Q4 w - - {0} 1"),
            ("Ka1 Rd1 ke8", "4k3/8/8/8/8/8/8/K2R4 w - - {0} 1"),
        ];

        foreach (var (pieces, fen) in cases)
        {
            foreach (string code in codes)
            {
                Setup(pieces, false, false, false, false);
                if (!Shuffle4()) { Check($"Wrapper {code}: shuffle set-up for {pieces}", false); continue; }

                var native = service.ProbeRoot(Parse(string.Format(fen, _history.GetReversibleMovesCount())));
                var strategy = NewStrategy(code, 4);
                var r = strategy.GetResult();
                Check($"Wrapper {code}: root move equals native DTZ move ({pieces}, value {r.Value})",
                    r.Move != null && native.IsValid && r.Move.From == native.From && r.Move.To == native.To
                    && r.Value == TablebaseWin - native.Dtz);
            }
        }

        // Cursed win at clock 99 must score 0 through every wrapper.
        string[] cycle = ["a1b1", "h8g8", "b1a1", "g8h8"];
        foreach (string code in codes)
        {
            Setup("Ka1 kh8 qd5", false, false, false, false);
            for (int i = 0; i < 96; i++) Play(cycle[i % 4]);
            Play("a1b1", "h8g8", "b1a1");
            var r = NewStrategy(code, 4).GetResult();
            Check($"Wrapper {code}: cursed win at clock 99 scores 0", _history.GetReversibleMovesCount() == 99 && r.Move != null && r.Value == 0);
        }
    }

    private static void BenchmarkTests()
    {
        var service = SyzygyService.Instance;
        var original = Boot.GetService<ITablebaseService>();
        (string Name, string Pieces, string Fen, short Depth)[] positions =
        [
            ("KQvK", "Ka1 Qd1 ke8", "4k3/8/8/8/8/8/8/K2Q4 w - - {0} 1", 8),
            ("KRvK", "Ka1 Rd1 ke8", "4k3/8/8/8/8/8/8/K2R4 w - - {0} 1", 8),
            ("KBNvK", "Ka1 Bc1 Ne2 ke8", "4k3/8/8/8/8/8/4N3/K1B5 w - - {0} 1", 8),
            ("KPvK", "Ka1 Pb5 ke8", "4k3/8/8/1P6/8/8/8/K7 w - - {0} 1", 8),
            ("KRvKB", "Ka1 Rd1 ke8 bf8", "4kb2/8/8/8/8/8/8/K2R4 w - - {0} 1", 8),
            ("KQvKR", "Ka1 Qd1 ke8 rh8", "4k2r/8/8/8/8/8/8/K2Q4 w - - {0} 1", 8),
        ];

        Console.WriteLine($"{"Position",-8} {"Mode",-4} {"ms",8} {"Value",8}  Move");
        foreach (var (name, pieces, fen, depth) in positions)
        {
            Setup(pieces, false, false, false, false);
            if (!Shuffle4()) { Check($"Bench {name}: shuffle set-up", false); continue; }
            string nativeFen = string.Format(fen, _history.GetReversibleMovesCount());
            var native = service.ProbeRoot(Parse(nativeFen));

            SetSyzygy(true);
            var onStrategy = NewStrategy("id", depth);
            var sw = Stopwatch.StartNew();
            var on = onStrategy.GetResult();
            sw.Stop();
            long onMs = sw.ElapsedMilliseconds;

            SetSyzygy(false);
            var offStrategy = NewStrategy("id", depth);
            sw.Restart();
            var off = offStrategy.GetResult();
            sw.Stop();
            long offMs = sw.ElapsedMilliseconds;
            SetSyzygy(true);

            Console.WriteLine($"{name,-8} {"on",-4} {onMs,8} {on.Value,8}  {on.Move}");
            Console.WriteLine($"{name,-8} {"off",-4} {offMs,8} {off.Value,8}  {off.Move}");

            bool onOk = on.Move != null && native.IsValid && on.Move.From == native.From && on.Move.To == native.To;
            Check($"Bench {name}: probing-on move equals native DTZ move", onOk);
            Check($"Bench {name}: probing-on is not slower than 2x plain search ({onMs} vs {offMs} ms)", onMs <= offMs * 2 + 50);

        }
    }
}
