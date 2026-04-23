namespace GamesServices;

public interface ISequenceService
{
    void Initialize();

    void Save();

    void ProcessSequence(byte[] sequences);
}