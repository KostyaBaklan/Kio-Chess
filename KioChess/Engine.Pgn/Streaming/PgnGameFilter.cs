namespace Engine.Pgn.Streaming;

public enum TimeControlClass
{
    UltraBullet = 0,
    Bullet = 1,
    Blitz = 2,
    Rapid = 3,
    Classical = 4,
    None = 5
}

/// <summary>
/// Accepts a game when its time-control class has a rule and min(WhiteElo, BlackElo) reaches that rule's Elo.
/// Classes without a rule are rejected.
/// </summary>
public sealed class PgnGameFilter
{
    private readonly int[] _minElo = new int[6];

    public PgnGameFilter()
    {
        Array.Fill(_minElo, int.MaxValue);
    }

    public PgnGameFilter Set(TimeControlClass timeControl, int minElo)
    {
        _minElo[(int)timeControl] = minElo;
        return this;
    }

    public bool Accepts(int minPlayerElo, string timeControl) =>
        minPlayerElo >= _minElo[(int)Classify(timeControl)];

    /// <summary>
    /// Lichess classes by base + 40 * increment: under 30 s UltraBullet, under 180 s Bullet, under 480 s Blitz,
    /// under 1500 s Rapid, otherwise Classical. A missing or unparseable TimeControl is None.
    /// </summary>
    public static TimeControlClass Classify(string timeControl)
    {
        if (string.IsNullOrEmpty(timeControl) || timeControl == "-") return TimeControlClass.None;

        var plus = timeControl.IndexOf('+');
        var baseText = plus < 0 ? timeControl : timeControl.Substring(0, plus);
        if (!int.TryParse(baseText, out var baseSeconds)) return TimeControlClass.None;

        int increment = plus >= 0 && int.TryParse(timeControl.AsSpan(plus + 1), out var inc) ? inc : 0;
        int estimated = baseSeconds + 40 * increment;

        if (estimated < 30) return TimeControlClass.UltraBullet;
        if (estimated < 180) return TimeControlClass.Bullet;
        if (estimated < 480) return TimeControlClass.Blitz;
        if (estimated < 1500) return TimeControlClass.Rapid;
        return TimeControlClass.Classical;
    }
}
