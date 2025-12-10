using Engine.Models.Enums;
using Engine.Models.Moves;

namespace Engine.Services;

public partial class MoveProvider
{
    #region Queens

    private void SetBlackQueenAttacks()
    {
        var piece = Pieces.BlackQueen;
        var moves = _attacksTemp[piece];
        SetBlackStrightAttacks(piece, moves);
        SetBlackDiagonalAttacks(piece, moves);
    }

    private void SetWhiteQueenAttacks()
    {
        var piece = Pieces.WhiteQueen;
        var moves = _attacksTemp[piece];
        SetWhiteStrightAttacks(piece, moves);
        SetWhiteDiagonalAttacks(piece, moves);
    }

    private void SetBlackQueenMoves()
    {
        var piece = Pieces.BlackQueen;
        var moves = _movesTemp[piece];
        SetBlackDiagonalMoves(piece, moves);
        SetBlackStrightMoves(piece, moves);
    }

    private void SetWhiteQueenMoves()
    {
        var piece = Pieces.WhiteQueen;
        var moves = _movesTemp[piece];
        SetWhiteDiagonalMoves(piece, moves);
        SetWhiteStrightMoves(piece, moves);
    }

    #endregion

    #region Rooks

    private void SetBlackRookAttacks()
    {
        var piece = Pieces.BlackRook;
        var moves = _attacksTemp[piece];
        SetBlackStrightAttacks(piece, moves);
    }

    private void SetWhiteRookAttacks()
    {
        var piece = Pieces.WhiteRook;
        var moves = _attacksTemp[piece];
        SetWhiteStrightAttacks(piece, moves);
    }

    private void SetBlackRookMoves()
    {
        var piece = Pieces.BlackRook;
        var moves = _movesTemp[piece];
        SetBlackStrightMoves(piece, moves);
    }

    private void SetWhiteRookMoves()
    {
        var piece = Pieces.WhiteRook;
        var moves = _movesTemp[piece];
        SetWhiteStrightMoves(piece, moves);
    }

    #endregion

    #region Bishops

    private void SetBlackBishopAttacks()
    {
        var piece = Pieces.BlackBishop;
        var moves = _attacksTemp[piece];
        SetBlackDiagonalAttacks(piece, moves);
    }

    private void SetWhiteBishopAttacks()
    {
        var piece = Pieces.WhiteBishop;
        var moves = _attacksTemp[piece];
        SetWhiteDiagonalAttacks(piece, moves);
    }

    private void SetBlackBishopMoves()
    {
        var piece = Pieces.BlackBishop;
        var moves = _movesTemp[piece];
        SetBlackDiagonalMoves(piece, moves);
    }

    private void SetMovesWhiteBishop()
    {
        var piece = Pieces.WhiteBishop;
        var moves = _movesTemp[piece];
        SetWhiteDiagonalMoves(piece, moves);
    }

    #endregion

    #region Kings

    private void SetBlackKingAttacks()
    {
        var figure = Pieces.BlackKing;
        var moves = _attacksTemp[figure];

        for (int from = 0; from < _squaresNumber; from++)
        {
            foreach (int to in KingMoves(from).Where(IsIn))
            {
                var move = new BlackSimpleAttack
                { From = (byte)from, To = (byte)to, Piece = figure };
                moves[from].Add([move]);
            }
        }
    }

    private void SetWhiteKingAttacks()
    {
        var figure = Pieces.WhiteKing;
        var moves = _attacksTemp[figure];

        for (int from = 0; from < _squaresNumber; from++)
        {
            foreach (int to in KingMoves(from).Where(IsIn))
            {
                var move = new WhiteSimpleAttack
                { From = (byte)from, To = (byte)to, Piece = figure };
                moves[from].Add([move]);
            }
        }
    }

