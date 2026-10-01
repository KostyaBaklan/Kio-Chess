namespace DataAccess.Syzygy
{
    public interface ISyzygyService : IDisposable
    {
        bool IsInitialized { get; }
        int LargestTable { get; }
        bool Initialize(string dllPath, string tablesPath);
        bool CanProbe(in SyzygyPosition position);
        bool TryProbeWdl(in SyzygyPosition position, out TbResult result);
        SyzygyRootResult ProbeRoot(in SyzygyPosition position);
    }
}
