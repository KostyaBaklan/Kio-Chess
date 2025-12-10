using Engine.DataStructures;
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

        _whitePawnOverAttacks = GeneratePawnOverAttacks(overAttacks.Where(a => a.Piece == Pieces.WhitePawn));
        _blackPawnOverAttacks = GeneratePawnOverAttacks(overAttacks.Where(a => a.Piece == Pieces.BlackPawn));
    }

    private void SetPromotionAttacks()
    {
        for (byte p = 0; p < _piecesNumbers; p++)
        {
            for (byte s = 0; s < _squaresNumber; s++)
            {
                if (_promotionsAttackTemp[p][s] == null) continue;

                var dynamicArray = new DynamicArray<PromotionAttackList>(_promotionsAttackTemp[p][s].Count);
                for (var i = 0; i < _promotionsAttackTemp[p][s].Count; i++)
                {
                    if (_promotionsAttackTemp[p][s][i] == null) continue;

                    PromotionAttackList moves = new(_promotionsAttackTemp[p][s][i].Count);
                    for (var j = 0; j < _promotionsAttackTemp[p][s][i].Count; j++)
                    {
                        moves.Add(_promotionsAttackTemp[p][s][i][j]);
                    }
                    dynamicArray.Add(moves);
                    _promotionsAttackTemp[p][s][i].Clear();
                    _promotionsAttackTemp[p][s][i] = null;
                }

                _promotionsAttackTemp[p][s].Clear();
                _promotionsAttackTemp[p][s] = null;
                _promotionAttacks[p][s] = dynamicArray;
            }

            Array.Clear(_promotionsAttackTemp[p], 0, _promotionsAttackTemp[p].Length);
            _promotionsAttackTemp[p] = null;
        }

        Array.Clear(_promotionsAttackTemp, 0, _promotionsAttackTemp.Length);
        _promotionsAttackTemp = null;
    }

    private void SetAttacks()
    {
        for (byte p = 0; p < _piecesNumbers; p++)
        {
            for (byte s = 0; s < _squaresNumber; s++)
            {
                if (_attacksTemp[p][s] == null) continue;

                var dynamicArray = new DynamicArray<AttackList>(_attacksTemp[p][s].Count);
                for (var i = 0; i < _attacksTemp[p][s].Count; i++)
                {
                    if (_attacksTemp[p][s][i] == null) continue;

                    AttackList moves = new(_attacksTemp[p][s][i].Count);
                    for (var j = 0; j < _attacksTemp[p][s][i].Count; j++)
                    {
                        moves.Add(_attacksTemp[p][s][i][j]);
                    }
                    dynamicArray.Add(moves);
                    _attacksTemp[p][s][i].Clear();
                    _attacksTemp[p][s][i] = null;
                }

                _attacksTemp[p][s].Clear();
                _attacksTemp[p][s] = null;
                _attacks[p][s] = dynamicArray;
            }

            Array.Clear(_attacksTemp[p], 0, _attacksTemp[p].Length);
            _attacksTemp[p] = null;
        }

        Array.Clear(_attacksTemp, 0, _attacksTemp.Length);
        _attacksTemp = null;


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
        for (byte p = 0; p < _piecesNumbers; p++)
        {
            for (byte s = 0; s < _squaresNumber; s++)
            {
                if (_promotionsTemp[p][s] == null) continue;

                var dynamicArray = new DynamicArray<PromotionList>(_promotionsTemp[p][s].Count);
                for (var i = 0; i < _promotionsTemp[p][s].Count; i++)
                {
                    if (_promotionsTemp[p][s][i] == null) continue;

                    PromotionList moves = new(_promotionsTemp[p][s][i].Count);
                    for (var j = 0; j < _promotionsTemp[p][s][i].Count; j++)
                    {
                        moves.Add(_promotionsTemp[p][s][i][j]);
                    }

                    dynamicArray.Add(moves);
                    _promotionsTemp[p][s][i].Clear();
                    _promotionsTemp[p][s][i] = null;
                }

                _promotionsTemp[p][s].Clear();
                _promotionsTemp[p][s] = null;
                _promotions[p][s] = dynamicArray;
            }

            Array.Clear(_promotionsTemp[p], 0, _promotionsTemp[p].Length);
            _promotionsTemp[p] = null;
        }

        Array.Clear(_promotionsTemp, 0, _promotionsTemp.Length);
        _promotionsTemp = null;

        var promotions = _all.Where(m => m.IsPromotion).ToList();

        _whitePromotions = GeneratePromotions(promotions.OfType<PromotionMove>().Where(p => p.Piece == Pieces.WhitePawn));
        _blackPromotions = GeneratePromotions(promotions.OfType<PromotionMove>().Where(p => p.Piece == Pieces.BlackPawn));

        _whitePromotionAttacks = GeneratePromotions(promotions.OfType<WhitePromotionAttack>());
        _blackPromotionAttacks = GeneratePromotions(promotions.OfType<BlackPromotionAttack>());
    }

    private void SetMoves()
    {
        for (byte p = 0; p < _piecesNumbers; p++)
        {
            for (byte s = 0; s < _squaresNumber; s++)
            {
                if (_movesTemp[p][s] == null) continue;

                var dynamicArray = new DynamicArray<MoveList>(_movesTemp[p][s].Count);
                for (var i = 0; i < _movesTemp[p][s].Count; i++)
                {
                    if (_movesTemp[p][s][i] == null) continue;

                    MoveList moves = new(_movesTemp[p][s][i].Count);
                    for (var j = 0; j < _movesTemp[p][s][i].Count; j++)
                    {
                        moves.Add(_movesTemp[p][s][i][j]);
                    }

                    dynamicArray.Add(moves);
                    _movesTemp[p][s][i].Clear();
                    _movesTemp[p][s][i] = null;
                }

                _movesTemp[p][s].Clear();
                _movesTemp[p][s] = null;
                _moves[p][s] = dynamicArray;
            }

            Array.Clear(_movesTemp[p], 0, _movesTemp[p].Length);
            _movesTemp[p] = null;
        }

        Array.Clear(_movesTemp, 0, _movesTemp.Length);
        _movesTemp = null;

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

    private void SetAttackPatterns(byte piece)
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

    private void SetAttacks(byte piece)
    {
        switch (piece)
        {
            case Pieces.WhitePawn:
                SetWhitePawnAttacks();
                SetWhitePromotionAttacks();
                break;
            case Pieces.WhiteKnight:
                SetWhiteKnightAttacks();
                break;
            case Pieces.WhiteBishop:
                SetWhiteBishopAttacks();
                break;
            case Pieces.WhiteRook:
                SetWhiteRookAttacks();
                break;
            case Pieces.WhiteKing:
                SetWhiteKingAttacks();
                break;
            case Pieces.WhiteQueen:
                SetWhiteQueenAttacks();
                break;
            case Pieces.BlackPawn:
                SetBlackPawnAttacks();
                SetBlackPromotionAttacks();
                break;
            case Pieces.BlackKnight:
                SetBlackKnightAttacks();
                break;
            case Pieces.BlackBishop:
                SetBlackBishopAttacks();
                break;
            case Pieces.BlackRook:
                SetBlackRookAttacks();
                break;
            case Pieces.BlackKing:
                SetBlackKingAttacks();
                break;
            case Pieces.BlackQueen:
                SetBlackQueenAttacks();
                break;
        }
    }

    private void SetMoves(byte piece)
    {
        switch (piece)
        {
            case Pieces.WhitePawn:
                SetWhitePawnMoves();
                SetWhitePromotionMoves();
                break;
            case Pieces.WhiteKnight:
                SetWhiteKnightMoves();
                break;
            case Pieces.WhiteBishop:
                SetMovesWhiteBishop();
                break;
            case Pieces.WhiteRook:
                SetWhiteRookMoves();
                break;
            case Pieces.WhiteKing:
                SetWhiteKingMoves();
                break;
            case Pieces.WhiteQueen:
                SetWhiteQueenMoves();
                break;
            case Pieces.BlackPawn:
                SetBlackPawnMoves();
                SetBlackPromotionMoves();
                break;
            case Pieces.BlackKnight:
                SetBlackKnightMoves();
                break;
            case Pieces.BlackBishop:
                SetBlackBishopMoves();
                break;
            case Pieces.BlackRook:
                SetBlackRookMoves();
                break;
            case Pieces.BlackKing:
                SetBlackKingMoves();
                break;
            case Pieces.BlackQueen:
                SetBlackQueenMoves();
                break;
        }
    }

    private bool IsIn(int i) => i > -1 && i < 64;
}
