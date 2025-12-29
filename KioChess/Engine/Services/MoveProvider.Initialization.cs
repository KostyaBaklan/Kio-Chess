using Engine.DataStructures.Moves.Arrays;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards.Buffers;
using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Engine.Services;

public partial class MoveProvider
{
    private void SetPieceAttackPatterns(ref CellBuffer<BitBoard> buffer, BitBoard[] attacks)
    {
        for (byte i = 0; i < _squaresNumber; i++)
        {
            buffer[i] = attacks[i];
        }
    }

    private void SetPawnOver()
    {
        var overAttacks = _all.OfType<PawnOverAttack>().ToList();
        var whiteOvers = _all.OfType<PawnOverWhiteMove>()
            .ToDictionary(k => k.To);
        var blackOvers = _all.OfType<PawnOverBlackMove>()
            .ToDictionary(k => k.To);
        foreach (var pawnOverAttack in overAttacks)
        {
            var to = pawnOverAttack.To;
            if (pawnOverAttack.Piece == Pieces.WhitePawn)
            {
                byte enPassantSquare = (byte)(to - 8);
                pawnOverAttack.EnPassant = blackOvers[enPassantSquare];
            }
            else if (pawnOverAttack.Piece == Pieces.BlackPawn)
            {
                byte enPassantSquare = (byte)(to + 8);
                pawnOverAttack.EnPassant = whiteOvers[enPassantSquare];
            }
            else
            {
                throw new Exception("Suka");
            }
        }

        _whitePawnOverAttacks = new PawnOverAttackArray(overAttacks.Where(a => a.Piece == Pieces.WhitePawn));
        _blackPawnOverAttacks = new PawnOverAttackArray(overAttacks.Where(a => a.Piece == Pieces.BlackPawn));
    }

    private void SetAttacks()
    {
        Dictionary<byte, List<AttackBase>> AttacksMap = _all.OfType<AttackBase>().Where(m => m.IsAttack && !m.IsPromotion && !(m is PawnOverAttack))
            .GroupBy(m => m.Piece)
            .ToDictionary(k => k.Key, v => v.ToList());

        _whitePawnAttacks = GenerateAttacks(AttacksMap[Pieces.WhitePawn]);
        _whiteKnightAttacks = GenerateAttacks(AttacksMap[Pieces.WhiteKnight]);
        _whiteBishopAttacks = GenerateAttacks(AttacksMap[Pieces.WhiteBishop]);
        _whiteRookAttacks = GenerateAttacks(AttacksMap[Pieces.WhiteRook]);
        _whiteQueenAttacks = GenerateAttacks(AttacksMap[Pieces.WhiteQueen]);
        _whiteKingAttacks = GenerateAttacks(AttacksMap[Pieces.WhiteKing]);

        _blackPawnAttacks = GenerateAttacks(AttacksMap[Pieces.BlackPawn]);
        _blackKnightAttacks = GenerateAttacks(AttacksMap[Pieces.BlackKnight]);
        _blackBishopAttacks = GenerateAttacks(AttacksMap[Pieces.BlackBishop]);
        _blackRookAttacks = GenerateAttacks(AttacksMap[Pieces.BlackRook]);
        _blackQueenAttacks = GenerateAttacks(AttacksMap[Pieces.BlackQueen]);
        _blackKingAttacks = GenerateAttacks(AttacksMap[Pieces.BlackKing]);
    }

    private void SetPromotions()
    {
        var promotions = _all.Where(m => m.IsPromotion).ToList();

        _whitePromotions = new PromotionListArray(promotions.OfType<PromotionMove>().Where(p => p.Piece == Pieces.WhitePawn));
        _blackPromotions = new PromotionListArray(promotions.OfType<PromotionMove>().Where(p => p.Piece == Pieces.BlackPawn));

        _whitePromotionAttacks = new PromotionAttackListArray(promotions.OfType<WhitePromotionAttack>());
        _blackPromotionAttacks = new PromotionAttackListArray(promotions.OfType<BlackPromotionAttack>());
    }

