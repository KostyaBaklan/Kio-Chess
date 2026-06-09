namespace GamesServices;

/// <summary>
/// Service for processing game sequences and batch writing to games.db
/// </summary>
public interface ISequenceService
{
    void Initialize();

    void Save();

    /// <summary>
    /// Process game sequence with 128-bit hash support
    /// </summary>
    void ProcessSequence(byte[] sequences);
}