    private void SetBlackKingMoves()
    {
        var figure = Pieces.BlackKing;
        var moves = _movesTemp[figure];

        var small = new BlackSmallCastle
        { From = 60, To = 62, Piece = figure };
        small.Set(61, 62);
        moves[60].Add([small]);

        var big = new BlackBigCastle
        { From = 60, To = 58, Piece = figure };
        big.Set(58, 59);
        moves[60].Add([big]);

        for (byte from = 0; from < _squaresNumber; from++)
        {
            foreach (byte to in KingMoves(from).Where(IsIn))
            {
                var move = new BlackMove
                { From = from, To = to, Piece = figure };
                move.Set(to);
                moves[from].Add([move]);
            }
        }
    }

    private void SetWhiteKingMoves()
    {
        var figure = Pieces.WhiteKing;
        var moves = _movesTemp[figure];

        var small = new WhiteSmallCastle
        { From = 4, To = 6, Piece = figure };
        small.Set(5, 6);
        moves[4].Add([small]);

        var big = new WhiteBigCastle
        { From = 4, To = 2, Piece = figure };
        big.Set(2, 3);
        moves[4].Add([big]);

        for (byte from = 0; from < _squaresNumber; from++)
        {
            foreach (byte to in KingMoves(from).Where(IsIn))
            {
                var move = new WhiteMove
                { From = from, To = to, Piece = figure };
                move.Set(to);
                moves[from].Add([move]);
            }
        }
    }

    private IEnumerable<int> KingMoves(int f)
    {
        if (f == 0)
        {
            return new[] { 1, 9, 8 };
        }

        if (f == 7)
        {
            return new[] { 6, 14, 15 };
        }
        if (f == 56)
        {
            return new[] { 48, 49, 57 };
        }
        if (f == 63)
        {
            return new[] { 62, 54, 55 };
        }

        if (f % 8 == 0) //B1 => Squares.A1,Squares.C1,Squares.B2,Squares.A2,Squares.C2
        {
            return new[] { f + 8, f + 9, f + 1, f - 7, f - 8 };
        }
        if (f % 8 == 7)//B8 => Squares.A8,Squares.C8,Squares.B7,Squares.A7,Squares.C7
        {
            return new[] { f + 8, f + 7, f - 1, f - 9, f - 8 };
        }

        if (f / 8 == 0)
        {
            return new[] { f + 1, f - 1, f + 7, f + 9, f + 8 };
        }
        if (f / 8 == 7)
        {
            return new[] { f + 1, f - 7, f - 1, f - 9, f - 8 };
        }

        return new[] { f + 8, f + 7, f - 1, f + 9, f + 1, f - 9, f - 7, f - 8 };
    }

    #endregion

    #region Knights

    private void SetBlackKnightAttacks()
    {
        var figure = Pieces.BlackKnight;
        var moves = _attacksTemp[figure];

        for (int from = 0; from < _squaresNumber; from++)
        {
            foreach (var to in KnightMoves(from).Where(IsIn))
            {
                var move = new BlackSimpleAttack
                { From = (byte)from, To = (byte)to, Piece = figure };
                moves[from].Add([move]);
            }
        }
    }

    private void SetWhiteKnightAttacks()
    {
        var figure = Pieces.WhiteKnight;
        var moves = _attacksTemp[figure];

        for (int from = 0; from < _squaresNumber; from++)
        {
            foreach (var to in KnightMoves(from).Where(IsIn))
            {
                var move = new WhiteSimpleAttack
                { From = (byte)from, To = (byte)to, Piece = figure };
                moves[from].Add([move]);
            }
        }
    }

    private void SetBlackKnightMoves()
    {
        var figure = Pieces.BlackKnight;
        var moves = _movesTemp[figure];

        for (byte from = 0; from < _squaresNumber; from++)
        {
            foreach (byte to in KnightMoves(from).Where(IsIn))
            {
                var move = new BlackMove
                { From = from, To = to, Piece = figure };
                move.Set(to);
                moves[from].Add([move]);
            }
        }
    }

    private void SetWhiteKnightMoves()
    {
        var figure = Pieces.WhiteKnight;
        var moves = _movesTemp[figure];

        for (byte from = 0; from < _squaresNumber; from++)
        {
            foreach (byte to in KnightMoves(from).Where(IsIn))
            {
                var move = new WhiteMove
                { From = from, To = to, Piece = figure };
                move.Set(to);
                moves[from].Add([move]);
            }
        }
    }

