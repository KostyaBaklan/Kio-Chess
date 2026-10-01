using DataAccess.Syzygy;
using Engine.Models.Boards;

namespace Engine.Services.Syzygy;

public interface ITablebaseService
{
    bool IsEnabled { get; }

    int MaxPieces { get; }

    /// <summary>
    /// Cached WDL probe (halfmove clock assumed 0, no castling rights). Piece-count gating is done internally.
    /// </summary>
    bool TryProbeWdl(Board board, bool whiteToMove, out TbResult wdl);

    /// <summary>
    /// Root probe (WDL + DTZ + best move). The last result is cached, so repeated calls
    /// during iterative deepening do not hit the tables again.
    /// </summary>
    bool TryProbeRoot(Board board, bool whiteToMove, int rule50, out SyzygyRootResult root);
}
