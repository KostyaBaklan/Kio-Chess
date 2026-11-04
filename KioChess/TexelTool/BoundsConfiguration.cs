namespace TexelTool;

public class BoundsConfiguration
{
    public int KingSafetyMaxValue { get; set; }
    public int PassedPawnMaxValue { get; set; }
    public int ScalarMinValue { get; set; }
    public int ScalarMaxValue { get; set; }
    public int PstMinValue { get; set; }
    public int PstMaxValue { get; set; }
    public PieceRangesConfiguration PieceRanges { get; set; } = new();
    public PieceRulesConfiguration PieceRules { get; set; } = new();
}

public class ValueRange
{
    public int Min { get; set; }
    public int Max { get; set; }
}

public class PieceRangesConfiguration
{
    public ValueRange Knight { get; set; } = new() { Min = 260, Max = 400 };
    public ValueRange Bishop { get; set; } = new() { Min = 260, Max = 400 };
    public ValueRange Rook { get; set; } = new() { Min = 420, Max = 620 };
    public ValueRange Queen { get; set; } = new() { Min = 850, Max = 1100 };
}

public class PieceRulesConfiguration
{
    public bool Enabled { get; set; } = true;
    public int MaxMinorDifference { get; set; } = 50;
}
