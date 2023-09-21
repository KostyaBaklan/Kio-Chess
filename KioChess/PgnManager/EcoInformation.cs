class EcoInformation
{
    public EcoInformation(string[] items)
    {
        ECO = items[0];

        var v = items.Skip(2).Take(items.Length - 3).Select(s=>s.Trim('"')).ToList();
        var m = SetMoves(items.Last());

        string variation = string.Join(", ", v);
        Opening = new Opening { Name = items[1].Trim('"'), Variation = variation, Moves = m };

    }

    public string ECO { get; set; }
    public Opening Opening { get; set; }

    internal List<string> SetMoves(string sequence)
    {
        var moves = sequence.Split(" ").Where(p =>
        {
            return !int.TryParse(p, out _);
        }).ToList();

        return moves;
    }
}
