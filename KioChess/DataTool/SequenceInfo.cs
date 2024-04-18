
using DataAccess.Entities;

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
    public string Sequence { get;  set; }
}

class DebutInfo
{
    public string Sequence { get; set; }
    public Debut Debut { get; set; }
    public string Moves { get; set; }
}
