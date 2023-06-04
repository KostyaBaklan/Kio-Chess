using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Config;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Strategies.Models;

namespace Engine.Models.Boards
{
    public class Position : IPosition
    {
        #region Pieces

        const byte WhitePawn = 0;
        const byte WhiteKnight = 1;
        const byte WhiteBishop = 2;
        const byte WhiteRook = 3;
        const byte WhiteQueen = 4;
        const byte WhiteKing = 5;
        const byte BlackPawn = 6;
        const byte BlackKnight = 7;
        const byte BlackBishop = 8;
        const byte BlackRook = 9;
        const byte BlackQueen = 10;
        const byte BlackKing = 11;

        #endregion

        #region Squares

        const byte A1 = 0;
        const byte B1 = 1;
        const byte C1 = 2;
        const byte D1 = 3;
        const byte E1 = 4;
        const byte F1 = 5;
        const byte G1 = 6;
        const byte H1 = 7;
        const byte A2 = 8;
        const byte B2 = 9;
        const byte C2 = 10;
        const byte D2 = 11;
        const byte E2 = 12;
        const byte F2 = 13;
        const byte G2 = 14;
        const byte H2 = 15;
        const byte A3 = 16;
        const byte B3 = 17;
        const byte C3 = 18;
        const byte D3 = 19;
        const byte E3 = 20;
        const byte F3 = 21;
        const byte G3 = 22;
        const byte H3 = 23;
        const byte A4 = 24;
        const byte B4 = 25;
        const byte C4 = 26;
        const byte D4 = 27;
        const byte E4 = 28;
        const byte F4 = 29;
        const byte G4 = 30;
        const byte H4 = 31;
        const byte A5 = 32;
        const byte B5 = 33;
        const byte C5 = 34;
        const byte D5 = 35;
        const byte E5 = 36;
        const byte F5 = 37;
        const byte G5 = 38;
        const byte H5 = 39;
        const byte A6 = 40;
        const byte B6 = 41;
        const byte C6 = 42;
        const byte D6 = 43;
        const byte E6 = 44;
        const byte F6 = 45;
        const byte G6 = 46;
        const byte H6 = 47;
        const byte A7 = 48;
        const byte B7 = 49;
        const byte C7 = 50;
        const byte D7 = 51;
        const byte E7 = 52;
        const byte F7 = 53;
        const byte G7 = 54;
        const byte H7 = 55;
        const byte A8 = 56;
        const byte B8 = 57;
        const byte C8 = 58;
        const byte D8 = 59;
        const byte E8 = 60;
        const byte F8 = 61;
        const byte G8 = 62;
        const byte H8 = 63;

        #endregion

        private Turn _turn;
        private byte _phase;
        private SortContext _sortContext;

        private readonly byte[][] _white;
        private readonly byte[][] _black;
        private readonly byte[][] _whiteAttacks;
        private readonly byte[][] _blackAttacks;

        private readonly SquareList[] _squares;
        private readonly SquareList _promotionSquares;

        private readonly AttackList _attacks;

        private readonly MoveList _moves;

        private readonly PromotionList _promotions;

        private readonly List<PromotionAttackList> _promotionsAttack;

        private readonly SquareList[] _squaresCheck;
        private readonly SquareList _promotionSquaresCheck;

        private readonly AttackList _attacksCheck;
        private readonly MoveList _movesCheck;

        private readonly IBoard _board;
        private readonly IMoveProvider _moveProvider;
        private readonly IMoveHistoryService _moveHistoryService;

