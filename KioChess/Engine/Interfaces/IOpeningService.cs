namespace Engine.Interfaces;

public interface IOpeningService
{
    IDictionary<string, ICollection<string>> GetSequences();
    IEnumerable<ICollection<short>> GetMoveKeys();
}
