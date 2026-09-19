using DataAccess.Interfaces;
using DataAccess.Models;

namespace Engine.Dal.Interfaces;

/// <summary>
/// Service interface for managing game history records (legacy GameDbService behavior)
/// Supports aggregating move outcomes by 128-bit position hash
/// </summary>
public interface IGameHistoryService : IDbService
{
    /// <summary>
    /// Get aggregated move outcomes for a given position hash
    /// </summary>
    /// <param name="hash">128-bit position hash</param>
    /// <returns>HistoryValue containing next move outcomes</returns>
    HistoryValue Get(UInt128 hash);

    /// <summary>
    /// Update history records based on game outcome
    /// Uses current move sequence from MoveHistoryService
    /// </summary>
    /// <param name="value">Game outcome (WhiteWin, Draw, or BlackWin)</param>
    void UpdateHistory(GameValue value);
}
