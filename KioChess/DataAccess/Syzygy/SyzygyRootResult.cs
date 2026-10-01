namespace DataAccess.Syzygy
{
    public readonly struct SyzygyRootResult
    {
        public const int PromotionNone = 0;
        public const int PromotionQueen = 1;
        public const int PromotionRook = 2;
        public const int PromotionBishop = 3;
        public const int PromotionKnight = 4;

        public readonly bool IsValid;
        public readonly TbResult Wdl;
        public readonly int From;
        public readonly int To;
        public readonly int Promotion;
        public readonly bool IsEnPassant;
        public readonly int Dtz;

        public SyzygyRootResult(bool isValid, TbResult wdl, int from, int to, int promotion, bool isEnPassant, int dtz)
        {
            IsValid = isValid;
            Wdl = wdl;
            From = from;
            To = to;
            Promotion = promotion;
            IsEnPassant = isEnPassant;
            Dtz = dtz;
        }

        public static SyzygyRootResult Invalid => new SyzygyRootResult(false, TbResult.Draw, 0, 0, 0, false, 0);
    }
}
