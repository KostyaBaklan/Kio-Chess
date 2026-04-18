namespace Analysis.Core.Models;


/// <summary>
/// Identifies one of the built-in WPF Path-based piece sets.
/// Each set is pure vector — no external image files needed.
/// </summary>
public sealed class PieceSet
{
    public string Key         { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;

    public static readonly IReadOnlyList<PieceSet> All =
    [
        new() { Key = "Classic",  DisplayName = "Classic"  },
        new() { Key = "Modern",   DisplayName = "Modern"   },
        new() { Key = "Minimal",  DisplayName = "Minimal"  },
    ];

    public static PieceSet Default => All[0];

    public override string ToString() => DisplayName;
}

