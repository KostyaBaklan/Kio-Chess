using DataAccess.Syzygy;
using Engine.Models.Boards;

namespace Engine.Services.Syzygy;

public interface ITablebaseService
{
    bool IsEnabled { get; }

    int MaxPieces { get; }

    /// <summary>
    /// Exact zero-clock WDL probe: returns false unless rule50 is 0 (the native WDL API cannot account for a nonzero clock)
    /// or the position has castling rights. enPassant is the Fathom target square or 0.
    /// </summary>
    bool TryProbeWdl(Board board, bool whiteToMove, int rule50, int enPassant, out TbResult wdl);

    /// <summary>
    /// Root probe (WDL + DTZ + best move). The last result is cached, so repeated calls
    /// during iterative deepening do not hit the tables again.
    /// </summary>
    bool TryProbeRoot(Board board, bool whiteToMove, int rule50, int enPassant, out SyzygyRootResult root);
}