    private static IEnumerable<int> KnightMoves(int i)
    {
        if (i / 8 - 1 == (i - 10) / 8)
        {
            yield return i - 10;
        }
        if (i / 8 - 2 == (i - 17) / 8)
        {
            yield return i - 17;
        }
        if (i / 8 + 1 == (i + 6) / 8)
        {
            yield return i + 6;
        }
        if (i / 8 + 1 == (i + 10) / 8)
        {
            yield return i + 10;
        }
        if (i / 8 - 1 == (i - 6) / 8)
        {
            yield return i - 6;
        }
        if (i / 8 - 2 == (i - 15) / 8)
        {
            yield return i - 15;
        }
        if (i / 8 + 2 == (i + 15) / 8)
        {
            yield return i + 15;
        }
        if (i / 8 + 2 == (i + 17) / 8)
        {
            yield return i + 17;
        }
    }

    #endregion

    #region Pawns

    private void SetBlackPromotionAttacks()
    {
        var figure = Pieces.BlackPawn;
        var moves = _promotionsAttackTemp[figure];

        for (int i = 8; i < 16; i++)
        {
            var listLeft = new List<PromotionAttack>(4);
            var listRight = new List<PromotionAttack>(4);
            List<byte> types =
            [
                Pieces.BlackQueen,Pieces.BlackRook,Pieces.BlackBishop,Pieces.BlackKnight
            ];
            for (int j = 0; j < types.Count; j++)
            {
                if (i < 15)
                {
                    var a1 = new BlackPromotionAttack
                    {
                        From = (byte)i,
                        To = (byte)(i - 7),
                        Piece = figure,
                        PromotionPiece = types[j],
                        PromotionSee = see[j]
                    };
                    listLeft.Add(a1);
                }

                if (i > 8)
                {
                    var a2 = new BlackPromotionAttack
                    {
                        From = (byte)i,
                        To = (byte)(i - 9),
                        Piece = figure,
                        PromotionPiece = types[j],
                        PromotionSee = see[j]
                    };
                    listRight.Add(a2);
                }
            }

            moves[i].Add(listLeft);
            moves[i].Add(listRight);
        }
    }

    private void SetBlackPawnAttacks()
    {
        var figure = Pieces.BlackPawn;
        var moves = _attacksTemp[figure];

        for (int i = 16; i < 56; i++)
        {
            int x = i % 8;

            if (x < 7)
            {
                var a1 = new BlackSimpleAttack
                {
                    From = (byte)i,
                    To = (byte)(i - 7),
                    Piece = figure
                };
                moves[i].Add([a1]);
            }

            if (x > 0)
            {
                var a2 = new BlackSimpleAttack
                {
                    From = (byte)i,
                    To = (byte)(i - 9),
                    Piece = figure
                };
                moves[i].Add([a2]);
            }
        }

        for (int i = 24; i < 32; i++)
        {
            if (i < 31)
            {
                var a1 = new PawnOverBlackAttack
                {
                    From = (byte)i,
                    To = (byte)(i - 7),
                    Piece = figure
                };
                moves[i].Add([a1]);
            }

            if (i > 24)
            {
                var a2 = new PawnOverBlackAttack
                {
                    From = (byte)i,
                    To = (byte)(i - 9),
                    Piece = figure
                };
                moves[i].Add([a2]);
            }
        }
    }

    private void SetWhitePromotionAttacks()
    {
        var figure = Pieces.WhitePawn;
        var moves = _promotionsAttackTemp[figure];
        for (int i = 48; i < 56; i++)
        {
            var listLeft = new List<PromotionAttack>(4);
            var listRight = new List<PromotionAttack>(4);
            List<byte> types =
            [
                Pieces.WhiteQueen,Pieces.WhiteRook,Pieces.WhiteBishop,Pieces.WhiteKnight
            ];
            for (int j = 0; j < types.Count; j++)
            {
                if (i > 48)
                {
                    var a1 = new WhitePromotionAttack
                    {
                        From = (byte)i,
                        To = (byte)(i + 7),
                        Piece = figure,
                        PromotionPiece = types[j],
                        PromotionSee = see[j]
                    };
                    listLeft.Add(a1);
                }

                if (i < 55)
                {
                    var a2 = new WhitePromotionAttack
                    {
                        From = (byte)i,
                        To = (byte)(i + 9),
                        Piece = figure,
                        PromotionPiece = types[j],
                        PromotionSee = see[j]
                    };
                    listRight.Add(a2);
                }
            }

            moves[i].Add(listLeft);
            moves[i].Add(listRight);
        }
    }

