namespace DataAccess.Entities;

public class PopularPosition
{
    public string History { get; set; }
    public short NextMove { get; set; }
    public short Value { get; set; }
}

public class PopularPositions
{
    public List<PopularPosition> Popular { get; set; }
    public List<VeryPopularPosition> VeryPopular { get; set; }
}
