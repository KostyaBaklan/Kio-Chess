namespace Analysis.Core.Models;

/// <summary>
/// A named Stockfish difficulty profile.
/// <para>
/// Strength is controlled through <see cref="TargetElo"/> using Stockfish's built-in
/// UCI_LimitStrength feature (Stockfish 12+).
/// </para>
/// <para>
/// <b>UCI_LimitStrength + UCI_Elo</b>: Stockfish automatically adjusts its search depth,
/// evaluation pruning, and move selection to match the target ELO rating.
/// This provides consistent and accurate playing strength across all position types.
/// </para>
/// <para>
/// <b>ELO Range</b>: 1320-3190 (Stockfish's supported range)
/// For ratings below 1320, we scale down to 800 using reduced movetime.
/// </para>
/// </summary>
public sealed class EloProfile
{
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Target ELO rating for the engine to play at.
    /// Range: 800-3190 (Stockfish supports 1320-3190, lower values use reduced time).
    /// </summary>
    public int TargetElo { get; init; } = 1500;

    /// <summary>
    /// Time in milliseconds the engine should think per move.
    /// Shorter times = weaker play for low ELO levels.
    /// </summary>
    public int ThinkTimeMs { get; init; } = 1000;

    public string DisplayLabel => $"{Name} ({TargetElo})";

    public override string ToString() => DisplayLabel;

    /// <summary>
    /// Pre-defined difficulty profiles using direct ELO ratings.
    /// Stockfish 12+ uses UCI_LimitStrength + UCI_Elo for accurate strength control.
    /// Profiles range from 800 (beginner) to 3190 (maximum Stockfish).
    /// </summary>
    public static readonly IReadOnlyList<EloProfile> Presets =
    [
        // Beginner levels: 800-1200 (uses reduced UCI_Elo + short movetime)
        new() { Name = "Beginner",       TargetElo =  800, ThinkTimeMs =  300 },
        new() { Name = "Novice",         TargetElo =  950, ThinkTimeMs =  400 },
        new() { Name = "Casual",         TargetElo = 1100, ThinkTimeMs =  500 },
        
        // Intermediate levels: 1250-1600 (Stockfish native UCI_Elo range starts at 1320)
        new() { Name = "Improving",      TargetElo = 1250, ThinkTimeMs =  600 },
        new() { Name = "Intermediate",   TargetElo = 1400, ThinkTimeMs =  750 },
        new() { Name = "Club Player",    TargetElo = 1550, ThinkTimeMs = 1000 },
        
        // Advanced levels: 1700-2000
        new() { Name = "Strong Club",    TargetElo = 1700, ThinkTimeMs = 1000 },
        new() { Name = "Advanced",       TargetElo = 1850, ThinkTimeMs = 1250 },
        new() { Name = "Expert",         TargetElo = 2000, ThinkTimeMs = 1500 },
        
        // Master levels: 2150-2450
        new() { Name = "Candidate Master", TargetElo = 2150, ThinkTimeMs = 1750 },
        new() { Name = "FIDE Master",    TargetElo = 2300, ThinkTimeMs = 2000 },
        new() { Name = "Int. Master",    TargetElo = 2450, ThinkTimeMs = 2500 },
        
        // Elite levels: 2600-3190
        new() { Name = "Grandmaster",    TargetElo = 2600, ThinkTimeMs = 3000 },
        new() { Name = "Super GM",       TargetElo = 2750, ThinkTimeMs = 4000 },
        new() { Name = "World Class",    TargetElo = 2900, ThinkTimeMs = 5000 },
        new() { Name = "Maximum",        TargetElo = 3190, ThinkTimeMs = 6000 },  // Stockfish max UCI_Elo
    ];
}