    private void SetWhitePawnAttacks()
    {
        var figure = Pieces.WhitePawn;
        var moves = _attacksTemp[figure];

        for (int i = 8; i < 48; i++)
        {
            int x = i % 8;

            if (x > 0)
            {
                var a1 = new WhiteSimpleAttack
                {
                    From = (byte)i,
                    To = (byte)(i + 7),
                    Piece = figure
                };
                moves[i].Add([a1]);
            }

            if (x < 7)
            {
                var a2 = new WhiteSimpleAttack
                {
                    From = (byte)i,
                    To = (byte)(i + 9),
                    Piece = figure
                };
                moves[i].Add([a2]);
            }
        }

        for (int i = 32; i < 40; i++)
        {
            if (i > 32)
            {
                var b = i - 1;
                var a1 = new PawnOverWhiteAttack
                {
                    From = (byte)i,
                    To = (byte)(i + 7),
                    Piece = figure
                };
                moves[i].Add([a1]);
            }

            if (i < 39)
            {
                var b = i + 1;
                var a2 = new PawnOverWhiteAttack
                {
                    From = (byte)i,
                    To = (byte)(i + 9),
                    Piece = figure
                };
                moves[i].Add([a2]);
            }
        }
    }

    private void SetBlackPromotionMoves()
    {
        var moves = _promotionsTemp[6];
        for (byte i = 8; i < 16; i++)
        {
            var list = new List<PromotionMove>(4);
            List<byte> types =
            [
                Pieces.BlackQueen,Pieces.BlackRook,Pieces.BlackBishop,Pieces.BlackKnight
            ];

            for (int j = 0; j < types.Count; j++)
            {
                var move = new PromotionBlackMove
                {
                    From = i,
                    To = (byte)(i - 8),
                    Piece = Pieces.BlackPawn,
                    PromotionPiece = types[j],
                    PromotionSee = see[j]
                };


                move.Set((byte)(i - 8));
                list.Add(move);
            }
            moves[i].Add(list);
        }
    }

    private void SetBlackPawnMoves()
    {
        var figure = Pieces.BlackPawn;
        var moves = _movesTemp[figure];
        for (byte i = 48; i < 56; i++)
        {
            var to = i - 16;
            var move = new PawnOverBlackMove()
            { From = i, To = (byte)to, Piece = figure };

            if (i == 48)
            {
                move.OpponentPawns |= move.OpponentPawns.Add(to + 1);
            }
            else if (i == 55)
            {
                move.OpponentPawns |= move.OpponentPawns.Add(to - 1);
            }
            else
            {
                move.OpponentPawns |= move.OpponentPawns.Add(to - 1);
                move.OpponentPawns |= move.OpponentPawns.Add(to + 1);
            }

            move.Set((byte)(i - 8), (byte)to);
            moves[i].Add([move]);
        }

        for (byte i = 16; i < 56; i++)
        {
            var move = new BlackMove
            { From = i, To = (byte)(i - 8), Piece = figure };
            move.Set((byte)(i - 8));
            moves[i].Add([move]);
        }
    }

    private void SetWhitePromotionMoves()
    {
        var moves = _promotionsTemp[0];
        for (byte i = 48; i < 56; i++)
        {
            var list = new List<PromotionMove>(4);
            List<byte> types =
            [
                Pieces.WhiteQueen,Pieces.WhiteRook,Pieces.WhiteBishop,Pieces.WhiteKnight
            ];
            for (int j = 0; j < types.Count; j++)
            {
                var move = new PromotionWhiteMove
                {
                    From = i,
                    To = (byte)(i + 8),
                    Piece = Pieces.WhitePawn,
                    PromotionPiece = types[j],
                    PromotionSee = see[j]
                };
                move.Set((byte)(i + 8));
                list.Add(move);
            }
            moves[i].Add(list);
        }
    }

