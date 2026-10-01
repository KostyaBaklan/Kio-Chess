using DataAccess.Syzygy;
using Engine.Models.Config;
using Engine.Services.Syzygy;

internal static partial class EngineEdgeCaseTests
{
    private static void DegradationTests()
    {
        // A second, never-initialized service instance: the process singleton stays untouched.
        var fresh = (SyzygyService)Activator.CreateInstance(typeof(SyzygyService), true)!;

        bool ok = fresh.Initialize(@"C:\no\such\dir\fathomDll.dll", @"C:\no\such\tables");
        Check("Degrade: missing DLL -> Initialize returns false without throwing", !ok);
        Check("Degrade: missing DLL -> service not initialized, no tables", !fresh.IsInitialized && fresh.LargestTable == 0);

        Setup("Ka1 Qd1 ke8", false, false, false, false);
        Play("a1b1", "e8f8");
        var position = Parse("5k2/8/8/8/8/8/8/1K1Q4 w - - 0 1");
        Check("Degrade: uninitialized service cannot probe", !fresh.CanProbe(position));
        Check("Degrade: uninitialized WDL probe is refused", !fresh.TryProbeWdl(position, out _));
        Check("Degrade: uninitialized root probe is invalid", !fresh.ProbeRoot(position).IsValid);

        var service = new TablebaseService(fresh);
        Check("Degrade: TablebaseService reports disabled with no tables", !service.IsEnabled && service.MaxPieces == 0);
        Check("Degrade: TablebaseService WDL refuses without throwing",
            !service.TryProbeWdl(_board, true, 0, 0, out _));
        Check("Degrade: TablebaseService root refuses without throwing",
            !service.TryProbeRoot(_board, true, 0, 0, out _));

        // Disabled or empty configuration returns the process singleton and leaves it as it was.
        bool before = SyzygyService.Instance.IsInitialized;
        var disabled = SyzygyBootstrapper.Initialize(new SyzygyConfiguration { IsEnabled = false, TablesPath = @"C:\x" });
        var empty = SyzygyBootstrapper.Initialize(new SyzygyConfiguration { IsEnabled = true, TablesPath = "" });
        var none = SyzygyBootstrapper.Initialize(null!);
        Check("Degrade: bootstrapper returns the singleton for disabled/empty/null config",
            ReferenceEquals(disabled, SyzygyService.Instance) && ReferenceEquals(empty, SyzygyService.Instance) && ReferenceEquals(none, SyzygyService.Instance));
        Check("Degrade: bootstrapper with such config does not change state", SyzygyService.Instance.IsInitialized == before);

        // Strategies must run normally when tablebases are switched off.
        SetSyzygy(false);
        Setup("Ka1 Qd1 ke8", false, false, false, false);
        Shuffle4();
        var r = NewStrategy("id", 4).GetResult();
        SetSyzygy(true);
        Check("Degrade: strategy still returns a move with tablebases off", r.Move != null && Math.Abs(r.Value) < TablebaseWin - 1000);
    }
}
