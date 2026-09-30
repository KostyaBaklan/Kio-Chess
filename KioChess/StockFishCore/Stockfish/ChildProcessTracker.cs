using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StockFishCore.Stockfish
{
    /// <summary>
    /// Binds child processes to a Windows Job Object configured with
    /// KILL_ON_JOB_CLOSE, so that every tracked child is terminated by the OS
    /// when this process exits - including on Environment.Exit, unhandled
    /// exceptions, or external termination. Without this, stockfish.exe children
    /// survive their parent and accumulate over long test runs.
    /// </summary>
    internal static class ChildProcessTracker
    {
        private static readonly IntPtr _jobHandle;

        static ChildProcessTracker()
        {
            if (!OperatingSystem.IsWindows()) return;

            _jobHandle = CreateJobObject(IntPtr.Zero, $"ChessTest_{Environment.ProcessId}");
            if (_jobHandle == IntPtr.Zero) return;

            var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
            };

            var extended = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION { BasicLimitInformation = info };

            int length = Marshal.SizeOf<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>();
            IntPtr ptr = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.StructureToPtr(extended, ptr, false);
                SetInformationJobObject(_jobHandle, JobObjectExtendedLimitInformation, ptr, (uint)length);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static void AddProcess(Process process)
        {
            if (_jobHandle == IntPtr.Zero || process == null) return;
            try { AssignProcessToJobObject(_jobHandle, process.Handle); }
            catch { /* best effort */ }
        }

        private const int JobObjectExtendedLimitInformation = 9;
        private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string name);

        [DllImport("kernel32.dll")]
        private static extern bool SetInformationJobObject(IntPtr job, int infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport("kernel32.dll")]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_COUNTERS
        {
            public ulong ReadOperationCount, WriteOperationCount, OtherOperationCount;
            public ulong ReadTransferCount, WriteTransferCount, OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public long PerProcessUserTimeLimit, PerJobUserTimeLimit;
            public uint LimitFlags;
            public UIntPtr MinimumWorkingSetSize, MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass, SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit, JobMemoryLimit, PeakProcessMemoryUsed, PeakJobMemoryUsed;
        }
    }
}