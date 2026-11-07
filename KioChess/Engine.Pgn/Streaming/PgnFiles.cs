using ZstdSharp;

namespace Engine.Pgn.Streaming;

/// <summary>
/// Locates PGN sources (single file or folder) and opens plain or Zstandard-compressed files.
/// </summary>
public static class PgnFiles
{
    public static bool IsCompressed(string path) =>
        path.EndsWith(".zst", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns the file itself, or every *.pgn and *.pgn.zst file when <paramref name="path"/> is a folder.
    /// </summary>
    public static string[] Enumerate(string path)
    {
        if (Directory.Exists(path))
        {
            return Directory.EnumerateFiles(path)
                .Where(f => f.EndsWith(".pgn", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".pgn.zst", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        return File.Exists(path) ? new[] { path } : Array.Empty<string>();
    }

    /// <summary>
    /// Creates a text writer; the output is Zstandard-compressed when the path ends with .zst.
    /// </summary>
    public static StreamWriter CreateWriter(string path, int compressionLevel = 6)
    {
        Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20);
        if (IsCompressed(path)) stream = new CompressionStream(stream, compressionLevel, 1 << 20, leaveOpen: false);
        return new StreamWriter(stream, new System.Text.UTF8Encoding(false), 1 << 16) { NewLine = "\n" };
    }

    /// <summary>
    /// Opens a file for reading.
    /// </summary>
    internal static Stream Open(string path, out FileStream raw)
    {
        raw = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 14, FileOptions.SequentialScan);
        if (!IsCompressed(path)) return raw;

        var decompression = new DecompressionStream(raw, 1 << 14, leaveOpen: true);
        decompression.SetParameter(ZstdSharp.Unsafe.ZSTD_dParameter.ZSTD_d_windowLogMax, 31);
        return decompression;
    }
}