    private void SetWhitePawnMoves()
    {
        var figure = Pieces.WhitePawn;
        var moves = _movesTemp[figure];
        for (byte i = 8; i < 16; i++)
        {
            var to = i + 16;
            var move = new PawnOverWhiteMove
            { From = i, To = (byte)to, Piece = figure };
            if (i == 8)
            {
                move.OpponentPawns |= move.OpponentPawns.Add(to + 1);
            }
            else if (i == 15)
            {
                move.OpponentPawns |= move.OpponentPawns.Add(to - 1);
            }
            else
            {
                move.OpponentPawns |= move.OpponentPawns.Add(to - 1);
                move.OpponentPawns |= move.OpponentPawns.Add(to + 1);
            }

            move.Set((byte)(i + 8), (byte)to);
            moves[i].Add([move]);
        }

        for (byte i = 8; i < 48; i++)
        {
            var move = new WhiteMove
            { From = i, To = (byte)(i + 8), Piece = figure };
            move.Set((byte)(i + 8));
            moves[i].Add([move]);
        }
    }

    #endregion

    #region Stright and Diagonal Moves

    private static void SetBlackStrightMoves(byte piece, List<List<MoveBase>>[] moves)
    {
        for (byte y = 0; y < 8; y++)
        {
            for (byte x = 0; x < 8; x++)
            {
                byte cF = (byte)(y * 8 + x);

                var l = new List<MoveBase>();
                int offset = 1;
                var a = x - 1;
                while (a > -1)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new BlackMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)(y * 8 + x - i));
                    }