        public Position()
        {
            _turn = Turn.White;

            IPieceOrderConfiguration pieceOrderConfiguration = ServiceLocator.Current.GetInstance<IConfigurationProvider>().PieceOrderConfiguration;

            _white = pieceOrderConfiguration.Whites.Select(pair => pair.Value.Select(p => p).ToArray()).ToArray();
            _black = pieceOrderConfiguration.Blacks.Select(pair => pair.Value.Select(p => p).ToArray()).ToArray();
            _whiteAttacks = pieceOrderConfiguration.WhitesAttacks.Select(pair => pair.Value.Select(p => p).ToArray()).ToArray();
            _blackAttacks = pieceOrderConfiguration.BlacksAttacks.Select(pair => pair.Value.Select(p => p).ToArray()).ToArray();

            _squares = new SquareList[6];
            for (int i = 0; i < _squares.Length; i++)
            {
                _squares[i] = new SquareList();
            }
            _promotionSquares = new SquareList();

            _attacks = new AttackList();
            _moves = new MoveList();
            _promotions = new PromotionList();
            _promotionsAttack = new List<PromotionAttackList> { new PromotionAttackList(), new PromotionAttackList() };

            _squaresCheck = new SquareList[6];
            for (int i = 0; i < _squares.Length; i++)
            {
                _squaresCheck[i] = new SquareList();
            }
            _promotionSquaresCheck = new SquareList();

            _attacksCheck = new AttackList();
            _movesCheck = new MoveList();

            _board = new Board();
            _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
            _moveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        #region Implementation of IPosition

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetPiece(byte cell, out byte? piece)
        {
            return _board.GetPiece(cell, out piece);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetKey()
        {
            return _board.GetKey();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetValue()
        {
            if (_turn == Turn.White)
                return _board.GetValue();
            return (short)-_board.GetValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetStaticValue()
        {
            if (_turn == Turn.White)
                return _board.GetStaticValue();
            return (short)-_board.GetStaticValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKingSafetyValue()
        {
            if (_turn == Turn.White)
                return _board.GetKingSafetyValue();
            return (short)-_board.GetKingSafetyValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPawnValue()
        {
            if (_turn == Turn.White)
                return _board.GetPawnValue();
            return (short)-_board.GetPawnValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOpponentMaxValue()
        {
            if (_turn == Turn.White)
                return _board.GetBlackMaxValue();
            return _board.GetWhiteMaxValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Turn GetTurn()
        {
            return _turn;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<MoveBase> GetAllMoves(byte cell, byte piece)
        {
            _moveProvider.GetMoves(piece, cell, _moves);
            _moveProvider.GetAttacks(piece, cell, _attacks);
            _moveProvider.GetPromotions(piece, cell, _promotions);
            _moveProvider.GetPromotions(piece, cell, _promotionsAttack);

            IEnumerable<MoveBase> moves = _moves.Concat(_attacks).Concat(_promotions).Concat(_promotionsAttack.SelectMany(p => p));

            return _turn == Turn.White
                ? moves.Where(a => IsWhiteLigal(a))
                : moves.Where(a => IsBlackLigal(a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhitePromotionAttacks(AttackList attacks)
        {
            BitList squares = stackalloc byte[8];
            _board.GetWhitePromotionSquares(ref squares);

            BitBoard to = new BitBoard();

            for (byte i = 0; i < squares.Count; i++)
            {
                var promotions = _moveProvider.GetWhitePromotionAttacks(squares[i]);

                for (byte j = 0; j < promotions.Length; j++)
                {
                    if (promotions[j].Count > 0)
                    {
                        var attack = promotions[j][0];
                        if (to.IsSet(attack.To)) continue;

                        if (IsWhiteLigal(attack))
                        {
                            attacks.Add(attack);
                        }
                        to |= attack.To.AsBitBoard();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackPromotionAttacks(AttackList attacks)
        {
            BitList squares = stackalloc byte[8];
            _board.GetBlackPromotionSquares(ref squares);
            BitBoard to = new BitBoard();

            for (byte i = 0; i < _promotionSquares.Length; i++)
            {
                var promotions = _moveProvider.GetBlackPromotionAttacks(squares[i]);

                for (byte j = 0; j < promotions.Length; j++)
                {
                    if (promotions[j].Count > 0)
                    {
                        var attack = promotions[j][0];
                        if (to.IsSet(attack.To)) continue;

                        if (IsBlackLigal(attack))
                        {
                            attacks.Add(attack);
                        }
                        to |= attack.To.AsBitBoard();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteAttacks(AttackList attacks)
        {
            BitList squares = stackalloc byte[8];
            BitBoard to = new BitBoard();

            _attacks.Clear();

            GenerateAllWhiteAttacks(ref squares);

            for (byte i = 0; i < _attacks.Count; i++)
            {
                var attack = _attacks[i];
                if (to.IsSet(attack.To)) continue;

                if (IsWhiteLigal(attack))
                {
                    attacks.Add(attack);
                }
                to |= attack.To.AsBitBoard();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackAttacks(AttackList attacks)
        {
            BitList squares = stackalloc byte[8];
            BitBoard to = new BitBoard();

            _attacks.Clear();

            GenerateAllBlackAttacks(ref squares);

            for (byte i = 0; i < _attacks.Count; i++)
            {
                var attack = _attacks[i];
                if (to.IsSet(attack.To)) continue;

                if (IsBlackLigal(attack))
                {
                    attacks.Add(attack);
                }
                to |= attack.To.AsBitBoard();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveList GetAllAttacks(SortContext sortContext)
        {
            //var timer = Stopwatch.StartNew();
            //try
            //{
                _sortContext = sortContext;

                if (_turn == Turn.White)
                {
                    GetAllWhiteAttacks();
                }
                else
                {
                    GetAllBlackAttacks();
                }

                return sortContext.GetMoves();
            //}
            //finally
            //{
            //    timer.Stop();
            //    MoveGenerationPerformance.Add(nameof(GetAllAttacks), timer.Elapsed);
            //}
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetAllBlackAttacks()
        {
            BitList squares = stackalloc byte[8];
            _attacks.Clear();

            GenerateAllBlackAttacks(ref squares);

            for (byte i = 0; i < _attacks.Count; i++)
            {
                if (IsBlackLigal(_attacks[i]))
                {
                    _sortContext.ProcessCaptureMove(_attacks[i]);
                }
            }

            if (_board.CanBlackPromote())
            {
                _board.GetBlackPromotionSquares(ref squares);

                ProcessBlackPromotionCapuresWithoutPv(ref squares);

                ProcessBlackPromotionsWithoutPv(ref squares);
            }
        }

        private void GenerateAllBlackAttacks(ref BitList squares)
        {
            _board.GetBlackPawnSquares(ref squares);
            _moveProvider.GetBlackPawnAttacks(ref squares, _attacks);


            _board.GetSquares(Pieces.BlackKnight, ref squares);
            _moveProvider.GetBlackKnightAttacks(ref squares, _attacks);

            _board.GetSquares(Pieces.BlackBishop, ref squares);
            _moveProvider.GetBlackBishopAttacks(ref squares, _attacks);

            _board.GetSquares(Pieces.BlackRook, ref squares);
            _moveProvider.GetBlackRookAttacks(ref squares, _attacks);

            _board.GetSquares(Pieces.BlackQueen, ref squares);
            _moveProvider.GetBlackQueenAttacks(ref squares, _attacks);

            _board.GetSquares(Pieces.BlackKing, ref squares);
            _moveProvider.GetBlackKingAttacks(ref squares, _attacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetAllWhiteAttacks()
        {
            //Span<byte> squares = stackalloc byte[12];
            BitList squares = stackalloc byte[8];
            _attacks.Clear();

            GenerateAllWhiteAttacks(ref squares);

            for (byte i = 0; i < _attacks.Count; i++)
            {
                if (IsWhiteLigal(_attacks[i]))
                {
                    _sortContext.ProcessCaptureMove(_attacks[i]);
                }
            }

            if (_board.CanWhitePromote())
            {
                _board.GetWhitePromotionSquares(ref squares);

                ProcessWhitePromotionCapuresWithoutPv(ref squares);

                ProcessWhitePromotionsWithoutPv(ref squares);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GenerateAllWhiteMoves(ref BitList squares)
        {
            _board.GetWhitePawnSquares(ref squares);
            _moveProvider.GetWhitePawnMoves(ref squares, _moves);

            _board.GetSquares(Pieces.WhiteKnight, ref squares);
            _moveProvider.GetWhiteKnightMoves(ref squares, _moves);

            _board.GetSquares(Pieces.WhiteBishop, ref squares);
            _moveProvider.GetWhiteBishopMoves(ref squares, _moves);

            _board.GetSquares(Pieces.WhiteRook, ref squares);
            _moveProvider.GetWhiteRookMoves(ref squares, _moves);

            _board.GetSquares(Pieces.WhiteQueen, ref squares);
            _moveProvider.GetWhiteQueenMoves(ref squares, _moves);

            _board.GetSquares(Pieces.WhiteKing, ref squares);
            _moveProvider.GetWhiteKingMoves(ref squares, _moves);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GenerateAllBlackMoves(ref BitList squares)
        {
            _board.GetBlackPawnSquares(ref squares);
            _moveProvider.GetBlackPawnMoves(ref squares, _moves);

            _board.GetSquares(Pieces.BlackKnight, ref squares);
            _moveProvider.GetBlackKnightMoves(ref squares, _moves);

            _board.GetSquares(Pieces.BlackBishop, ref squares);
            _moveProvider.GetBlackBishopMoves(ref squares, _moves);

            _board.GetSquares(Pieces.BlackRook, ref squares);
            _moveProvider.GetBlackRookMoves(ref squares, _moves);

            _board.GetSquares(Pieces.BlackQueen, ref squares);
            _moveProvider.GetBlackQueenMoves(ref squares, _moves);

            _board.GetSquares(Pieces.BlackKing, ref squares);
            _moveProvider.GetBlackKingMoves(ref squares, _moves);
        }

        private void GenerateAllWhiteAttacks(ref BitList squares)
        {
            _board.GetWhitePawnSquares(ref squares);
            _moveProvider.GetWhitePawnAttacks(ref squares, _attacks);

            _board.GetSquares(Pieces.WhiteKnight, ref squares);
            _moveProvider.GetWhiteKnightAttacks(ref squares, _attacks);

            _board.GetSquares(Pieces.WhiteBishop, ref squares);
            _moveProvider.GetWhiteBishopAttacks(ref squares, _attacks);

            _board.GetSquares(Pieces.WhiteRook, ref squares);
            _moveProvider.GetWhiteRookAttacks(ref squares, _attacks);

            _board.GetSquares(Pieces.WhiteQueen, ref squares);
            _moveProvider.GetWhiteQueenAttacks(ref squares, _attacks);

            _board.GetSquares(Pieces.WhiteKing, ref squares);
            _moveProvider.GetWhiteKingAttacks(ref squares, _attacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveList GetAllMoves(SortContext sortContext)
        {
            //var timer = Stopwatch.StartNew();
            //try
            //{
                _sortContext = sortContext;

                if (_turn == Turn.White)
                {
                    GetAllWhiteMoves();
                }
                else
                {
                    GetAllBlackMoves();
                }
                return sortContext.GetMoves();
            //}
            //finally
            //{
            //    timer.Stop();
            //    MoveGenerationPerformance.Add(nameof(GetAllMoves), timer.Elapsed);
            //}
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetAllWhiteMoves()
        {
            BitList squares = stackalloc byte[8];
            _attacks.Clear();
            _moves.Clear();

            GenerateAllWhites(ref squares);

            if (_sortContext.HasPv)
            {
                if (_sortContext.IsPvCapture)
                {
                    ProcessWhiteCapuresWithPv();
                    if (_board.CanWhitePromote())
                    {
                        _board.GetWhitePromotionSquares(ref squares);
                        ProcessWhitePromotionCapuresWithPv(ref squares);

                        ProcessWhitePromotionsWithoutPv(ref squares);
                    }
                    ProcessWhiteMovesWithoutPv();
                }
                else
                {
                    ProcessWhiteCapuresWithoutPv();
                    if (_board.CanWhitePromote())
                    {
                        _board.GetWhitePromotionSquares(ref squares);
                        ProcessWhitePromotionCapuresWithoutPv(ref squares);

                        ProcessWhitePromotionsWithPv(ref squares);
                    }
                    ProcessWhiteMovesWithPv();
                }
            }
            else
            {
                ProcessWhiteCapuresWithoutPv();
                if (_board.CanWhitePromote())
                {
                    _board.GetWhitePromotionSquares(ref squares);
                    ProcessWhitePromotionCapuresWithoutPv(ref squares);

                    ProcessWhitePromotionsWithoutPv(ref squares);
                }
                ProcessWhiteMovesWithoutPv();
            }
        }

        private void GenerateAllWhites(ref BitList squares)
        {
            _board.GetWhitePawnSquares(ref squares);
            _moveProvider.GetWhitePawnAttacks(ref squares, _attacks);
            _moveProvider.GetWhitePawnMoves(ref squares, _moves);

            _board.GetSquares(Pieces.WhiteKnight, ref squares);
            _moveProvider.GetWhiteKnightAttacks(ref squares, _attacks);
            _moveProvider.GetWhiteKnightMoves(ref squares, _moves);

            _board.GetSquares(Pieces.WhiteBishop, ref squares);
            _moveProvider.GetWhiteBishopAttacks(ref squares, _attacks);
            _moveProvider.GetWhiteBishopMoves(ref squares, _moves);

            _board.GetSquares(Pieces.WhiteRook, ref squares);
            _moveProvider.GetWhiteRookAttacks(ref squares, _attacks);
            _moveProvider.GetWhiteRookMoves(ref squares, _moves);

            _board.GetSquares(Pieces.WhiteQueen, ref squares);
            _moveProvider.GetWhiteQueenAttacks(ref squares, _attacks);
            _moveProvider.GetWhiteQueenMoves(ref squares, _moves);

            _board.GetSquares(Pieces.WhiteKing, ref squares);
            _moveProvider.GetWhiteKingAttacks(ref squares, _attacks);
            _moveProvider.GetWhiteKingMoves(ref squares, _moves);
        }

        private void GenerateAllBlacks(ref BitList squares)
        {
            _board.GetBlackPawnSquares(ref squares);
            _moveProvider.GetBlackPawnAttacks(ref squares, _attacks);
            _moveProvider.GetBlackPawnMoves(ref squares, _moves);

            _board.GetSquares(Pieces.BlackKnight, ref squares);
            _moveProvider.GetBlackKnightAttacks(ref squares, _attacks);
            _moveProvider.GetBlackKnightMoves(ref squares, _moves);

            _board.GetSquares(Pieces.BlackBishop, ref squares);
            _moveProvider.GetBlackBishopAttacks(ref squares, _attacks);
            _moveProvider.GetBlackBishopMoves(ref squares, _moves);

            _board.GetSquares(Pieces.BlackRook, ref squares);
            _moveProvider.GetBlackRookAttacks(ref squares, _attacks);
            _moveProvider.GetBlackRookMoves(ref squares, _moves);

            _board.GetSquares(Pieces.BlackQueen, ref squares);
            _moveProvider.GetBlackQueenAttacks(ref squares, _attacks);
            _moveProvider.GetBlackQueenMoves(ref squares, _moves);

            _board.GetSquares(Pieces.BlackKing, ref squares);
            _moveProvider.GetBlackKingAttacks(ref squares, _attacks);
            _moveProvider.GetBlackKingMoves(ref squares, _moves);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetAllBlackMoves()
        {
            BitList squares = stackalloc byte[8];
            _attacks.Clear();
            _moves.Clear();

            GenerateAllBlacks(ref squares);

            if (_sortContext.HasPv)
            {
                if (_sortContext.IsPvCapture)
                {
                    ProcessBlackCapuresWithPv();
                    if (_board.CanBlackPromote())
                    {
                        _board.GetBlackPromotionSquares(ref squares);
                        ProcessBlackPromotionCapuresWithPv(ref squares);

                        ProcessBlackPromotionsWithoutPv(ref squares);
                    }
                    ProcessBlackMovesWithoutPv();
                }
                else
                {
                    ProcessBlackCapuresWithoutPv();
                    if (_board.CanBlackPromote())
                    {
                        _board.GetBlackPromotionSquares(ref squares);
                        ProcessBlackPromotionCapuresWithoutPv(ref squares);

                        ProcessBlackPromotionsWithPv(ref squares);
                    }
                    ProcessBlackMovesWithPv();
                }
            }
            else
            {
                ProcessBlackCapuresWithoutPv();
                if (_board.CanBlackPromote())
                {
                    _board.GetBlackPromotionSquares(ref squares);
                    ProcessBlackPromotionCapuresWithoutPv(ref squares);

                    ProcessBlackPromotionsWithoutPv(ref squares);
                }
                ProcessBlackMovesWithoutPv();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhitePromotionCapuresWithPv()
        {
            for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                var promotions = _moveProvider.GetWhitePromotionAttacks(_sortContext.PromotionSquares[f]);

                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count == 0 || !IsWhiteLigal(promotions[i][0]))
                        continue;


                    if (promotions[i].HasPv(_sortContext.Pv))
                    {
                        _sortContext.ProcessHashMoves(promotions[i]);
                    }
                    else
                    {
                        _sortContext.ProcessPromotionCaptures(promotions[i]);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhitePromotionCapuresWithPv(ref BitList squares)
        {
            for (byte f = 0; f < squares.Count; f++)
            {
                var promotions = _moveProvider.GetWhitePromotionAttacks(squares[f]);

                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count == 0 || !IsWhiteLigal(promotions[i][0]))
                        continue;


                    if (promotions[i].HasPv(_sortContext.Pv))
                    {
                        _sortContext.ProcessHashMoves(promotions[i]);
                    }
                    else
                    {
                        _sortContext.ProcessPromotionCaptures(promotions[i]);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhitePromotionCapuresWithoutPv(ref BitList squares)
        {
            for (byte f = 0; f < squares.Count; f++)
            {
                var promotions = _moveProvider.GetWhitePromotionAttacks(squares[f]);

                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count != 0 && IsWhiteLigal(promotions[i][0]))
                        _sortContext.ProcessPromotionCaptures(promotions[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhitePromotionCapuresWithoutPv()
        {
            for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                var promotions = _moveProvider.GetWhitePromotionAttacks(_sortContext.PromotionSquares[f]);

                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count != 0 && IsWhiteLigal(promotions[i][0]))
                        _sortContext.ProcessPromotionCaptures(promotions[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackPromotionCapuresWithPv()
        {
            for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                var promotions = _moveProvider.GetBlackPromotionAttacks(_sortContext.PromotionSquares[f]);

                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count == 0 || !IsBlackLigal(promotions[i][0]))
                        continue;

                    if (promotions[i].HasPv(_sortContext.Pv))
                    {
                        _sortContext.ProcessHashMoves(promotions[i]);
                    }
                    else
                    {
                        _sortContext.ProcessPromotionCaptures(promotions[i]);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackPromotionCapuresWithPv(ref BitList squares)
        {
            for (byte f = 0; f < squares.Count; f++)
            {
                var promotions = _moveProvider.GetBlackPromotionAttacks(squares[f]);

                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count == 0 || !IsBlackLigal(promotions[i][0]))
                        continue;

                    if (promotions[i].HasPv(_sortContext.Pv))
                    {
                        _sortContext.ProcessHashMoves(promotions[i]);
                    }
                    else
                    {
                        _sortContext.ProcessPromotionCaptures(promotions[i]);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackPromotionCapuresWithoutPv()
        {
            for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                var promotions = _moveProvider.GetBlackPromotionAttacks(_sortContext.PromotionSquares[f]);

                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count != 0 && IsBlackLigal(promotions[i][0]))
                        _sortContext.ProcessPromotionCaptures(promotions[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackPromotionCapuresWithoutPv(ref BitList squares)
        {
            for (byte f = 0; f < squares.Count; f++)
            {
                var promotions = _moveProvider.GetBlackPromotionAttacks(squares[f]);

                for (byte i = 0; i < promotions.Length; i++)
                {
                    if (promotions[i].Count != 0 && IsBlackLigal(promotions[i][0]))
                        _sortContext.ProcessPromotionCaptures(promotions[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhitePromotionsWithPv()
        {
            for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                var promotions = _moveProvider.GetWhitePromotions(_sortContext.PromotionSquares[f]);

                if (promotions.Count == 0 || !IsWhiteLigal(promotions[0]))
                    continue;

                if (promotions.HasPv(_sortContext.Pv))
                {
                    _sortContext.ProcessHashMoves(promotions);
                }
                else
                {
                    _sortContext.ProcessPromotionMoves(promotions);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhitePromotionsWithPv(ref BitList squares)
        {
            for (byte f = 0; f < squares.Count; f++)
            {
                var promotions = _moveProvider.GetWhitePromotions(squares[f]);

                if (promotions.Count == 0 || !IsWhiteLigal(promotions[0]))
                    continue;

                if (promotions.HasPv(_sortContext.Pv))
                {
                    _sortContext.ProcessHashMoves(promotions);
                }
                else
                {
                    _sortContext.ProcessPromotionMoves(promotions);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhitePromotionsWithoutPv(ref BitList squares)
        {
            for (byte f = 0; f < squares.Count; f++)
            {
                var promotions = _moveProvider.GetWhitePromotions(squares[f]);

                if (promotions.Count > 0 && IsWhiteLigal(promotions[0]))
                {
                    _sortContext.ProcessPromotionMoves(promotions);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhitePromotionsWithoutPv()
        {
            for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                var promotions = _moveProvider.GetWhitePromotions(_sortContext.PromotionSquares[f]);

                if (promotions.Count > 0 && IsWhiteLigal(promotions[0]))
                {
                    _sortContext.ProcessPromotionMoves(promotions);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackPromotionsWithPv()
        {
            for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                var promotions = _moveProvider.GetBlackPromotions(_sortContext.PromotionSquares[f]);

                if (promotions.Count <= 0 || !IsBlackLigal(promotions[0]))
                    continue;

                if (promotions.HasPv(_sortContext.Pv))
                {
                    _sortContext.ProcessHashMoves(promotions);
                }
                else
                {
                    _sortContext.ProcessPromotionMoves(promotions);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackPromotionsWithPv(ref BitList squares)
        {
            for (byte f = 0; f < squares.Count; f++)
            {
                var promotions = _moveProvider.GetBlackPromotions(squares[f]);

                if (promotions.Count <= 0 || !IsBlackLigal(promotions[0]))
                    continue;

                if (promotions.HasPv(_sortContext.Pv))
                {
                    _sortContext.ProcessHashMoves(promotions);
                }
                else
                {
                    _sortContext.ProcessPromotionMoves(promotions);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackPromotionsWithoutPv()
        {
            for (byte f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                var promotions = _moveProvider.GetBlackPromotions(_sortContext.PromotionSquares[f]);

                if (promotions.Count > 0 && IsBlackLigal(promotions[0]))
                {
                    _sortContext.ProcessPromotionMoves(promotions);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackPromotionsWithoutPv(ref BitList squares)
        {
            for (byte f = 0; f < squares.Count; f++)
            {
                var promotions = _moveProvider.GetBlackPromotions(squares[f]);

                if (promotions.Count > 0 && IsBlackLigal(promotions[0]))
                {
                    _sortContext.ProcessPromotionMoves(promotions);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteCapuresWithPv()
        {
            for (byte i = 0; i < _attacks.Count; i++)
            {
                var capture = _attacks[i];
                if (!IsWhiteLigal(capture))
                    continue;

                if (_sortContext.Pv != capture.Key)
                {
                    _sortContext.ProcessCaptureMove(capture);
                }
                else
                {
                    _sortContext.ProcessHashMove(capture);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteCapuresWithPv(ref BitList squares)
        {
            for (byte i = 0; i < _attacks.Count; i++)
            {
                var capture = _attacks[i];
                if (!IsWhiteLigal(capture))
                    continue;

                if (_sortContext.Pv != capture.Key)
                {
                    _sortContext.ProcessCaptureMove(capture);
                }
                else
                {
                    _sortContext.ProcessHashMove(capture);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteMovesWithPv(ref BitList squares)
        {
            _moves.Clear();

            GenerateAllWhiteMoves(ref squares);

            for (byte i = 0; i < _moves.Count; i++)
            {
                var move = _moves[i];
                if (!IsWhiteLigal(move))
                    continue;

                if (_sortContext.Pv == move.Key)
                {
                    _sortContext.ProcessHashMove(move);
                }
                else if (_sortContext.IsKiller(move.Key))
                {
                    _sortContext.ProcessKillerMove(move);
                }
                else
                {
                    _sortContext.ProcessMove(move);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteMovesWithPv()
        {
            for (byte i = 0; i < _moves.Count; i++)
            {
                var move = _moves[i];
                if (!IsWhiteLigal(move))
                    continue;

                if (_sortContext.Pv == move.Key)
                {
                    _sortContext.ProcessHashMove(move);
                }
                else if (_sortContext.IsKiller(move.Key))
                {
                    _sortContext.ProcessKillerMove(move);
                }
                else
                {
                    _sortContext.ProcessMove(move);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteCapuresWithoutPv()
        {
            for (byte i = 0; i < _attacks.Count; i++)
            {
                if (IsWhiteLigal(_attacks[i]))
                {
                    _sortContext.ProcessCaptureMove(_attacks[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteCapuresWithoutPv(ref BitList squares)
        {
            _attacks.Clear();

            GenerateAllWhiteAttacks(ref squares);

            for (byte i = 0; i < _attacks.Count; i++)
            {
                if (IsWhiteLigal(_attacks[i]))
                {
                    _sortContext.ProcessCaptureMove(_attacks[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteMovesWithoutPv(ref BitList squares)
        {
            _moves.Clear();

            GenerateAllWhiteMoves(ref squares);

            for (byte i = 0; i < _moves.Count; i++)
            {
                var move = _moves[i];
                if (!IsWhiteLigal(move))
                    continue;

                if (_sortContext.IsKiller(move.Key))
                {
                    _sortContext.ProcessKillerMove(move);
                }
                else
                {
                    _sortContext.ProcessMove(move);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteMovesWithoutPv()
        {
            for (byte i = 0; i < _moves.Count; i++)
            {
                var move = _moves[i];
                if (!IsWhiteLigal(move))
                    continue;

                if (_sortContext.IsKiller(move.Key))
                {
                    _sortContext.ProcessKillerMove(move);
                }
                else
                {
                    _sortContext.ProcessMove(move);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackCapuresWithPv()
        {
            for (byte i = 0; i < _attacks.Count; i++)
            {
                var capture = _attacks[i];
                if (!IsBlackLigal(capture))
                    continue;

                if (_sortContext.Pv != capture.Key)
                {
                    _sortContext.ProcessCaptureMove(capture);
                }
                else
                {
                    _sortContext.ProcessHashMove(capture);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackCapuresWithPv(ref BitList squares)
        {
            _attacks.Clear();

            GenerateAllBlackAttacks(ref squares);

            for (byte i = 0; i < _attacks.Count; i++)
            {
                var capture = _attacks[i];
                if (!IsBlackLigal(capture))
                    continue;

                if (_sortContext.Pv != capture.Key)
                {
                    _sortContext.ProcessCaptureMove(capture);
                }
                else
                {
                    _sortContext.ProcessHashMove(capture);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackMovesWithPv(ref BitList squares)
        {
            _moves.Clear();

            GenerateAllBlackMoves(ref squares);

            for (byte i = 0; i < _moves.Count; i++)
            {
                var move = _moves[i];
                if (!IsBlackLigal(move))
                    continue;

                if (_sortContext.Pv == move.Key)
                {
                    _sortContext.ProcessHashMove(move);
                }
                else if (_sortContext.IsKiller(move.Key))
                {
                    _sortContext.ProcessKillerMove(move);
                }
                else
                {
                    _sortContext.ProcessMove(move);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackMovesWithPv()
        {
            for (byte i = 0; i < _moves.Count; i++)
            {
                var move = _moves[i];
                if (!IsBlackLigal(move))
                    continue;

                if (_sortContext.Pv == move.Key)
                {
                    _sortContext.ProcessHashMove(move);
                }
                else if (_sortContext.IsKiller(move.Key))
                {
                    _sortContext.ProcessKillerMove(move);
                }
                else
                {
                    _sortContext.ProcessMove(move);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackCapuresWithoutPv()
        {
            for (byte i = 0; i < _attacks.Count; i++)
            {
                if (IsBlackLigal(_attacks[i]))
                {
                    _sortContext.ProcessCaptureMove(_attacks[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackCapuresWithoutPv(ref BitList squares)
        {
            _attacks.Clear();

            GenerateAllBlackAttacks(ref squares);

            for (byte i = 0; i < _attacks.Count; i++)
            {
                if (IsBlackLigal(_attacks[i]))
                {
                    _sortContext.ProcessCaptureMove(_attacks[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackMovesWithoutPv(ref BitList squares)
        {
            _moves.Clear();

            GenerateAllBlackMoves(ref squares);

            for (byte i = 0; i < _moves.Count; i++)
            {
                var move = _moves[i];
                if (!IsBlackLigal(move))
                    continue;

                if (_sortContext.IsKiller(move.Key))
                {
                    _sortContext.ProcessKillerMove(move);
                }
                else
                {
                    _sortContext.ProcessMove(move);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackMovesWithoutPv()
        {
            for (byte i = 0; i < _moves.Count; i++)
            {
                var move = _moves[i];
                if (!IsBlackLigal(move))
                    continue;

                if (_sortContext.IsKiller(move.Key))
                {
                    _sortContext.ProcessKillerMove(move);
                }
                else
                {
                    _sortContext.ProcessMove(move);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GenerateWhiteAttacks(SquareList[] Squares)
        {
            _moveProvider.GetWhitePawnAttacks(Squares[WhitePawn], _attacks);
            _moveProvider.GetWhiteKnightAttacks(Squares[WhiteKnight], _attacks);
            _moveProvider.GetWhiteBishopAttacks(Squares[WhiteBishop], _attacks);
            _moveProvider.GetWhiteRookAttacks(Squares[WhiteRook], _attacks);
            _moveProvider.GetWhiteQueenAttacks(Squares[WhiteQueen], _attacks);
            _moveProvider.GetWhiteKingAttacks(Squares[WhiteKing], _attacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GenerateBlackAttacks(SquareList[] Squares)
        {
            _moveProvider.GetBlackPawnAttacks(Squares[WhitePawn], _attacks);
            _moveProvider.GetBlackKnightAttacks(Squares[WhiteKnight], _attacks);
            _moveProvider.GetBlackBishopAttacks(Squares[WhiteBishop], _attacks);
            _moveProvider.GetBlackRookAttacks(Squares[WhiteRook], _attacks);
            _moveProvider.GetBlackQueenAttacks(Squares[WhiteQueen], _attacks);
            _moveProvider.GetBlackKingAttacks(Squares[WhiteKing], _attacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GenerateWhiteMoves(SquareList[] Squares)
        {
            _moveProvider.GetWhitePawnMoves(Squares[WhitePawn], _moves);
            _moveProvider.GetWhiteKnightMoves(Squares[WhiteKnight], _moves);
            _moveProvider.GetWhiteBishopMoves(Squares[WhiteBishop], _moves);
            _moveProvider.GetWhiteRookMoves(Squares[WhiteRook], _moves);
            _moveProvider.GetWhiteQueenMoves(Squares[WhiteQueen], _moves);
            _moveProvider.GetWhiteKingMoves(Squares[WhiteKing], _moves);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GenerateBlackMoves(SquareList[] Squares)
        {
            _moveProvider.GetBlackPawnMoves(Squares[WhitePawn], _moves);
            _moveProvider.GetBlackKnightMoves(Squares[WhiteKnight], _moves);
            _moveProvider.GetBlackBishopMoves(Squares[WhiteBishop], _moves);
            _moveProvider.GetBlackRookMoves(Squares[WhiteRook], _moves);
            _moveProvider.GetBlackQueenMoves(Squares[WhiteQueen], _moves);
            _moveProvider.GetBlackKingMoves(Squares[WhiteKing], _moves);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PossibleSingleWhiteAttacks(SquareList[] squares, AttackList attacks)
        {
            BitBoard to = new BitBoard();

            _attacks.Clear();

            GenerateWhiteAttacks(squares);

            for (byte i = 0; i < _attacks.Count; i++)
            {
                var attack = _attacks[i];
                if (to.IsSet(attack.To)) continue;

                if (IsWhiteLigal(attack))
                {
                    attacks.Add(attack);
                }
                to |= attack.To.AsBitBoard();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PossibleSingleBlackAttacks(SquareList[] squares, AttackList attacks)
        {
            BitBoard to = new BitBoard();

            _attacks.Clear();

            GenerateBlackAttacks(squares);

            for (byte i = 0; i < _attacks.Count; i++)
            {
                var attack = _attacks[i];
                if (to.IsSet(attack.To)) continue;

                if (IsBlackLigal(attack))
                {
                    attacks.Add(attack);
                }
                to |= attack.To.AsBitBoard();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhiteAttacksTo(byte to, AttackList attackList)
        {
            attackList.Clear();

            _moveProvider.GetWhiteAttacksToForPromotion(to, _attacks);

            for (byte i = 0; i < _attacks.Count; i++)
            {
                if (IsWhiteLigal(_attacks[i]))
                {
                    attackList.Add(_attacks[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackAttacksTo(byte to, AttackList attackList)
        {
            attackList.Clear();

            _moveProvider.GetBlackAttacksToForPromotion(to, _attacks);

            for (byte i = 0; i < _attacks.Count; i++)
            {
                if (IsBlackLigal(_attacks[i]))
                {
                    attackList.Add(_attacks[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBoard GetBoard()
        {
            return _board;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<MoveBase> GetHistory()
        {
            return _moveHistoryService.GetHistory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsWhiteNotLegal(MoveBase move)
        {
            return _board.IsBlackAttacksTo(_board.GetWhiteKingPosition()) ||
                (move.IsCastle && _board.IsBlackAttacksTo(move.To == C1 ? D1 : F1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlackNotLegal(MoveBase move)
        {
            return _board.IsWhiteAttacksTo(_board.GetBlackKingPosition()) ||
                 (move.IsCastle && _board.IsWhiteAttacksTo(move.To == C8 ? D8 : F8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetPhase()
        {
            return _phase;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanWhitePromote()
        {
            return _board.CanWhitePromote();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanBlackPromote()
        {
            return _board.CanBlackPromote();
        }

        public void SaveHistory()
        {
            var moveFormatter = ServiceLocator.Current.GetInstance<IMoveFormatter>();
            IEnumerable<MoveBase> history = GetHistory();
            List<string> moves = new List<string>();
            bool isWhite = true;
            StringBuilder builder = new StringBuilder();
            foreach (var move in history)
            {
                if (isWhite)
                {
                    builder = new StringBuilder();
                    builder.Append($"W={moveFormatter.Format(move)} ");
                }
                else
                {
                    builder.Append($"B={moveFormatter.Format(move)} ");
                    moves.Add(builder.ToString());
                }
                isWhite = !isWhite;
            }
            var path = "History";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllLines($@"{path}\\{DateTime.Now:yyyy_MM_dd_hh_mm_ss}.txt", moves);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetWhiteSquares(byte[] pieces, SquareList[] squares)
        {
            _board.GetWhitePawnSquares(squares[0]);

            for (byte i = 1; i < 6; i++)
            {
                _board.GetSquares(pieces[i], squares[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetBlackSquares(byte[] pieces, SquareList[] squares)
        {
            _board.GetBlackPawnSquares(squares[0]);

            for (byte i = 1; i < 6; i++)
            {
                _board.GetSquares(pieces[i], squares[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetWhiteSquares(byte[] pieces)
        {
            _board.GetWhitePawnSquares(_squares[0]);

            for (byte i = 1; i < 6; i++)
            {
                _board.GetSquares(pieces[i], _squares[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetBlackSquares(byte[] pieces)
        {
            _board.GetBlackPawnSquares(_squares[0]);
            for (byte i = 1; i < 6; i++)
            {
                _board.GetSquares(pieces[i], _squares[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MakeFirst(MoveBase move)
        {
            _moveHistoryService.AddFirst(move);

            move.Make();

            move.IsCheck = _turn != Turn.White
                ? _board.IsBlackAttacksTo(_board.GetWhiteKingPosition())
                : _board.IsWhiteAttacksTo(_board.GetBlackKingPosition());

            _phase = _board.UpdatePhase();

            _moveHistoryService.Add(_board.GetKey());

            SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Make(MoveBase move)
        {
            _moveHistoryService.Add(move);

            move.Make();

            move.IsCheck = _turn != Turn.White
                ? _board.IsBlackAttacksTo(_board.GetWhiteKingPosition())
                : _board.IsWhiteAttacksTo(_board.GetBlackKingPosition());

            _phase = _board.UpdatePhase();

            _moveHistoryService.Add(_board.GetKey());

            SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnMake()
        {
            _moveHistoryService.Remove(_board.GetKey());
            MoveBase move = _moveHistoryService.Remove();

            move.UnMake();

            move.IsCheck = false;

            _phase = _board.UpdatePhase();

            SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Do(MoveBase move)
        {
            move.Make();

            SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnDo(MoveBase move)
        {
            move.UnMake();

            SwapTurn();
        }

        #endregion


        #region Any Moves/Captures

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AnyWhiteMoves()
        {
            GetWhiteSquares(_white[_phase], _squaresCheck);

            return AnyWhiteMove() || AnyWhiteCapture() || AnyWhitePromotion();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyLigalWhiteCapture()
        {
            if (_attacksCheck.Count == 0) return false;

            for (byte i = 0; i < _attacksCheck.Count; i++)
            {
                if (IsWhiteLigal(_attacksCheck[i]))
                    return true;
            }

            _attacksCheck.Count = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyLigalBlackCapture()
        {
            if (_attacksCheck.Count == 0) return false;

            for (byte i = 0; i < _attacksCheck.Count; i++)
            {
                if (IsBlackLigal(_attacksCheck[i]))
                    return true;
            }

            _attacksCheck.Count = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyLigalWhiteMoves()
        {
            if (_movesCheck.Count == 0) return false;

            for (byte i = 0; i < _movesCheck.Count; i++)
            {
                if (IsWhiteLigal(_movesCheck[i]))
                    return true;
            }

            _movesCheck.Count = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyLigalBlackMoves()
        {
            if (_movesCheck.Count == 0) return false;

            for (byte i = 0; i < _movesCheck.Count; i++)
            {
                if (IsBlackLigal(_movesCheck[i]))
                    return true;
            }

            _movesCheck.Count = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyWhiteCapture()
        {
            _attacksCheck.Clear();

            return AnyWhitePawnCapture() || AnyWhiteKnightCapture() || AnyWhiteBishopCapture()
                || AnyWhiteRookCapture() || AnyWhiteQueenCapture() || AnyWhiteKingCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyBlackCapture()
        {
            _attacksCheck.Clear();

            return AnyBlackPawnCapture() || AnyBlackKnightCapture() || AnyBlackBishopCapture()
                || AnyBlackRookCapture() || AnyBlackQueenCapture() || AnyBlackKingCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyWhiteKingCapture()
        {
            _moveProvider.GetWhiteKingAttacks(_squaresCheck[WhiteKing], _attacksCheck);
            return AnyLigalWhiteCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyWhiteQueenCapture()
        {
            _moveProvider.GetWhiteQueenAttacks(_squaresCheck[WhiteQueen], _attacksCheck);
            return AnyLigalWhiteCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyWhiteRookCapture()
        {
            _moveProvider.GetWhiteRookAttacks(_squaresCheck[WhiteRook], _attacksCheck);
            return AnyLigalWhiteCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyWhiteBishopCapture()
        {
            _moveProvider.GetWhiteBishopAttacks(_squaresCheck[WhiteBishop], _attacksCheck);
            return AnyLigalWhiteCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyWhiteKnightCapture()
        {
            _moveProvider.GetWhiteKnightAttacks(_squaresCheck[WhiteKnight], _attacksCheck);
            return AnyLigalWhiteCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyWhitePawnCapture()
        {
            _moveProvider.GetWhitePawnAttacks(_squaresCheck[WhitePawn], _attacksCheck);
            return AnyLigalWhiteCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyBlackKingCapture()
        {
            _moveProvider.GetBlackKingAttacks(_squaresCheck[WhiteKing], _attacksCheck);
            return AnyLigalBlackCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyBlackQueenCapture()
        {
            _moveProvider.GetBlackQueenAttacks(_squaresCheck[WhiteQueen], _attacksCheck);
            return AnyLigalBlackCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyBlackRookCapture()
        {
            _moveProvider.GetBlackRookAttacks(_squaresCheck[WhiteRook], _attacksCheck);
            return AnyLigalBlackCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyBlackBishopCapture()
        {
            _moveProvider.GetBlackBishopAttacks(_squaresCheck[WhiteBishop], _attacksCheck);
            return AnyLigalBlackCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyBlackKnightCapture()
        {
            _moveProvider.GetBlackKnightAttacks(_squaresCheck[WhiteKnight], _attacksCheck);
            return AnyLigalBlackCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyBlackPawnCapture()
        {
            _moveProvider.GetBlackPawnAttacks(_squaresCheck[WhitePawn], _attacksCheck);
            return AnyLigalBlackCapture();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyWhiteMove()
        {
            _movesCheck.Clear();

            return AnyWhiteKingMove() || AnyWhitePawnMove() || AnyWhiteKnightMove() || AnyWhiteBishopMove() ||
                AnyWhiteRookMove() || AnyWhiteQueenMove();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyWhiteKingMove()
        {
            _moveProvider.GetWhiteKingMoves(_squaresCheck[WhiteKing], _movesCheck);
            return AnyLigalWhiteMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyWhiteQueenMove()
        {
            _moveProvider.GetWhiteQueenMoves(_squaresCheck[WhiteQueen], _movesCheck);
            return AnyLigalWhiteMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyWhiteRookMove()
        {
            _moveProvider.GetWhiteRookMoves(_squaresCheck[WhiteRook], _movesCheck);
            return AnyLigalWhiteMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyWhiteBishopMove()
        {
            _moveProvider.GetWhiteBishopMoves(_squaresCheck[WhiteBishop], _movesCheck);
            return AnyLigalWhiteMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyWhiteKnightMove()
        {
            _moveProvider.GetWhiteKnightMoves(_squaresCheck[WhiteKnight], _movesCheck);
            return AnyLigalWhiteMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyWhitePawnMove()
        {
            _moveProvider.GetWhitePawnMoves(_squaresCheck[WhitePawn], _movesCheck);
            return AnyLigalWhiteMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyBlackMove()
        {
            _movesCheck.Clear();

            return AnyBlackKingMove() || AnyBlackPawnMove() || AnyBlackKnightMove() || AnyBlackBishopMove() ||
                AnyBlackRookMove() || AnyBlackQueenMove();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyBlackKingMove()
        {
            _moveProvider.GetBlackKingMoves(_squaresCheck[WhiteKing], _movesCheck);
            return AnyLigalBlackMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyBlackQueenMove()
        {
            _moveProvider.GetBlackQueenMoves(_squaresCheck[WhiteQueen], _movesCheck);
            return AnyLigalBlackMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyBlackRookMove()
        {
            _moveProvider.GetBlackRookMoves(_squaresCheck[WhiteRook], _movesCheck);
            return AnyLigalBlackMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyBlackBishopMove()
        {
            _moveProvider.GetBlackBishopMoves(_squaresCheck[WhiteBishop], _movesCheck);
            return AnyLigalBlackMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyBlackKnightMove()
        {
            _moveProvider.GetBlackKnightMoves(_squaresCheck[WhiteKnight], _movesCheck);
            return AnyLigalBlackMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyBlackPawnMove()
        {
            _moveProvider.GetBlackPawnMoves(_squaresCheck[WhitePawn], _movesCheck);
            return AnyLigalBlackMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyWhitePromotion()
        {
            if (_board.CanWhitePromote())
            {
                _board.GetWhitePromotionSquares(_promotionSquaresCheck);

                for (byte f = 0; f < _promotionSquaresCheck.Length; f++)
                {
                    var promotions = _moveProvider.GetWhitePromotionAttacks(_promotionSquaresCheck[f]);

                    for (byte i = 0; i < promotions.Length; i++)
                    {
                        if (promotions[i].Count != 0 && IsWhiteLigal(promotions[i][0]))
                            return true;
                    }

                    var p = _moveProvider.GetWhitePromotions(_promotionSquaresCheck[f]);

                    if (p.Count > 0 && IsWhiteLigal(p[0]))
                        return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AnyBlackMoves()
        {
            GetBlackSquares(_black[_phase], _squaresCheck);

            return AnyBlackMove() || AnyBlackCapture() || AnyBlackPromotion();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AnyBlackPromotion()
        {
            if (_board.CanBlackPromote())
            {
                _board.GetBlackPromotionSquares(_promotionSquaresCheck);

                for (byte f = 0; f < _promotionSquaresCheck.Length; f++)
                {
                    var promotions = _moveProvider.GetBlackPromotionAttacks(_promotionSquaresCheck[f]);

                    for (byte i = 0; i < promotions.Length; i++)
                    {
                        if (promotions[i].Count != 0 && IsBlackLigal(promotions[i][0]))
                            return true;
                    }

                    var p = _moveProvider.GetBlackPromotions(_promotionSquaresCheck[f]);

                    if (p.Count > 0 && IsBlackLigal(p[0]))
                        return true;
                }
            }
            return false;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsWhiteLigal(MoveBase move)
        {
            Do(move);

            bool isLegal = !IsWhiteNotLegal(move);

            UnDo(move);

            return isLegal;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsBlackLigal(MoveBase move)
        {
            Do(move);

            bool isLegal = !IsBlackNotLegal(move);

            UnDo(move);

            return isLegal;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SwapTurn()
        {
            _turn = _turn == Turn.White ? Turn.Black : Turn.White;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Turn = {_turn}, Key = {GetKey()}, Value = {GetValue()}, Static = {GetStaticValue()}");
            builder.AppendLine(_board.ToString());
            return builder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDraw()
        {
            return _board.IsDraw();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlockedByBlack(byte position)
        {
            return _board.IsBlockedByBlack(position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlockedByWhite(byte position)
        {
            return _board.IsBlockedByWhite(position);
        }
    }
}