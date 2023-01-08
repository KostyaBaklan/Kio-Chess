using Engine.DataStructures.Moves;
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
        IEnumerable<AttackBase> GetAttacks(Piece piece, Square cell);
        void GetAttacks(byte piece, Square cell, AttackList attackList);
        void GetAttacks(Piece piece, byte @from, AttackList attackList);
        //IEnumerable<AttackBase> GetAttacks(Piece piece, int @from);
        bool AnyLegalAttacksTo(Piece piece, Square from, byte to);
        IEnumerable<AttackBase> GetAttacks(Piece piece, byte from, byte to);
        IEnumerable<MoveBase> GetMoves(Piece piece, Square cell);
        void GetMoves(byte piece, Square cell, MoveList moveList);
        bool AnyBlackCheck();
        bool AnyWhiteCheck();
        bool IsUnderAttack(byte piece, byte to);
        void GetWhiteAttacksTo(byte to, AttackList attackList);
        void GetBlackAttacksTo(byte to, AttackList attackList);
        BitBoard GetAttackPattern(byte piece, byte position);
        bool IsWhiteUnderAttack(Square square);
        bool IsBlackUnderAttack(Square square);
        void SetBoard(IBoard board);
        void AgeHistory();
        MoveBase GetMoveTo(Piece whiteKnight, Square from, Square to);
        IEnumerable<MoveBase> GetPossibleMoves(Piece whiteKing, Square from);
    }
}