                    l.Add(move);
                    a--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                a = x + 1;
                while (a < 8)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new BlackMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)(y * 8 + x + i));
                    }

                    l.Add(move);
                    a++;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                var b = y - 1;
                while (b > -1)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new BlackMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)((y - i) * 8 + x));
                    }

                    l.Add(move);
                    b--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                b = y + 1;
                while (b < 8)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new BlackMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)((y + i) * 8 + x));
                    }

                    l.Add(move);
                    b++;
                    offset++;
                }
                moves[cF].Add(l);
            }
        }
    }

    private static void SetWhiteStrightMoves(byte piece, List<List<MoveBase>>[] moves)
    {
        for (byte y = 0; y < 8; y++)
        {
            for (byte x = 0; x < 8; x++)
            {
                byte cF = (byte)(y * 8 + x);

                var l = new List<MoveBase>();
                int offset = 1;
                var a = x - 1;
                while (a > -1)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new WhiteMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)(y * 8 + x - i));
                    }

                    l.Add(move);
                    a--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                a = x + 1;
                while (a < 8)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new WhiteMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)(y * 8 + x + i));
                    }

                    l.Add(move);
                    a++;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                var b = y - 1;
                while (b > -1)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new WhiteMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)((y - i) * 8 + x));
                    }

                    l.Add(move);
                    b--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                b = y + 1;
                while (b < 8)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new WhiteMove { From = cF, To = cT, Piece = piece };
                    for (byte i = 1; i <= offset; i++)
                    {
                        move.Set((byte)((y + i) * 8 + x));
                    }

                    l.Add(move);
                    b++;
                    offset++;
                }
                moves[cF].Add(l);
            }
        }
    }

    private static void SetBlackStrightAttacks(byte piece, List<List<AttackBase>>[] moves)
    {
        for (byte y = 0; y < 8; y++)
        {
            for (byte x = 0; x < 8; x++)
            {
                byte cF = (byte)(y * 8 + x);

                var l = new List<AttackBase>();
                int offset = 1;
                var a = x - 1;
                while (a > -1)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new BlackAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)((y * 8) + x - i));
                    }

                    l.Add(move);
                    a--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                a = x + 1;
                while (a < 8)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new BlackAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)(y * 8 + x + i));
                    }

                    l.Add(move);
                    a++;
                    offset++;
                }
                moves[cF].Add(l);


                l = [];
                offset = 1;
                var b = y - 1;
                while (b > -1)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new BlackAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)((y - i) * 8 + x));
                    }

                    l.Add(move);
                    b--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                b = y + 1;
                while (b < 8)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new BlackAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)((y + i) * 8 + x));
                    }

                    l.Add(move);
                    b++;
                    offset++;
                }
                moves[cF].Add(l);
            }
        }
    }

    private static void SetWhiteStrightAttacks(byte piece, List<List<AttackBase>>[] moves)
    {
        for (byte y = 0; y < 8; y++)
        {
            for (byte x = 0; x < 8; x++)
            {
                byte cF = (byte)(y * 8 + x);

                var l = new List<AttackBase>();
                int offset = 1;
                var a = x - 1;
                while (a > -1)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new WhiteAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)(y * 8 + x - i));
                    }

                    l.Add(move);
                    a--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                a = x + 1;
                while (a < 8)
                {
                    byte cT = (byte)(y * 8 + a);
                    var move = new WhiteAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)(y * 8 + x + i));
                    }

                    l.Add(move);
                    a++;
                    offset++;
                }
                moves[cF].Add(l);


                l = [];
                offset = 1;
                var b = y - 1;
                while (b > -1)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new WhiteAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)((y - i) * 8 + x));
                    }

                    l.Add(move);
                    b--;
                    offset++;
                }
                moves[cF].Add(l);

                l = [];
                offset = 1;
                b = y + 1;
                while (b < 8)
                {
                    byte cT = (byte)(b * 8 + x);
                    var move = new WhiteAttack { From = cF, To = cT, Piece = piece };
                    for (int i = 1; i < offset; i++)
                    {
                        move.Set((byte)((y + i) * 8 + x));
                    }

                    l.Add(move);
                    b++;
                    offset++;
                }
                moves[cF].Add(l);
            }
        }
    }

    private static void SetWhiteDiagonalMoves(byte piece, List<List<MoveBase>>[] moves)
    {
        for (byte i = 0; i < _squaresNumber; i++)
        {
            int x = i % 8;
            int y = i / 8;

            int a = x + 1;
            int b = y + 1;

            var l = new List<MoveBase>();
            int to = i + 9;
            while (to < _squaresNumber && a < 8 && b < 8)
            {
                var m = new WhiteMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 9; j <= to; j += 9)
                {
                    m.Set(j);
                }
                l.Add(m);
                to += 9;
                a++;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x - 1;
            b = y + 1;
            to = i + 7;
            while (to < _squaresNumber && a > -1 && b < 8)
            {
                var m = new WhiteMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 7; j <= to; j += 7)
                {
                    m.Set(j);
                }
                l.Add(m);

                to += 7;
                a--;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x + 1;
            b = y - 1;
            to = i - 7;
            while (to > -1 && a < 8 && b > -1)
            {
                var m = new WhiteMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 7; j >= to; j -= 7)
                {
                    m.Set(j);
                }
                l.Add(m);

                to -= 7;
                a++;
                b--;
            }
            moves[i].Add(l);


            l = [];
            a = x - 1;
            b = y - 1;
            to = i - 9;
            while (to > -1 && a > -1 && b > -1)
            {
                var m = new WhiteMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 9; j >= to; j -= 9)
                {
                    m.Set(j);
                }
                l.Add(m);

                to -= 9;
                a--;
                b--;
            }
            moves[i].Add(l);
        }
    }

    private static void SetBlackDiagonalMoves(byte piece, List<List<MoveBase>>[] moves)
    {
        for (byte i = 0; i < _squaresNumber; i++)
        {
            int x = i % 8;
            int y = i / 8;

            int a = x + 1;
            int b = y + 1;

            var l = new List<MoveBase>();
            int to = i + 9;
            while (to < _squaresNumber && a < 8 && b < 8)
            {
                var m = new BlackMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 9; j <= to; j += 9)
                {
                    m.Set(j);
                }
                l.Add(m);
                to += 9;
                a++;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x - 1;
            b = y + 1;
            to = i + 7;
            while (to < _squaresNumber && a > -1 && b < 8)
            {
                var m = new BlackMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 7; j <= to; j += 7)
                {
                    m.Set(j);
                }
                l.Add(m);

                to += 7;
                a--;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x + 1;
            b = y - 1;
            to = i - 7;
            while (to > -1 && a < 8 && b > -1)
            {
                var m = new BlackMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 7; j >= to; j -= 7)
                {
                    m.Set(j);
                }
                l.Add(m);

                to -= 7;
                a++;
                b--;
            }
            moves[i].Add(l);


            l = [];
            a = x - 1;
            b = y - 1;
            to = i - 9;
            while (to > -1 && a > -1 && b > -1)
            {
                var m = new BlackMove { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 9; j >= to; j -= 9)
                {
                    m.Set(j);
                }
                l.Add(m);

                to -= 9;
                a--;
                b--;
            }
            moves[i].Add(l);
        }
    }

    private static void SetBlackDiagonalAttacks(byte piece, List<List<AttackBase>>[] moves)
    {
        for (byte i = 0; i < _squaresNumber; i++)
        {
            int x = i % 8;
            int y = i / 8;

            int a = x + 1;
            int b = y + 1;

            var l = new List<AttackBase>();
            int to = i + 9;
            while (to < _squaresNumber && a < 8 && b < 8)
            {
                var m = new BlackAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 9; j < to; j += 9)
                {
                    m.Set(j);
                }

                l.Add(m);
                to += 9;
                a++;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x - 1;
            b = y + 1;
            to = i + 7;
            while (to < _squaresNumber && a > -1 && b < 8)
            {
                var m = new BlackAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 7; j < to; j += 7)
                {
                    m.Set(j);
                }

                l.Add(m);

                to += 7;
                a--;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x + 1;
            b = y - 1;
            to = i - 7;
            while (to > -1 && a < 8 && b > -1)
            {
                var m = new BlackAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 7; j > to; j -= 7)
                {
                    m.Set(j);
                }

                l.Add(m);

                to -= 7;
                a++;
                b--;
            }
            moves[i].Add(l);

            l = [];
            a = x - 1;
            b = y - 1;
            to = i - 9;
            while (to > -1 && a > -1 && b > -1)
            {
                var m = new BlackAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 9; j > to; j -= 9)
                {
                    m.Set(j);
                }

                l.Add(m);

                to -= 9;
                a--;
                b--;
            }
            moves[i].Add(l);
        }
    }

    private static void SetWhiteDiagonalAttacks(byte piece, List<List<AttackBase>>[] moves)
    {
        for (byte i = 0; i < _squaresNumber; i++)
        {
            int x = i % 8;
            int y = i / 8;

            int a = x + 1;
            int b = y + 1;

            var l = new List<AttackBase>();
            int to = i + 9;
            while (to < _squaresNumber && a < 8 && b < 8)
            {
                var m = new WhiteAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 9; j < to; j += 9)
                {
                    m.Set(j);
                }

                l.Add(m);
                to += 9;
                a++;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x - 1;
            b = y + 1;
            to = i + 7;
            while (to < _squaresNumber && a > -1 && b < 8)
            {
                var m = new WhiteAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i + 7; j < to; j += 7)
                {
                    m.Set(j);
                }

                l.Add(m);

                to += 7;
                a--;
                b++;
            }
            moves[i].Add(l);

            l = [];
            a = x + 1;
            b = y - 1;
            to = i - 7;
            while (to > -1 && a < 8 && b > -1)
            {
                var m = new WhiteAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 7; j > to; j -= 7)
                {
                    m.Set(j);
                }

                l.Add(m);

                to -= 7;
                a++;
                b--;
            }
            moves[i].Add(l);

            l = [];
            a = x - 1;
            b = y - 1;
            to = i - 9;
            while (to > -1 && a > -1 && b > -1)
            {
                var m = new WhiteAttack { From = i, To = (byte)to, Piece = piece };
                for (int j = i - 9; j > to; j -= 9)
                {
                    m.Set(j);
                }

                l.Add(m);

                to -= 9;
                a--;
                b--;
            }
            moves[i].Add(l);
        }
    }

    #endregion
}
