using Engine.DataStructures.Moves.Lists;
using Engine.Models.Enums;
using Engine.Models.Moves;

namespace Engine.Services;

public partial class MoveProvider
{
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

        _whitePawnOverAttacks = GeneratePawnOverAttacks(overAttacks.Where(a => a.Piece == Pieces.WhitePawn));
        _blackPawnOverAttacks = GeneratePawnOverAttacks(overAttacks.Where(a => a.Piece == Pieces.BlackPawn));
    }

    private void SetPromotionAttacks()
    {
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

        _whitePromotions = GeneratePromotions(promotions.OfType<PromotionMove>().Where(p => p.Piece == Pieces.WhitePawn));
        _blackPromotions = GeneratePromotions(promotions.OfType<PromotionMove>().Where(p => p.Piece == Pieces.BlackPawn));

        _whitePromotionAttacks = GeneratePromotions(promotions.OfType<WhitePromotionAttack>());
        _blackPromotionAttacks = GeneratePromotions(promotions.OfType<BlackPromotionAttack>());
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

    private AttackList[] GeneratePawnOverAttacks(IEnumerable<PawnOverAttack> moveBases)
    {
        AttackList[] moves = new AttackList[64];
        for (int f = 0; f < 64; f++)
        {
            moves[f] = new AttackList(2);
        }

        var overAttacks = moveBases.GroupBy(m => m.From).ToDictionary(k => k.Key, v => v.ToArray());

        foreach (var over in overAttacks)
        {
            foreach (var attack in over.Value)
            {
                moves[over.Key].Add(attack);
            }
        }

        return moves;
    }

    private AttackBase[][] GenerateAttacks(List<AttackBase> moveBases)
    {
        AttackBase[][] moves = new AttackBase[64][];
        for (int f = 0; f < 64; f++)
        {
            moves[f] = new AttackBase[64];
        }

        moveBases.ForEach(m => moves[m.From][m.To] = m);

        return moves;
    }

    private MoveBase[][] GenerateMoves(List<MoveBase> moveBases)
    {
        MoveBase[][] moves = new MoveBase[64][];
        for (int f = 0; f < 64; f++)
        {
            moves[f] = new MoveBase[64];
        }

        moveBases.ForEach(m => moves[m.From][m.To] = m);

        return moves;
    }

    private PromotionAttackList[][] GeneratePromotions(IEnumerable<BlackPromotionAttack> enumerable)
    {
        PromotionAttackList[][] moves = new PromotionAttackList[64][];
        for (int f = 0; f < 64; f++)
        {
            moves[f] = new PromotionAttackList[64];
        }

        foreach (var m in enumerable)
        {
            if (moves[m.From][m.To] == null)
            {
                moves[m.From][m.To] = new PromotionAttackList(4);
            }
            moves[m.From][m.To].Add(m);
        }

        return moves;
    }

    private PromotionAttackList[][] GeneratePromotions(IEnumerable<WhitePromotionAttack> enumerable)
    {
        PromotionAttackList[][] moves = new PromotionAttackList[64][];
        for (int f = 0; f < 64; f++)
        {
            moves[f] = new PromotionAttackList[64];
        }

        foreach (var m in enumerable)
        {
            if (moves[m.From][m.To] == null)
            {
                moves[m.From][m.To] = new PromotionAttackList(4);
            }
            moves[m.From][m.To].Add(m);
        }

        return moves;
    }

    private PromotionList[][] GeneratePromotions(IEnumerable<PromotionMove> enumerable)
    {
        PromotionList[][] moves = new PromotionList[64][];
        for (int f = 0; f < 64; f++)
        {
            moves[f] = new PromotionList[64];
        }

        foreach (var m in enumerable)
        {
            if (moves[m.From][m.To] == null)
            {
                moves[m.From][m.To] = new PromotionList(4);
            }
            moves[m.From][m.To].Add(m);
        }

        return moves;
    }

    private void SetAttacks(byte piece, List<List<AttackBase>>[][] attacksTemp, List<List<PromotionAttack>>[][] promotionsAttackTemp)
    {
        switch (piece)
        {
            case Pieces.WhitePawn:
                SetWhitePawnAttacks(attacksTemp[Pieces.WhitePawn]);
                SetWhitePromotionAttacks(promotionsAttackTemp[Pieces.WhitePawn]);
                break;
            case Pieces.WhiteKnight:
                SetWhiteKnightAttacks(attacksTemp[Pieces.WhiteKnight]);
                break;
            case Pieces.WhiteBishop:
                SetWhiteBishopAttacks(attacksTemp[Pieces.WhiteBishop]);
                break;
            case Pieces.WhiteRook:
                SetWhiteRookAttacks(attacksTemp[Pieces.WhiteRook]);
                break;
            case Pieces.WhiteKing:
                SetWhiteKingAttacks(attacksTemp[Pieces.WhiteKing]);
                break;
            case Pieces.WhiteQueen:
                SetWhiteQueenAttacks(attacksTemp[Pieces.WhiteQueen]);
                break;
            case Pieces.BlackPawn:
                SetBlackPawnAttacks(attacksTemp[Pieces.BlackPawn]);
                SetBlackPromotionAttacks(promotionsAttackTemp[Pieces.BlackPawn]);
                break;
            case Pieces.BlackKnight:
                SetBlackKnightAttacks(attacksTemp[Pieces.BlackKnight]);
                break;
            case Pieces.BlackBishop:
                SetBlackBishopAttacks(attacksTemp[Pieces.BlackBishop]);
                break;
            case Pieces.BlackRook:
                SetBlackRookAttacks(attacksTemp[Pieces.BlackRook]);
                break;
            case Pieces.BlackKing:
                SetBlackKingAttacks(attacksTemp[Pieces.BlackKing]);
                break;
            case Pieces.BlackQueen:
                SetBlackQueenAttacks(attacksTemp[Pieces.BlackQueen]);
                break;
        }
    }

    private void SetMoves(byte piece, List<List<MoveBase>>[][] movesTemp, List<List<PromotionMove>>[][] promotionsTemp)
    {
        switch (piece)
        {
            case Pieces.WhitePawn:
                SetWhitePawnMoves(movesTemp[Pieces.WhitePawn]);
                SetWhitePromotionMoves(promotionsTemp[Pieces.WhitePawn]);
                break;
            case Pieces.WhiteKnight:
                SetWhiteKnightMoves(movesTemp[Pieces.WhiteKnight]);
                break;
            case Pieces.WhiteBishop:
                SetMovesWhiteBishop(movesTemp[Pieces.WhiteBishop]);
                break;
            case Pieces.WhiteRook:
                SetWhiteRookMoves(movesTemp[Pieces.WhiteRook]);
                break;
            case Pieces.WhiteKing:
                SetWhiteKingMoves(movesTemp[Pieces.WhiteKing]);
                break;
            case Pieces.WhiteQueen:
                SetWhiteQueenMoves(movesTemp[Pieces.WhiteQueen]);
                break;
            case Pieces.BlackPawn:
                SetBlackPawnMoves(movesTemp[Pieces.BlackPawn]);
                SetBlackPromotionMoves(promotionsTemp[Pieces.WhitePawn]);
                break;
            case Pieces.BlackKnight:
                SetBlackKnightMoves(movesTemp[Pieces.BlackKnight]);
                break;
            case Pieces.BlackBishop:
                SetBlackBishopMoves(movesTemp[Pieces.BlackBishop]);
                break;
            case Pieces.BlackRook:
                SetBlackRookMoves(movesTemp[Pieces.BlackRook]);
                break;
            case Pieces.BlackKing:
                SetBlackKingMoves(movesTemp[Pieces.BlackKing]);
                break;
            case Pieces.BlackQueen:
                SetBlackQueenMoves(movesTemp[Pieces.BlackQueen]);
                break;
        }
    }

    private bool IsIn(int i) => i > -1 && i < 64;
}
