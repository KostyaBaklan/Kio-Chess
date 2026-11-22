using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Services;

namespace TuningCommon.Data
{
    public static class PositionHash
    {
        public static int EnPassantFile(MoveHistoryService history)
        {
            var last = history.GetPly() < 0 ? null : history.GetLastMove();
            if (last == null) return 8;
            bool pawn = last.Piece == Pieces.WhitePawn || last.Piece == Pieces.BlackPawn;
            return pawn && Math.Abs(last.To - last.From) == 16 ? last.To & 7 : 8;
        }

        public static ulong Compute(Board board, byte castle, bool whiteToMove, int enPassantFile)
        {
            ulong hash = 0xcbf29ce484222325UL ^ castle ^ ((ulong)enPassantFile << 8) ^ (whiteToMove ? 0x9E3779B97F4A7C15UL : 0UL);
            for (byte p = 0; p < 12; p++)
            {
                hash = (hash ^ board.GetPieceBits(p)) * 0x100000001b3UL;
                hash ^= hash >> 29;
                hash *= 0xbf58476d1ce4e5b9UL;
            }

            return hash;
        }
    }
}