    private void SetMoves()
    {
        Dictionary<byte, List<MoveBase>> movesMap = _all.Where(m => !m.IsAttack && !m.IsPromotion)
            .GroupBy(m => m.Piece)
            .ToDictionary(k => k.Key, v => v.ToList());

        _whitePawnMoves = GenerateMoves(movesMap[Pieces.WhitePawn]);
        _whiteKnightMoves = GenerateMoves(movesMap[Pieces.WhiteKnight]);
        _whiteBishopMoves = GenerateMoves(movesMap[Pieces.WhiteBishop]);
        _whiteRookMoves = GenerateMoves(movesMap[Pieces.WhiteRook]);
        _whiteQueenMoves = GenerateMoves(movesMap[Pieces.WhiteQueen]);
        _whiteKingMoves = GenerateMoves(movesMap[Pieces.WhiteKing]);

        _blackPawnMoves = GenerateMoves(movesMap[Pieces.BlackPawn]);
        _blackKnightMoves = GenerateMoves(movesMap[Pieces.BlackKnight]);
        _blackBishopMoves = GenerateMoves(movesMap[Pieces.BlackBishop]);
        _blackRookMoves = GenerateMoves(movesMap[Pieces.BlackRook]);
        _blackQueenMoves = GenerateMoves(movesMap[Pieces.BlackQueen]);
        _blackKingMoves = GenerateMoves(movesMap[Pieces.BlackKing]);
    }
    private AttackArray[] GenerateAttacks(List<AttackBase> moveBases)
    {
        var map = moveBases.GroupBy(m => m.From)
            .ToDictionary(k => k.Key, v => v.ToList());

        AttackArray[] moves = new AttackArray[64];

        foreach (var from in map)
        {
            moves[from.Key] = new AttackArray(from.Value);
        }

        return moves;
    }

    private MoveArray[] GenerateMoves(List<MoveBase> moveBases)
    {
        var map = moveBases.GroupBy(m => m.From)
            .ToDictionary(k => k.Key, v => v.ToList());

        MoveArray[] moves = new MoveArray[64];

        foreach (var from in map)
        {
            moves[from.Key] = new MoveArray(from.Value);
        }

        return moves;
    }

