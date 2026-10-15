namespace Engine.Models.Moves;

public sealed class NullMove : MoveBase
{
    public static readonly NullMove Instance = new() { Key = -1 };

    public override bool IsLegal() => true;

    public override void Make() { }

    public override void UnMake() { }
}
