namespace Analysis.DataAccess.Entities;

/// <summary>
/// Represents a chess opening with its move sequence and metadata.
/// Designed for tree-based navigation with parent-child relationships.
/// </summary>
public class OpeningEntry
{
    /// <summary>Unique identifier</summary>
    public int Id { get; set; }
    
    /// <summary>ECO code (e.g., "B90", "C55")</summary>
    public string ECO { get; set; } = string.Empty;
    
    /// <summary>Opening name (e.g., "Sicilian Defense")</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Variation name (e.g., "Najdorf")</summary>
    public string Variation { get; set; }
    
    /// <summary>Sub-variation (e.g., "Poisoned Pawn")</summary>
    public string SubVariation { get; set; }
    
    /// <summary>Full display name including all variations</summary>
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>UCI move sequence from start: "e2e4 c7c5 g1f3"</summary>
    public string MovesUCI { get; set; } = string.Empty;
    
    /// <summary>SAN move sequence: "1. e4 c5 2. Nf3"</summary>
    public string MovesSAN { get; set; } = string.Empty;
    
    /// <summary>Number of half-moves (ply) in this opening</summary>
    public int MoveCount { get; set; }
    
    /// <summary>FEN position after all moves</summary>
    public string FEN { get; set; } = string.Empty;
    
    /// <summary>Position key for fast lookup (move sequence based)</summary>
    public string PositionKey { get; set; } = string.Empty;
    
    /// <summary>Parent opening ID (null for root moves like 1. e4)</summary>
    public int? ParentId { get; set; }
    
    /// <summary>Popularity score (0-100, calculated from usage)</summary>
    public int Popularity { get; set; }
    
    /// <summary>Is this a main line or a side variation?</summary>
    public bool IsMainLine { get; set; }
    
    /// <summary>Navigation: Parent opening</summary>
    public virtual OpeningEntry Parent { get; set; }
    
    /// <summary>Navigation: Child variations</summary>
    public virtual ICollection<OpeningEntry> Children { get; set; } = new HashSet<OpeningEntry>();
}
