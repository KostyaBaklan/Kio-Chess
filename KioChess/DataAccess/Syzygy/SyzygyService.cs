namespace DataAccess.Syzygy
{
    /// <summary>
    /// Fathom is not thread-safe and can be initialized only once per process,
    /// so calls are serialized and the service is a process-wide singleton.
    /// </summary>
    public sealed class SyzygyService : ISyzygyService
    {
        private const uint WdlMask = 0xF;
        private const uint ToMask = 0x3F0;
        private const uint FromMask = 0xFC00;
        private const uint PromotesMask = 0x70000;
        private const uint EpMask = 0x80000;
        private const uint DtzMask = 0xFFF00000;
        private const uint ResultFailed = 0xFFFFFFFF;
        private const uint ResultCheckmate = 4;
        private const uint ResultStalemate = 2;

        private static readonly object Sync = new object();
        private static readonly SyzygyService _instance = new SyzygyService();

        private bool _initialized;
        private bool _disposed;

        private SyzygyService() { }

        public static SyzygyService Instance => _instance;

        public bool IsInitialized => _initialized;

        public int LargestTable { get; private set; }

        public bool Initialize(string dllPath, string tablesPath)
        {
            lock (Sync)
            {
                if (_initialized) return true;
                if (_disposed) return false;
                if (!File.Exists(dllPath)) return false;

                FathomNative.SetLibraryPath(dllPath);

                if ((FathomNative.tb_init_(tablesPath) & 0xFF) != 1) return false;

                LargestTable = FathomNative.get_largest();
                _initialized = LargestTable > 0;
                return _initialized;
            }
        }

        public bool CanProbe(in SyzygyPosition position)
        {
            return _initialized && position.Castling == 0 && position.PieceCount <= LargestTable;
        }

        public bool TryProbeWdl(in SyzygyPosition p, out TbResult result)
        {
            result = TbResult.Draw;
            if (!CanProbe(p) || p.Rule50 != 0) return false;

            uint res;
            lock (Sync)
            {
                // Native tb_probe_wdl rejects a nonzero clock; zero is passed only because the guard above proved it.
                res = FathomNative.tb_probe_wdl_(p.White, p.Black, p.Kings, p.Queens, p.Rooks, p.Bishops,
                    p.Knights, p.Pawns, p.Rule50, p.Castling, p.EnPassant, p.WhiteToMove);
            }

            if (res == ResultFailed) return false;

            result = (TbResult)res;
            return true;
        }

        public SyzygyRootResult ProbeRoot(in SyzygyPosition p)
        {
            if (!CanProbe(p)) return SyzygyRootResult.Invalid;

            uint res;
            lock (Sync)
            {
                res = FathomNative.tb_probe_root_(p.White, p.Black, p.Kings, p.Queens, p.Rooks, p.Bishops,
                    p.Knights, p.Pawns, p.Rule50, p.Castling, p.EnPassant, p.WhiteToMove, IntPtr.Zero);
            }

            if (res == ResultFailed || res == ResultCheckmate || res == ResultStalemate)
                return SyzygyRootResult.Invalid;

            return new SyzygyRootResult(
                true,
                (TbResult)(res & WdlMask),
                (int)((res & FromMask) >> 10),
                (int)((res & ToMask) >> 4),
                (int)((res & PromotesMask) >> 16),
                (res & EpMask) != 0,
                (int)((res & DtzMask) >> 20));
        }

        public void Dispose()
        {
            lock (Sync)
            {
                if (!_initialized) return;
                FathomNative.tb_free_();
                _initialized = false;
                _disposed = true;
            }
        }
    }
}


