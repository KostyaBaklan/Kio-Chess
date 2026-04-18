namespace Analysis.Core.Models;

/// <summary>
/// A named Stockfish difficulty profile.
/// <para>
/// Strength is controlled through both <see cref="SkillLevel"/> and <see cref="SearchDepth"/>.
/// </para>
/// <para>
/// <b>Skill Level (0-20)</b>: Introduces randomness in move selection. At level 0, 
/// Stockfish may play the 3rd-5th best move. At level 20, it always plays the best.
/// </para>
/// <para>
/// <b>Search Depth</b>: Critical for weak play! Even with Skill Level 0, if depth is high,
/// the "random" moves are still very strong. Low depth prevents finding deep tactics.
/// </para>
/// </summary>
public sealed class EloProfile
{
    public string Name          { get; init; } = string.Empty;
    public int    SkillLevel    { get; init; }
    public int    SearchDepth   { get; init; } = 10;
    public int    ThinkTimeMs   { get; init; } = 1000;
    
    /// <summary>
    /// Approximate ELO for display purposes.
    /// </summary>
    public int DisplayElo { get; init; }

    public string DisplayLabel => $"{Name} (~{DisplayElo})";

    public override string ToString() => DisplayLabel;

    /// <summary>
    /// Pre-defined difficulty profiles.
    /// <para>
    /// Key insight: BOTH Skill Level AND Depth must be low for weak play.
    /// High depth + low skill = still strong play (just occasionally picks 2nd best strong move).
    /// Low depth + low skill = actually weak play (can't see tactics + random selection).
    /// </para>
    /// </summary>
    public static readonly IReadOnlyList<EloProfile> Presets =
    [
        // Very weak: Low skill + minimal depth = blind to tactics
        // Depth 1-2: Only sees immediate captures, no tactics
        new() { Name = "Beginner",       SkillLevel =  0, SearchDepth =  1, ThinkTimeMs =  200, DisplayElo =  800 },
        new() { Name = "Casual",         SkillLevel =  1, SearchDepth =  2, ThinkTimeMs =  300, DisplayElo =  950 },
        new() { Name = "Novice",         SkillLevel =  2, SearchDepth =  2, ThinkTimeMs =  400, DisplayElo = 1050 },
        
        // Weak: Low skill + shallow depth = misses 2+ move tactics
        // Depth 3-4: Sees simple 1-2 move tactics only
        new() { Name = "Improving",      SkillLevel =  3, SearchDepth =  3, ThinkTimeMs =  500, DisplayElo = 1150 },
        new() { Name = "Intermediate",   SkillLevel =  5, SearchDepth =  4, ThinkTimeMs =  600, DisplayElo = 1300 },
        new() { Name = "Experienced",    SkillLevel =  7, SearchDepth =  5, ThinkTimeMs =  700, DisplayElo = 1450 },
        
        // Club level: Moderate skill + moderate depth = solid but imperfect
        // Depth 6-8: Sees most standard tactics
        new() { Name = "Club Player",    SkillLevel =  9, SearchDepth =  6, ThinkTimeMs =  800, DisplayElo = 1550 },
        new() { Name = "Strong Club",    SkillLevel = 11, SearchDepth =  7, ThinkTimeMs = 1000, DisplayElo = 1700 },
        new() { Name = "Advanced",       SkillLevel = 13, SearchDepth =  8, ThinkTimeMs = 1500, DisplayElo = 1850 },
        
        // Expert level: High skill + good depth = strong tactical vision
        // Depth 10-14: Sees complex combinations
        new() { Name = "Tournament",     SkillLevel = 15, SearchDepth = 10, ThinkTimeMs = 2000, DisplayElo = 2000 },
        new() { Name = "Expert",         SkillLevel = 17, SearchDepth = 12, ThinkTimeMs = 2500, DisplayElo = 2200 },
        new() { Name = "National Master",SkillLevel = 18, SearchDepth = 14, ThinkTimeMs = 3000, DisplayElo = 2350 },
        
        // Master level: Near-perfect skill + deep search
        // Depth 16+: Full tactical awareness
        new() { Name = "FIDE Master",    SkillLevel = 19, SearchDepth = 16, ThinkTimeMs = 3500, DisplayElo = 2450 },
        new() { Name = "Intl. Master",   SkillLevel = 20, SearchDepth = 18, ThinkTimeMs = 4000, DisplayElo = 2550 },
        new() { Name = "Grandmaster",    SkillLevel = 20, SearchDepth = 20, ThinkTimeMs = 5000, DisplayElo = 2700 },
        new() { Name = "Super GM",       SkillLevel = 20, SearchDepth = 24, ThinkTimeMs = 7000, DisplayElo = 2850 },
        
        // Maximum: Full depth search
        new() { Name = "Maximum",        SkillLevel = 20, SearchDepth = 30, ThinkTimeMs = 10000, DisplayElo = 3500 },
    ];
}
