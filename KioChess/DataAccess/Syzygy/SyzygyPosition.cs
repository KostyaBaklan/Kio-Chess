namespace DataAccess.Syzygy
{
    /// <summary>
    /// Bitboard position description for Fathom. Square index: a1 = 0, h8 = 63.
    /// </summary>
    public readonly struct SyzygyPosition
    {
        public readonly ulong White;
        public readonly ulong Black;
        public readonly ulong Kings;
        public readonly ulong Queens;
        public readonly ulong Rooks;
        public readonly ulong Bishops;
        public readonly ulong Knights;
        public readonly ulong Pawns;
        public readonly uint Rule50;
        public readonly uint Castling;
        public readonly uint EnPassant;
        public readonly bool WhiteToMove;

        public SyzygyPosition(ulong white, ulong black, ulong kings, ulong queens, ulong rooks,
            ulong bishops, ulong knights, ulong pawns, uint rule50, uint castling, uint enPassant, bool whiteToMove)
        {
            White = white;
            Black = black;
            Kings = kings;
            Queens = queens;
            Rooks = rooks;
            Bishops = bishops;
            Knights = knights;
            Pawns = pawns;
            Rule50 = rule50;
            Castling = castling;
            EnPassant = enPassant;
            WhiteToMove = whiteToMove;
        }

        public int PieceCount => System.Numerics.BitOperations.PopCount(White | Black);
    }
}