    private void SetPawnAttackPatterns()
    {
        _attackPatterns[Pieces.WhitePawn][Squares.A1] = Squares.B2.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.B1] = Squares.A2.AsBitBoard() | Squares.C2.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.C1] = Squares.B2.AsBitBoard() | Squares.D2.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.D1] = Squares.C2.AsBitBoard() | Squares.E2.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.E1] = Squares.D2.AsBitBoard() | Squares.F2.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.F1] = Squares.E2.AsBitBoard() | Squares.G2.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.G1] = Squares.F2.AsBitBoard() | Squares.H2.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.H1] = Squares.G2.AsBitBoard();

        _attackPatterns[Pieces.BlackPawn][Squares.A8] = Squares.B7.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.B8] = Squares.A7.AsBitBoard() | Squares.C7.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.C8] = Squares.B7.AsBitBoard() | Squares.D7.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.D8] = Squares.C7.AsBitBoard() | Squares.E7.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.E8] = Squares.D7.AsBitBoard() | Squares.F7.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.F8] = Squares.E7.AsBitBoard() | Squares.G7.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.G8] = Squares.F7.AsBitBoard() | Squares.H7.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.H8] = Squares.G7.AsBitBoard();

        _attackPatterns[Pieces.WhitePawn][Squares.A7] = Squares.B8.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.B7] = Squares.A8.AsBitBoard() | Squares.C8.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.C7] = Squares.B8.AsBitBoard() | Squares.D8.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.D7] = Squares.C8.AsBitBoard() | Squares.E8.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.E7] = Squares.D8.AsBitBoard() | Squares.F8.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.F7] = Squares.E8.AsBitBoard() | Squares.G8.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.G7] = Squares.F8.AsBitBoard() | Squares.H8.AsBitBoard();
        _attackPatterns[Pieces.WhitePawn][Squares.H7] = Squares.G8.AsBitBoard();

        _attackPatterns[Pieces.BlackPawn][Squares.A2] = Squares.B1.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.B2] = Squares.A1.AsBitBoard() | Squares.C1.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.C2] = Squares.B1.AsBitBoard() | Squares.D1.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.D2] = Squares.C1.AsBitBoard() | Squares.E1.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.E2] = Squares.D1.AsBitBoard() | Squares.F1.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.F2] = Squares.E1.AsBitBoard() | Squares.G1.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.G2] = Squares.F1.AsBitBoard() | Squares.H1.AsBitBoard();
        _attackPatterns[Pieces.BlackPawn][Squares.H2] = Squares.G1.AsBitBoard();
    }

    private void SetAttackPatterns(byte piece, List<List<AttackBase>>[][] _attacksTemp)
    {
        foreach (List<List<AttackBase>> attacks in _attacksTemp[piece])
        {
            if (attacks == null) continue;

            foreach (var moves in attacks)
            {
                foreach (var move in moves)
                {
                    _attackPatterns[piece][move.From] = _attackPatterns[piece][move.From] | move.EmptyBoard | move.To.AsBitBoard();
                }
            }
        }
    }

    private void SetAttacks(byte piece, List<List<AttackBase>>[][] attacksTemp, List<List<PromotionAttack>>[][] promotionsAttackTemp)
    {
        switch (piece)
        {
            case Pieces.WhitePawn:
                SetWhitePawnAttacks(attacksTemp);
                SetWhitePromotionAttacks(promotionsAttackTemp);
                break;
            case Pieces.WhiteKnight:
                SetWhiteKnightAttacks(attacksTemp);
                break;
            case Pieces.WhiteBishop:
                SetWhiteBishopAttacks(attacksTemp);
                break;
            case Pieces.WhiteRook:
                SetWhiteRookAttacks(attacksTemp);
                break;
            case Pieces.WhiteKing:
                SetWhiteKingAttacks(attacksTemp);
                break;
            case Pieces.WhiteQueen:
                SetWhiteQueenAttacks(attacksTemp);
                break;
            case Pieces.BlackPawn:
                SetBlackPawnAttacks(attacksTemp);
                SetBlackPromotionAttacks(promotionsAttackTemp);
                break;
            case Pieces.BlackKnight:
                SetBlackKnightAttacks(attacksTemp);
                break;
            case Pieces.BlackBishop:
                SetBlackBishopAttacks(attacksTemp);
                break;
            case Pieces.BlackRook:
                SetBlackRookAttacks(attacksTemp);
                break;
            case Pieces.BlackKing:
                SetBlackKingAttacks(attacksTemp);
                break;
            case Pieces.BlackQueen:
                SetBlackQueenAttacks(attacksTemp);
                break;
        }
    }

    private void SetMoves(byte piece, List<List<MoveBase>>[][] movesTemp, List<List<PromotionMove>>[][] promotionsTemp)
    {
        switch (piece)
        {
            case Pieces.WhitePawn:
                SetWhitePawnMoves(movesTemp);
                SetWhitePromotionMoves(promotionsTemp);
                break;
            case Pieces.WhiteKnight:
                SetWhiteKnightMoves(movesTemp);
                break;
            case Pieces.WhiteBishop:
                SetMovesWhiteBishop(movesTemp);
                break;
            case Pieces.WhiteRook:
                SetWhiteRookMoves(movesTemp);
                break;
            case Pieces.WhiteKing:
                SetWhiteKingMoves(movesTemp);
                break;
            case Pieces.WhiteQueen:
                SetWhiteQueenMoves(movesTemp);
                break;
            case Pieces.BlackPawn:
                SetBlackPawnMoves(movesTemp);
                SetBlackPromotionMoves(promotionsTemp);
                break;
            case Pieces.BlackKnight:
                SetBlackKnightMoves(movesTemp);
                break;
            case Pieces.BlackBishop:
                SetBlackBishopMoves(movesTemp);
                break;
            case Pieces.BlackRook:
                SetBlackRookMoves(movesTemp);
                break;
            case Pieces.BlackKing:
                SetBlackKingMoves(movesTemp);
                break;
            case Pieces.BlackQueen:
                SetBlackQueenMoves(movesTemp);
                break;
        }
    }

    private bool IsIn(int i) => i > -1 && i < 64;
}
