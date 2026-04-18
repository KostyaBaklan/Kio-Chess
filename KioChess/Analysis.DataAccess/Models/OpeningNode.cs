using Analysis.DataAccess.Entities;

namespace Analysis.DataAccess.Models;

/// <summary>
/// Represents a navigable node in the opening tree with full context.
/// </summary>
public class OpeningNode
{
    public int Id { get; set; }
    public string ECO { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Variation { get; set; } = string.Empty;
    
    public List<string> MovesUCI { get; set; } = new();
    public string MovesSAN { get; set; } = string.Empty;
    public int MoveCount { get; set; }
    
    public string PositionKey { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    
    public int Popularity { get; set; }
    public bool IsMainLine { get; set; }
    
    public List<OpeningNode> NextMoves { get; set; } = new();
    
    public static OpeningNode FromEntity(OpeningEntry entity)
    {
        return new OpeningNode
        {
            Id = entity.Id,
            ECO = entity.ECO,
            Name = entity.Name,
            FullName = entity.FullName,
            Variation = entity.Variation ?? string.Empty,
            MovesUCI = entity.MovesUCI.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList(),
            MovesSAN = entity.MovesSAN,
            MoveCount = entity.MoveCount,
            PositionKey = entity.PositionKey,
            ParentId = entity.ParentId,
            Popularity = entity.Popularity,
            IsMainLine = entity.IsMainLine
        };
    }
}

/// <summary>
/// Represents a possible next move in the opening tree.
/// </summary>
public class OpeningMove
{
    public string MoveUCI { get; set; } = string.Empty;
    public string MoveSAN { get; set; } = string.Empty;
    public string OpeningName { get; set; } = string.Empty;
    public string ECO { get; set; } = string.Empty;
    public int Popularity { get; set; }
    public bool IsMainLine { get; set; }
    public int OpeningId { get; set; }
}
