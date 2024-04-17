
class SequenceInfo
{
    public string Name { get; set; }
    public string Sequence { get; set; }
    public string Code { get; set; }

    public override string ToString()
    {
        return Name;
    }
}

class SequenceItem
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string Moves { get; set; }
}
