using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;

namespace Engine.Interfaces
{
    public interface IMoveProvider
    {
        int MovesCount { get; }
        MoveBase Get(short key);
        IEnumerable<MoveBase> GetAll();
        IEnumerable<AttackBase> GetAttacks(Piece piece, byte cell);
        void GetAttacks(byte piece, byte cell, AttackList attackList);
        void GetPromotions(byte piece, byte cell, PromotionList promotions);
        void GetPromotions(byte piece, byte square, List<PromotionAttackList> promotionsAttackTemp);
        void GetMoves(byte piece, byte cell, MoveList moveList);
        void GetAttacks(Piece piece, byte @from, AttackList attackList);
        //IEnumerable<AttackBase> GetAttacks(Piece piece, int @from);
        bool AnyLegalAttacksTo(Piece piece, byte from, byte to);
        IEnumerable<MoveBase> GetMoves(Piece piece, byte cell);
        void GetWhiteAttacksToForPromotion(byte to, AttackList attackList);
        void GetBlackAttacksToForPromotion(byte to, AttackList attackList);
        BitBoard GetAttackPattern(byte piece, byte position);
        void SetBoard(IBoard board);
        void AgeHistory(); 
        void SaveHistory(MoveBase move);
    }
}