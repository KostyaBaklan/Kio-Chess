namespace Engine.Models.Enums
{
    public enum MoveType : byte
    {
        PawnPromotionToQueen = 0,
        PawnPromotionToRook,
        PawnPromotionToKnight,
        PawnPromotionToBishop,
        EatByPawn,
        EatByPawnPromotionToQueen,
        EatByPawnPromotionToRook,
        EatByPawnPromotionToKnight,
        EatByPawnPromotionToBishop,
        EatByBishop,
        EatByKnight,
        EatByRook,
        EatByQueen,
        EatByKing,
        EatOver,
        SmallCastle,
        BigCastle,
        MoveKnight,
        MoveBishop,
        MovePawn,
        MoveQueen,
        MoveRook,
        MoveKing,
        Over
    }
}