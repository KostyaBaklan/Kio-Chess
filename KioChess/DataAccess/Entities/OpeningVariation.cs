namespace DataAccess.Entities;

public class OpeningVariation
{
    public int Id { get; set; }

    public string Name { get; set; }

    public short OpeningId { get; set; }

    public int VariationId { get; set; }

    public string Moves { get; set; }

    public virtual Opening Opening { get; set; }

    public virtual Variation Variation { get; set; }
}
