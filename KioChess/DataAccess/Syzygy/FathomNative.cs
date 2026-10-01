using System.Reflection;
using System.Runtime.InteropServices;

namespace DataAccess.Syzygy
{
    internal static class FathomNative
    {
        private const string LibraryName = "fathomDll";

        private static string _libraryPath;
        private static bool _resolverSet;

        public static void SetLibraryPath(string path)
        {
            _libraryPath = path;
            if (_resolverSet) return;

            NativeLibrary.SetDllImportResolver(typeof(FathomNative).Assembly, Resolve);
            _resolverSet = true;
        }

        private static IntPtr Resolve(string name, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (name == LibraryName && !string.IsNullOrEmpty(_libraryPath))
            {
                return NativeLibrary.Load(_libraryPath);
            }
            return IntPtr.Zero;
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int tb_init_(string path);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tb_free_();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int get_largest();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint tb_probe_wdl_(
            ulong white, ulong black,
            ulong kings, ulong queens,
            ulong rooks, ulong bishops,
            ulong knights, ulong pawns,
            uint rule50, uint castling,
            uint ep, bool stm);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint tb_probe_root_(
            ulong white, ulong black,
            ulong kings, ulong queens,
            ulong rooks, ulong bishops,
            ulong knights, ulong pawns,
            uint rule50, uint castling,
            uint ep, bool stm,
            IntPtr results);
    }
}
