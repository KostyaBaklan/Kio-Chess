class Opening
{
    public string Name { get; set; }
    public string Variation { get; set; }
    public List<string> Moves { get; set; }

    public OpeningVariation ToVariation() => new OpeningVariation { Name = Name, Variation = Variation };
}
