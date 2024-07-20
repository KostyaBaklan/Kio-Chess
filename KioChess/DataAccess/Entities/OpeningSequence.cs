namespace DataAccess.Entities;

public class OpeningSequence
{
    public int Id { get; set; }

    public string Sequence { get; set; }

    public int OpeningVariationId { get; set; }

    public virtual OpeningVariation OpeningVariation { get; set; }
}
