class CountResult
{
    public int Count { get; set; }
    public int Bits { get; set; }
    public double Average { get; set; }

    public override string ToString() => $"Size = {Count},Bits = {Bits},Relation = {Average}";
}
