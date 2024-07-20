class Sequence :IEquatable<Sequence>
{
    public Sequence()
    {
        Moves = new List<string>();
    }

    public Sequence(List<string> moves)
    {
        Moves = moves;
    }

    public List<string> Moves { get; set; }

    public bool Equals(Sequence other)
    {
        if(Moves.Count!= other.Moves.Count) return false;

        for (int i = 0; i < Moves.Count; i++)
        {
            if (Moves[i] != other.Moves[i]) return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        int code = 0;
        for (int i = 0; i < Moves.Count; i++)
        {
            code ^= Moves[i].GetHashCode();
        }

        return code;
    }

    public override string ToString() => string.Join(' ', Moves);
}
