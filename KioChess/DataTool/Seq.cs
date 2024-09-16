using Engine.Services;

class Seq:IComparable<Seq>
{
    private readonly MoveProvider _mp;

    public short White;
    public short Black;
    public int Total;

    public Seq(MoveProvider moveProvider)
    {
        _mp = moveProvider;
    }

    public int CompareTo(Seq other) => other.Total.CompareTo(Total);

    public override string ToString() => $"{_mp.Get(White).ToLightString()}-{_mp.Get(Black).ToLightString()} {Total}";

    public string ToSequence() => $"{White}-{Black}";
}
