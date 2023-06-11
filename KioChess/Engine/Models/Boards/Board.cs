using System.Runtime.CompilerServices;
using System.Text;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.DataStructures.Hash;
using Engine.Interfaces;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Newtonsoft.Json.Linq;

namespace Engine.Models.Boards
{
    public class Board : IBoard
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

        private byte _phase = Phase.Opening;

        private BitBoard _empty;
        private BitBoard _whites;
        private BitBoard _blacks;

        private BitBoard _whiteSmallCastleCondition;
        private BitBoard _whiteSmallCastleKing;
        private BitBoard _whiteSmallCastleRook;

        private BitBoard _whiteBigCastleCondition;
        private BitBoard _whiteBigCastleKing;
        private BitBoard _whiteBigCastleRook;

        private BitBoard _blackSmallCastleCondition;
        private BitBoard _blackSmallCastleKing;
        private BitBoard _blackSmallCastleRook;

        private BitBoard _blackBigCastleCondition;
        private BitBoard _blackBigCastleKing;
        private BitBoard _blackBigCastleRook;

        private readonly ZobristHash _hash;
        private BitBoard[] _notRanks;
        private BitBoard[] _ranks;
        private BitBoard[] _files;
        private BitBoard[] _boards;
        private BitBoard[] _whiteKingShield;
        private BitBoard[] _blackKingShield;
        private BitBoard[] _whiteKingFaceShield;
        private BitBoard[] _blackKingFaceShield;
        private BitBoard[] _whiteKingFace;
        private BitBoard[] _blackKingFace;
        private BitBoard[][] _whiteKingOpenFile;
        private BitBoard[][] _blackKingOpenFile;
        private BitBoard[] _rookFiles;
        private BitBoard[] _rookRanks;

        private BitBoard[] _whiteMinorDefense;
        private BitBoard[] _blackMinorDefense;
        private BitBoard[] _whiteFacing;
        private BitBoard[] _blackFacing;

        private BitBoard[] _whiteBlockedPawns;
        private BitBoard[] _whiteDoublePawns;
        private BitBoard[] _whitePassedPawns;
        private BitBoard[] _whiteIsolatedPawns;
        private List<KeyValuePair<BitBoard, BitBoard>>[] _whiteBackwardPawns;

        private BitBoard[] _blackBlockedPawns;
        private BitBoard[] _blackDoublePawns;
        private BitBoard[] _blackPassedPawns;
        private BitBoard[] _blackIsolatedPawns;
        private List<KeyValuePair<BitBoard, BitBoard>>[] _blackBackwardPawns;

        private readonly byte[] _pieces;
        private readonly BitBoard _whiteQueenOpening;
        private readonly BitBoard _blackQueenOpening;
        private BitBoard _notFileA;
        private BitBoard _notFileH;
        private BitBoard[] _whiteRookKingPattern;
        private BitBoard[] _whiteRookPawnPattern;
        private BitBoard[] _blackRookKingPattern;
        private BitBoard[] _blackRookPawnPattern;

        private PositionsList _positionList;
        private readonly IMoveProvider _moveProvider;
        private readonly IMoveHistoryService _moveHistory;
        private readonly IEvaluationService[] _evaluationServices;
        private IEvaluationService _evaluationService;
        private readonly IAttackEvaluationService _attackEvaluationService;

        public Board()
        {
            _pieces = new byte[64];
            _positionList = new PositionsList();

            MoveBase.Board = this;

            SetBoards();

            SetFilesAndRanks();

            SetCastles();

            _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
            _moveHistory = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
            _evaluationServices = ServiceLocator.Current.GetInstance<IEvaluationServiceFactory>().GetEvaluationServices();
            _attackEvaluationService = ServiceLocator.Current.GetInstance<IAttackEvaluationService>();
            _attackEvaluationService.SetBoard(this);

            _hash = new ZobristHash();
            _hash.Initialize(_boards);

            _moveProvider.SetBoard(this);
            

            _whiteQueenOpening = D1.AsBitBoard() | E1.AsBitBoard() | C1.AsBitBoard() |
                                 D2.AsBitBoard() | E2.AsBitBoard() | C2.AsBitBoard();

            _blackQueenOpening = D8.AsBitBoard() | E8.AsBitBoard() | C8.AsBitBoard() |
                                 D7.AsBitBoard() | E7.AsBitBoard() | C7.AsBitBoard();

            SetKingSafety();

            SetPawnProperties();

            SetKingRookPatterns();
        }

        #region Implementation of IBoard

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty(BitBoard bitBoard)
        {
            return _empty.IsSet(bitBoard);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsWhiteOpposite(byte square)
        {
            return _blacks.IsSet(square);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlackOpposite(byte square)
        {
            return _whites.IsSet(square);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlockedByBlack(byte square)
        {
            return _blacks.IsSet(square);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlockedByWhite(byte square)
        {
            return _whites.IsSet(square);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetPiece(byte cell)
        {
            return _pieces[cell];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetPiece(byte cell, out byte? piece)
        {
            piece = null;

            foreach (var p in Enumerable.Range(0, 12))
            {
                if (!_boards[p].IsSet(cell)) continue;

                piece = (byte)p;
                break;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(byte piece, byte square)
        {
            _hash.Update(square, piece);

            Remove(piece, square.AsBitBoard());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(byte piece, byte square)
        {
            _hash.Update(square, piece);
            _pieces[square] = piece;

            Add(piece, square.AsBitBoard());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Move(byte piece, byte from, byte to)
        {
            _hash.Update(from, to, piece);
            _pieces[to] = piece;

            Move(piece, from.AsBitBoard() | to.AsBitBoard());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetWhiteKingPosition()
        {
            return _boards[WhiteKing].BitScanForward();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetBlackKingPosition()
        {
            return _boards[BlackKing].BitScanForward();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PositionsList GetPiecePositions(byte index)
        {
            _boards[index].GetPositions(_positionList);
            return _positionList;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetSquares(byte index, SquareList squares)
        {
            _boards[index].GetPositions(squares);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhitePawnSquares(SquareList squares)
        {
            (_notRanks[6] & _boards[WhitePawn]).GetPositions(squares);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackPawnSquares(SquareList squares)
        {
            (_notRanks[1] & _boards[BlackPawn]).GetPositions(squares);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetWhitePromotionSquares(SquareList squares)
        {
            (_ranks[6] & _boards[WhitePawn]).GetPositions(squares);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetBlackPromotionSquares(SquareList squares)
        {
            (_ranks[1] & _boards[BlackPawn]).GetPositions(squares);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetKey()
        {
            return _hash.Key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetOccupied()
        {
            return ~_empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetEmpty()
        {
            return _empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetBlacks()
        {
            return _blacks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetWhites()
        {
            return _whites;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetPieceBits(byte piece)
        {
            return _boards[piece];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetPerimeter()
        {
            return _ranks[0] | _ranks[7] | _files[0] | _files[7];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetWhitePawnAttacks()
        {
            return ((_boards[WhitePawn] & _notFileA) << 7) |
                   ((_boards[WhitePawn] & _notFileH) << 9);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetBlackPawnAttacks()
        {
            return ((_boards[BlackPawn] & _notFileA) >> 9) |
                   ((_boards[BlackPawn] & _notFileH) >> 7);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte UpdatePhase()
        {
            var ply = _moveHistory.GetPly();
            _phase = ply < 16 ? Phase.Opening : ply > 39 && IsEndGame() ? Phase.End : Phase.Middle;
            return _phase;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsEndGame()
        {
            return IsEndGameForWhite() && IsEndGameForBlack();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsEndGameForBlack()
        {
            return _blacks.Remove(_boards[BlackPawn]).Count() < 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsEndGameForWhite()
        {
            return _whites.Remove(_boards[WhitePawn]).Count() < 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanWhitePromote()
        {
            return (_ranks[6] & _boards[WhitePawn]).Any();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanBlackPromote()
        {
            return (_ranks[1] & _boards[BlackPawn]).Any();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitBoard GetRank(int rank)
        {
            return _ranks[rank];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetPhase()
        {
            return _phase;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlackPass(byte position)
        {
            return (_blackPassedPawns[position] & _boards[WhitePawn]).IsZero();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsWhitePass(byte position)
        {
            return (_whitePassedPawns[position] & _boards[BlackPawn]).IsZero();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsWhiteOver(BitBoard opponentPawns)
        {
            return (_boards[WhitePawn] & opponentPawns).Any();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlackOver(BitBoard opponentPawns)
        {
            return (_boards[BlackPawn] & opponentPawns).Any();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDraw()
        {
            if ((_boards[WhitePawn] |
                _boards[WhiteRook] |
                _boards[WhiteQueen] |
                _boards[BlackPawn] |
                _boards[BlackRook] |
                _boards[BlackQueen]).Any()) return false;

            return (_boards[WhiteKnight] | _boards[WhiteBishop]).Count() < 2 && (_boards[BlackKnight] | _boards[BlackBishop]).Count() < 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlackAttacksTo(byte to)
        {
            return (_moveProvider.GetAttackPattern(WhiteKnight, to) & _boards[BlackKnight]).Any()
                || (to.BishopAttacks(~_empty) & (_boards[BlackBishop] | _boards[BlackQueen])).Any()
                || (to.RookAttacks(~_empty) & (_boards[BlackRook] | _boards[BlackQueen])).Any()
                || (_moveProvider.GetAttackPattern(WhitePawn, to) & _boards[BlackPawn]).Any()
                || (_moveProvider.GetAttackPattern(WhiteKing, to) & _boards[BlackKing]).Any();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsWhiteAttacksTo(byte to)
        {
            return (_moveProvider.GetAttackPattern(BlackKnight, to) & _boards[WhiteKnight]).Any()
            || (to.BishopAttacks(~_empty) & (_boards[WhiteBishop] | _boards[WhiteQueen])).Any()
            || (to.RookAttacks(~_empty) & (_boards[WhiteRook] | _boards[WhiteQueen])).Any()
            || (_moveProvider.GetAttackPattern(BlackPawn, to) & _boards[WhitePawn]).Any()
            || (_moveProvider.GetAttackPattern(BlackKing, to) & _boards[WhiteKing]).Any();
        }

        #endregion

        #region SEE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short StaticExchange(AttackBase attack)
        {
            //var timer = Stopwatch.StartNew();
            //try
            //{
            _attackEvaluationService.Initialize(_boards);
            return _attackEvaluationService.StaticExchange(attack);
            //}
            //finally
            //{
            //    timer.Stop();
            //    MoveGenerationPerformance.Add(nameof(StaticExchange), timer.Elapsed);
            //}
        }

        #endregion

        #region Castle

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoWhiteSmallCastle()
        {
            _pieces[G1] = WhiteKing;
            _pieces[F1] = WhiteRook;

            _hash.Update(H1, F1, WhiteRook);
            _hash.Update(E1, G1, WhiteKing);

            WhiteSmallCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoBlackSmallCastle()
        {
            _pieces[G8] = BlackKing;
            _pieces[F8] = BlackRook;

            _hash.Update(H8, F8, BlackRook);
            _hash.Update(E8, G8, BlackKing);

            BlackSmallCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoBlackBigCastle()
        {
            _pieces[C8] = BlackKing;
            _pieces[D8] = BlackRook;

            _hash.Update(A8, D8, BlackRook);
            _hash.Update(E8, C8, BlackKing);

            BlackBigCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DoWhiteBigCastle()
        {
            _pieces[C1] = WhiteKing;
            _pieces[D1] = WhiteRook;

            _hash.Update(A1, D1, WhiteRook);
            _hash.Update(E1, C1, WhiteKing);

            WhiteBigCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoWhiteSmallCastle()
        {
            _pieces[E1] = WhiteKing;
            _pieces[H1] = WhiteRook;

            _hash.Update(F1, H1, WhiteRook);
            _hash.Update(G1, E1, WhiteKing);

            WhiteSmallCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoBlackSmallCastle()
        {
            _pieces[E8] = BlackKing;
            _pieces[H8] = BlackRook;

            _hash.Update(F8, H8, BlackRook);
            _hash.Update(G8, E8, BlackKing);

            BlackSmallCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoWhiteBigCastle()
        {
            _pieces[E1] = WhiteKing;
            _pieces[A1] = WhiteRook;

            _hash.Update(D1, A1, WhiteRook);
            _hash.Update(C1, E1, WhiteKing);

            WhiteBigCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UndoBlackBigCastle()
        {
            _pieces[E8] = BlackKing;
            _pieces[A8] = BlackRook;

            _hash.Update(D8, A8, BlackRook);
            _hash.Update(C8, E8, BlackKing);

            BlackBigCastle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BlackSmallCastle()
        {
            _boards[BlackKing] ^= _blackSmallCastleKing;
            _boards[BlackRook] ^= _blackSmallCastleRook;

            _blacks ^= _blackSmallCastleKing;
            _blacks ^= _blackSmallCastleRook;

            _empty = ~(_whites | _blacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BlackBigCastle()
        {
            _boards[BlackKing] ^= _blackBigCastleKing;
            _boards[BlackRook] ^= _blackBigCastleRook;

            _blacks ^= _blackBigCastleKing;
            _blacks ^= _blackBigCastleRook;

            _empty = ~(_whites | _blacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WhiteBigCastle()
        {
            _boards[WhiteKing] ^= _whiteBigCastleKing;
            _boards[WhiteRook] ^= _whiteBigCastleRook;

            _whites ^= _whiteBigCastleKing;
            _whites ^= _whiteBigCastleRook;

            _empty = ~(_whites | _blacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WhiteSmallCastle()
        {
            _boards[WhiteKing] ^= _whiteSmallCastleKing;
            _boards[WhiteRook] ^= _whiteSmallCastleRook;

            _whites ^= _whiteSmallCastleKing;
            _whites ^= _whiteSmallCastleRook;

            _empty = ~(_whites | _blacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackSmallCastle()
        {
            if (!_moveHistory.CanDoBlackSmallCastle() || !_boards[BlackRook].IsSet(BitBoards.H8) ||
                !_empty.IsSet(_blackSmallCastleCondition)) return false;

            return CanDoBlackCastle(E8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteSmallCastle()
        {
            if (!_moveHistory.CanDoWhiteSmallCastle() || !_boards[WhiteRook].IsSet(BitBoards.H1) ||
                !_empty.IsSet(_whiteSmallCastleCondition)) return false;

            return CanDoWhiteCastle(E1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackBigCastle()
        {
            if (!_moveHistory.CanDoBlackBigCastle() || !_boards[BlackRook].IsSet(BitBoards.A8) ||
                !_empty.IsSet(_blackBigCastleCondition)) return false;

            return CanDoBlackCastle(E8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteBigCastle()
        {
            if (!_moveHistory.CanDoWhiteBigCastle() || !_boards[WhiteRook].IsSet(BitBoards.A1) ||
                !_empty.IsSet(_whiteBigCastleCondition)) return false;

            return CanDoWhiteCastle(E1);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteCastle(byte to)
        {
            return ((_moveProvider.GetAttackPattern(WhiteKnight, to) & _boards[BlackKnight])
                    | (to.BishopAttacks(~_empty) & (_boards[BlackBishop] | _boards[BlackQueen]))
                    | (to.RookAttacks(~_empty) & (_boards[BlackRook] | _boards[BlackQueen]))
                    | (_moveProvider.GetAttackPattern(WhitePawn, to) & _boards[BlackPawn])).IsZero();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackCastle(byte to)
        {
            return ((_moveProvider.GetAttackPattern(BlackKnight, to) & _boards[WhiteKnight])
                    | (to.BishopAttacks(~_empty) & (_boards[WhiteBishop] | _boards[WhiteQueen]))
                    | (to.RookAttacks(~_empty) & (_boards[WhiteRook] | _boards[WhiteQueen]))
                    | (_moveProvider.GetAttackPattern(BlackPawn, to) & _boards[WhitePawn])).IsZero();
        }

        #endregion

        #region Private

        private void SetKingRookPatterns()
        {
            _whiteRookKingPattern = new BitBoard[64];
            _whiteRookPawnPattern = new BitBoard[64];
            _blackRookKingPattern = new BitBoard[64];
            _blackRookPawnPattern = new BitBoard[64];
            for (int i = 0; i < 64; i++)
            {
                _whiteRookKingPattern[i] = new BitBoard();
                _whiteRookPawnPattern[i] = new BitBoard();
                _blackRookKingPattern[i] = new BitBoard();
                _blackRookPawnPattern[i] = new BitBoard();
            }

            SetWhiteRookKingPattern();

            SetWhiteRookPawnPattern();

            SetBlackRookKingPattern();

            SetBlackRookPawnPattern();
        }

        private void SetBlackRookPawnPattern()
        {
            var bitBoard = _blackRookPawnPattern[A8];
            bitBoard = bitBoard | A7.AsBitBoard() | A6.AsBitBoard();
            _blackRookPawnPattern[A8] = bitBoard;

            bitBoard = _blackRookPawnPattern[B8];
            bitBoard = bitBoard | B7.AsBitBoard() | B6.AsBitBoard();
            _blackRookPawnPattern[B8] = bitBoard;

            bitBoard = _blackRookPawnPattern[C8];
            bitBoard = bitBoard | C7.AsBitBoard() | C6.AsBitBoard();
            _blackRookPawnPattern[C8] = bitBoard;

            bitBoard = _blackRookPawnPattern[H8];
            bitBoard = bitBoard | H7.AsBitBoard() | H6.AsBitBoard();
            _blackRookPawnPattern[H8] = bitBoard;

            bitBoard = _blackRookPawnPattern[G8];
            bitBoard = bitBoard | G7.AsBitBoard() | G6.AsBitBoard();
            _blackRookPawnPattern[G8] = bitBoard;
        }

        private void SetWhiteRookPawnPattern()
        {
            var bitBoard = _whiteRookPawnPattern[A1];
            bitBoard = bitBoard | A2.AsBitBoard() | A3.AsBitBoard();
            _whiteRookPawnPattern[A1] = bitBoard;

            bitBoard = _whiteRookPawnPattern[B1];
            bitBoard = bitBoard | B2.AsBitBoard() | B3.AsBitBoard();
            _whiteRookPawnPattern[B1] = bitBoard;

            bitBoard = _whiteRookPawnPattern[C1];
            bitBoard = bitBoard | C2.AsBitBoard() | C3.AsBitBoard();
            _whiteRookPawnPattern[C1] = bitBoard;

            bitBoard = _whiteRookPawnPattern[H1];
            bitBoard = bitBoard | H2.AsBitBoard() | H3.AsBitBoard();
            _whiteRookPawnPattern[H1] = bitBoard;

            bitBoard = _whiteRookPawnPattern[G1];
            bitBoard = bitBoard | G2.AsBitBoard() | G3.AsBitBoard();
            _whiteRookPawnPattern[G1] = bitBoard;
        }

        private void SetBlackRookKingPattern()
        {
            var bitBoard = _blackRookKingPattern[A8];
            bitBoard = bitBoard | B8.AsBitBoard() | C8.AsBitBoard() | D8.AsBitBoard();
            _blackRookKingPattern[A8] = bitBoard;

            bitBoard = _blackRookKingPattern[B8];
            bitBoard = bitBoard | C8.AsBitBoard() | D8.AsBitBoard();
            _blackRookKingPattern[B8] = bitBoard;

            bitBoard = _blackRookKingPattern[C8];
            bitBoard = bitBoard | D8.AsBitBoard();
            _blackRookKingPattern[C8] = bitBoard;

            bitBoard = _blackRookKingPattern[H8];
            bitBoard = bitBoard | G8.AsBitBoard() | F8.AsBitBoard();
            _blackRookKingPattern[H8] = bitBoard;

            bitBoard = _blackRookKingPattern[G8];
            bitBoard = bitBoard | F8.AsBitBoard();
            _blackRookKingPattern[G8] = bitBoard;
        }

        private void SetWhiteRookKingPattern()
        {
            var bitBoard = _whiteRookKingPattern[A1];
            bitBoard = bitBoard | B1.AsBitBoard() | C1.AsBitBoard() | D1.AsBitBoard();
            _whiteRookKingPattern[A1] = bitBoard;

            bitBoard = _whiteRookKingPattern[B1];
            bitBoard = bitBoard | C1.AsBitBoard() | D1.AsBitBoard();
            _whiteRookKingPattern[B1] = bitBoard;

            bitBoard = _whiteRookKingPattern[C1];
            bitBoard = bitBoard | D1.AsBitBoard();
            _whiteRookKingPattern[C1] = bitBoard;

            bitBoard = _whiteRookKingPattern[H1];
            bitBoard = bitBoard | G1.AsBitBoard() | F1.AsBitBoard();
            _whiteRookKingPattern[H1] = bitBoard;

            bitBoard = _whiteRookKingPattern[G1];
            bitBoard = bitBoard | F1.AsBitBoard();
            _whiteRookKingPattern[G1] = bitBoard;
        }

        private void SetPawnProperties()
        {
            _whiteBlockedPawns = new BitBoard[64];
            _whiteDoublePawns = new BitBoard[64];
            _whitePassedPawns = new BitBoard[64];
            _whiteIsolatedPawns = new BitBoard[64];
            _whiteBackwardPawns = new List<KeyValuePair<BitBoard, BitBoard>>[64];

            _blackBlockedPawns = new BitBoard[64];
            _blackDoublePawns = new BitBoard[64];
            _blackPassedPawns = new BitBoard[64];
            _blackIsolatedPawns = new BitBoard[64];
            _blackBackwardPawns = new List<KeyValuePair<BitBoard, BitBoard>>[64];

            _whiteMinorDefense = new BitBoard[64];
            _blackMinorDefense = new BitBoard[64];

            _whiteFacing = new BitBoard[64];
            _blackFacing = new BitBoard[64];

            for (byte i = 8; i < 48; i++)
            {
                BitBoard b = new BitBoard();
                for (byte j = (byte)(i + 8); j < 56; j += 8)
                {
                    b |= j.AsBitBoard();
                }
                _whiteFacing[i] = b;
            }
            for (byte i = 16; i < 56; i++)
            {
                BitBoard b = new BitBoard();
                for (byte j = (byte)(i - 8); j >= 8; j -= 8)
                {
                    b |= j.AsBitBoard();
                }
                _blackFacing[i] = b;
            }

            for (byte i = 16; i < 64; i++)
            {
                _whiteMinorDefense[i] = _moveProvider.GetAttackPattern(BlackPawn, i);
            }
            for (byte i = 0; i < 48; i++)
            {
                _blackMinorDefense[i] = _moveProvider.GetAttackPattern(WhitePawn, i);
            }

            BitBoard ones = new BitBoard();
            ones = ~ones;

            for (byte i = 8; i < 56; i++)
            {
                var f = i % 8;
                var r = i / 8;

                _whiteBlockedPawns[i] = ((byte)(i + 8)).AsBitBoard();
                _blackBlockedPawns[i] = ((byte)(i - 8)).AsBitBoard();

                _whiteDoublePawns[i] = _files[f] ^ i.AsBitBoard();
                _blackDoublePawns[i] = _files[f] ^ i.AsBitBoard();

                if (f == 0)
                {
                    _whitePassedPawns[i] = (ones << i) & (_files[0] | _files[1]) & ~_ranks[r];
                    _blackPassedPawns[i] = ~(ones << i) & (_files[0] | _files[1]) & ~_ranks[r];

                    _whiteIsolatedPawns[i] = _files[1];
                    _blackIsolatedPawns[i] = _files[1];
                }
                else if (f == 7)
                {
                    _whitePassedPawns[i] = (ones << i) & (_files[6] | _files[7]) & ~_ranks[r];
                    _blackPassedPawns[i] = ~(ones << i) & (_files[6] | _files[7]) & ~_ranks[r];

                    _whiteIsolatedPawns[i] = _files[7];
                    _blackIsolatedPawns[i] = _files[7];
                }
                else
                {
                    _whitePassedPawns[i] = (ones << i) & (_files[f - 1] | _files[f] | _files[f + 1]) & ~_ranks[r];
                    _blackPassedPawns[i] = ~(ones << i) & (_files[f - 1] | _files[f] | _files[f + 1]) & ~_ranks[r];

                    _whiteIsolatedPawns[i] = _files[f - 1] | _files[f + 1];
                    _blackIsolatedPawns[i] = _files[f - 1] | _files[f + 1];
                }
            }

            for (int i = 0; i < 64; i++)
            {
                _whiteBackwardPawns[i] = new List<KeyValuePair<BitBoard, BitBoard>>();
                _blackBackwardPawns[i] = new List<KeyValuePair<BitBoard, BitBoard>>();
            }

            for (byte i = 8; i < 16; i++)
            {
                byte w = (byte)(i + 8);
                var bb = _moveProvider.GetAttackPattern(WhitePawn, w);
                var wb = _blackPassedPawns[w];
                for (int j = i; j >= 0; j -= 8)
                {
                    wb ^= j.AsBitBoard();
                }

                _whiteBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));

                w = (byte)(i + 16);
                bb = _moveProvider.GetAttackPattern(WhitePawn, w);
                wb = _blackPassedPawns[w];
                for (int j = i; j >= 0; j -= 8)
                {
                    wb ^= j.AsBitBoard();
                }
                _whiteBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));
            }
            for (byte i = 16; i < 48; i++)
            {
                byte w = (byte)(i + 8);
                var bb = _moveProvider.GetAttackPattern(WhitePawn, w);
                var wb = _blackPassedPawns[w];
                for (int j = i; j >= 0; j -= 8)
                {
                    wb ^= j.AsBitBoard();
                }

                _whiteBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));
            }
            for (byte i = 48; i < 56; i++)
            {
                byte w = (byte)(i - 8);
                var bb = _moveProvider.GetAttackPattern(BlackPawn, w);
                var wb = _whitePassedPawns[w];
                for (int j = i; j < 56; j += 8)
                {
                    wb ^= j.AsBitBoard();
                }

                _blackBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));

                w = (byte)(i - 16);
                bb = _moveProvider.GetAttackPattern(BlackPawn, w);
                wb = _whitePassedPawns[w];
                for (int j = i; j < 56; j += 8)
                {
                    wb ^= j.AsBitBoard();
                }

                _blackBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));
            }
            for (byte i = 16; i < 48; i++)
            {
                byte w = (byte)(i - 8);
                var bb = _moveProvider.GetAttackPattern(BlackPawn, w);
                var wb = _whitePassedPawns[w];
                for (int j = i; j < 56; j += 8)
                {
                    wb ^= j.AsBitBoard();
                }

                _blackBackwardPawns[i].Add(new KeyValuePair<BitBoard, BitBoard>(wb, bb));
            }
        }

        private void SetKingSafety()
        {
            _whiteKingShield = new BitBoard[64];
            for (byte i = 0; i < 16; i++)
            {
                _whiteKingShield[i] = _moveProvider.GetAttackPattern(WhiteKing, i) |
                                      _moveProvider.GetAttackPattern(WhiteKing, (byte)(i + 8));
            }

            for (byte i = 16; i < 64; i++)
            {
                _whiteKingShield[i] = _moveProvider.GetAttackPattern(WhiteKing, i);
            }

            _blackKingShield = new BitBoard[64];

            for (byte i = (byte)(_blackKingShield.Length - 1); i >= 48; i--)
            {
                _blackKingShield[i] = _moveProvider.GetAttackPattern(BlackKing, i) |
                                      _moveProvider.GetAttackPattern(BlackKing, (byte)(i - 8));
            }

            for (byte i = 0; i < 48; i++)
            {
                _blackKingShield[i] = _moveProvider.GetAttackPattern(BlackKing, i);
            }

            _whiteKingFace = new BitBoard[64];
            for (byte i = 0; i < 32; i++)
            {
                _whiteKingFace[i] = _moveProvider.GetAttackPattern(WhiteKing, i) &
                                    _ranks[i / 8 + 1];
            }

            _blackKingFace = new BitBoard[64];
            for (byte i = 32; i < 64; i++)
            {
                _blackKingFace[i] = _moveProvider.GetAttackPattern(BlackKing, i) &
                                    _ranks[i / 8 - 1];
            }

            _whiteKingFaceShield = new BitBoard[64];
            for (byte i = 0; i < 32; i++)
            {
                _whiteKingFaceShield[i] = _moveProvider.GetAttackPattern(WhiteKing, (byte)(i + 8)) &
                                          _ranks[i / 8 + 2];
            }

            _blackKingFaceShield = new BitBoard[64];
            for (byte i = 32; i < 64; i++)
            {
                _blackKingFaceShield[i] = _moveProvider.GetAttackPattern(BlackKing, (byte)(i - 8)) &
                                          _ranks[i / 8 - 2];
            }

            _whiteKingOpenFile = new BitBoard[64][];
            for (byte i = 0; i < _whiteKingOpenFile.Length; i++)
            {
                var rank = i % 8;
                if (rank == 0)
                {
                    _whiteKingOpenFile[i] = new[] { _files[0] ^ i.AsBitBoard(), _files[1] };
                }
                else if (rank == 7)
                {
                    _whiteKingOpenFile[i] = new[] { _files[7] ^ i.AsBitBoard(), _files[6] };
                }
                else
                {
                    _whiteKingOpenFile[i] = new[]
                        {_files[rank - 1], _files[rank] ^ i.AsBitBoard(), _files[rank + 1]};
                }
            }

            _blackKingOpenFile = new BitBoard[64][];
            for (byte i = 0; i < _blackKingOpenFile.Length; i++)
            {
                var rank = i % 8;
                if (rank == 0)
                {
                    _blackKingOpenFile[i] = new[] { _files[0] ^ i.AsBitBoard(), _files[1] };
                }
                else if (rank == 7)
                {
                    _blackKingOpenFile[i] = new[] { _files[7] ^ i.AsBitBoard(), _files[6] };
                }
                else
                {
                    _blackKingOpenFile[i] = new[]
                        {_files[rank - 1], _files[rank] ^ i.AsBitBoard(), _files[rank + 1]};
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Remove(byte piece, BitBoard bitBoard)
        {
            var bit = ~bitBoard;
            _boards[piece] &= bit;
            if (piece.IsWhite())
            {
                _whites &= bit;
            }
            else
            {
                _blacks &= bit;
            }

            _empty = ~(_whites | _blacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add(byte piece, BitBoard bitBoard)
        {
            _boards[piece] |= bitBoard;
            if (piece.IsWhite())
            {
                _whites |= bitBoard;
            }
            else
            {
                _blacks |= bitBoard;
            }

            _empty = ~(_whites | _blacks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Move(byte piece, BitBoard bitBoard)
        {
            _boards[piece] ^= bitBoard;
            if (piece.IsWhite())
            {
                _whites ^= bitBoard;
            }
            else
            {
                _blacks ^= bitBoard;
            }

            _empty = ~(_whites | _blacks);
        }

        private void SetBoards()
        {
            _boards = new BitBoard[12];
            _boards[WhitePawn] = _boards[WhitePawn].Set(Enumerable.Range(8, 8).ToArray());
            _boards[WhiteKnight] = _boards[WhiteKnight].Set(1, 6);
            _boards[WhiteBishop] = _boards[WhiteBishop].Set(2, 5);
            _boards[WhiteRook] = _boards[WhiteRook].Set(0, 7);
            _boards[WhiteQueen] = _boards[WhiteQueen].Set(3);
            _boards[WhiteKing] = _boards[WhiteKing].Set(4);

            _whites = _boards[WhitePawn] |
                      _boards[WhiteKnight] |
                      _boards[WhiteBishop] |
                      _boards[WhiteRook] |
                      _boards[WhiteQueen] |
                      _boards[WhiteKing];

            _boards[BlackPawn] =
                _boards[BlackPawn].Set(Enumerable.Range(48, 8).ToArray());
            _boards[BlackRook] = _boards[BlackRook].Set(56, 63);
            _boards[BlackKnight] = _boards[BlackKnight].Set(57, 62);
            _boards[BlackBishop] = _boards[BlackBishop].Set(58, 61);
            _boards[BlackQueen] = _boards[BlackQueen].Set(59);
            _boards[BlackKing] = _boards[BlackKing].Set(60);

            _blacks = _boards[BlackPawn] |
                      _boards[BlackRook] |
                      _boards[BlackKnight] |
                      _boards[BlackBishop] |
                      _boards[BlackQueen] |
                      _boards[BlackKing];

            _empty = ~(_whites | _blacks);

            foreach (var piece in Enumerable.Range(0, 12))
            {
                foreach (var b in _boards[piece].BitScan())
                {
                    _pieces[b] = (byte)piece;
                }
            }
        }

        private void SetFilesAndRanks()
        {
            BitBoard rank = new BitBoard(0);
            rank = rank.Set(Enumerable.Range(0, 8).ToArray());
            _ranks = new BitBoard[8];
            _notRanks = new BitBoard[8];
            for (var i = 0; i < _ranks.Length; i++)
            {
                _ranks[i] = rank;
                rank = rank << 8;
            }

            _files = new BitBoard[8];
            BitBoard file = new BitBoard(0);
            for (int i = 0; i < 60; i += 8)
            {
                file = file.Set(i);
            }

            for (var i = 0; i < _files.Length; i++)
            {
                _files[i] = file;
                file = file << 1;
            }

            _rookFiles = new BitBoard[64];
            for (byte i = 0; i < _rookFiles.Length; i++)
            {
                _rookFiles[i] = _files[i % 8] ^ i.AsBitBoard();
            }

            _rookRanks = new BitBoard[64];
            for (byte i = 0; i < _rookRanks.Length; i++)
            {
                _rookRanks[i] = _ranks[i / 8] ^ i.AsBitBoard();
            }

            _notFileA = ~_files[0];
            _notFileH = ~_files[7];

            for (int i = 0; i < _notRanks.Length; i++)
            {
                _notRanks[i] = ~_ranks[i];
            }
        }

        private void SetCastles()
        {
            _whiteSmallCastleCondition = new BitBoard();
            _whiteSmallCastleCondition = _whiteSmallCastleCondition.Set(5, 6);

            _whiteBigCastleCondition = new BitBoard();
            _whiteBigCastleCondition = _whiteBigCastleCondition.Set(1, 2, 3);

            _blackSmallCastleCondition = new BitBoard();
            _blackSmallCastleCondition = _blackSmallCastleCondition.Set(61, 62);

            _blackBigCastleCondition = new BitBoard();
            _blackBigCastleCondition = _blackBigCastleCondition.Set(57, 58, 59);

            _whiteBigCastleKing = new BitBoard();
            _whiteBigCastleKing = _whiteBigCastleKing.Or(4, 2);

            _whiteBigCastleRook = new BitBoard();
            _whiteBigCastleRook = _whiteBigCastleRook.Or(0, 3);

            _whiteSmallCastleKing = new BitBoard();
            _whiteSmallCastleKing = _whiteSmallCastleKing.Or(4, 6);

            _whiteSmallCastleRook = new BitBoard();
            _whiteSmallCastleRook = _whiteSmallCastleRook.Or(5, 7);

            _blackBigCastleKing = new BitBoard();
            _blackBigCastleKing = _blackBigCastleKing.Or(58, 60);

            _blackBigCastleRook = new BitBoard();
            _blackBigCastleRook = _blackBigCastleRook.Or(56, 59);

            _blackSmallCastleKing = new BitBoard();
            _blackSmallCastleKing = _blackSmallCastleKing.Or(60, 62);

            _blackSmallCastleRook = new BitBoard();
            _blackSmallCastleRook = _blackSmallCastleRook.Or(61, 63);
        }

        #endregion

        #region Evaluation

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EvaluateOpposite()
        {
            _evaluationService = _evaluationServices[_phase];

            if (_phase == Phase.Opening) return EvaluateBlackOpening() - EvaluateWhiteOpening();
            if (_phase == Phase.Middle) return EvaluateBlackMiddle() - EvaluateWhiteMiddle();
            return EvaluateBlackEnd() - EvaluateWhiteEnd();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Evaluate()
        {
            _evaluationService = _evaluationServices[_phase];

            if (_phase == Phase.Opening) return EvaluateWhiteOpening() - EvaluateBlackOpening();
            if (_phase == Phase.Middle) return EvaluateWhiteMiddle() - EvaluateBlackMiddle();
            return EvaluateWhiteEnd() - EvaluateWhiteEnd();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteOpening()
        {
            return GetWhitePawnValue() + GetWhiteKnightValue() + GetWhiteBishopValue() +
                   EvaluateWhiteRookOpening() + EvaluateWhiteQueenOpening() + EvaluateWhiteKingOpening();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteMiddle()
        {
            return GetWhitePawnValue() + GetWhiteKnightValue() + GetWhiteBishopValue() +
                   EvaluateWhiteRookMiddle() + EvaluateWhiteQueenMiddle() + EvaluateWhiteKingMiddle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteEnd()
        {
            return GetWhitePawnValue() + GetWhiteKnightValue() + GetWhiteBishopValue() +
                   EvaluateWhiteRookEnd() + EvaluateWhiteQueenEnd() + EvaluateWhiteKingEnd();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteRookOpening()
        {
            _boards[WhiteRook].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(WhiteRook, coordinate);

                if ((_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn]))
                    .IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();
                }
                else if ((_rookFiles[coordinate] & _boards[WhitePawn]).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();
                }

                if (_boards[BlackQueen].Any() && _rookFiles[coordinate].IsSet(_boards[BlackQueen]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                if (_rookFiles[coordinate].IsSet(_boards[BlackKing]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                if ((coordinate.RookAttacks(~_empty) & _boards[WhiteRook]).Any())
                {
                    if ((_rookFiles[coordinate] & _boards[WhiteRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookVerticalValue();
                    }
                    else if ((_rookRanks[coordinate] & _boards[WhiteRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookHorizontalValue();
                    }
                }

                if ((coordinate.RookAttacks(~_empty) & _boards[WhiteQueen]).Any()
                    && (_rookFiles[coordinate] & _boards[WhiteQueen]).Any())
                {
                    value += _evaluationService.GetDoubleRookVerticalValue();
                }

                if ((_whiteRookKingPattern[coordinate] & _boards[WhiteKing]).Any() &&
                    (_whiteRookPawnPattern[coordinate] & _boards[WhitePawn]).Any())
                {
                    value -= _evaluationService.GetRookBlockedByKingValue();
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteRookMiddle()
        {
            _boards[WhiteRook].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(WhiteRook, coordinate);

                if ((_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn]))
                    .IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();
                }
                else if ((_rookFiles[coordinate] & _boards[WhitePawn]).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();
                }

                if (_boards[BlackQueen].Any() && _rookFiles[coordinate].IsSet(_boards[BlackQueen]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                if (_rookFiles[coordinate].IsSet(_boards[BlackKing]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                if ((coordinate.RookAttacks(~_empty) & _boards[WhiteRook]).Any())
                {
                    if ((_rookFiles[coordinate] & _boards[WhiteRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookVerticalValue();
                    }
                    else if ((_rookRanks[coordinate] & _boards[WhiteRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookHorizontalValue();
                    }
                }

                if ((coordinate.RookAttacks(~_empty) & _boards[WhiteQueen]).Any()
                    && (_rookFiles[coordinate] & _boards[WhiteQueen]).Any())
                {
                    value += _evaluationService.GetDoubleRookVerticalValue();
                }

                if ((_whiteRookKingPattern[coordinate] & _boards[WhiteKing]).Any() &&
                    (_whiteRookPawnPattern[coordinate] & _boards[WhitePawn]).Any())
                {
                    value -= _evaluationService.GetRookBlockedByKingValue();
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteRookEnd()
        {
            _boards[WhiteRook].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(WhiteRook, coordinate);

                if ((_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn]))
                    .IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();
                }
                else if ((_rookFiles[coordinate] & _boards[WhitePawn]).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();
                }

                if (_boards[BlackQueen].Any() && _rookFiles[coordinate].IsSet(_boards[BlackQueen]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                if (_rookFiles[coordinate].IsSet(_boards[BlackKing]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                if ((coordinate.RookAttacks(~_empty) & _boards[WhiteRook]).Any())
                {
                    if ((_rookFiles[coordinate] & _boards[WhiteRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookVerticalValue();
                    }
                    else if ((_rookRanks[coordinate] & _boards[WhiteRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookHorizontalValue();
                    }
                }

                if ((coordinate.RookAttacks(~_empty) & _boards[WhiteQueen]).Any()
                    && (_rookFiles[coordinate] & _boards[WhiteQueen]).Any())
                {
                    value += _evaluationService.GetDoubleRookVerticalValue();
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteQueenOpening()
        {
            _boards[WhiteQueen].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(WhiteQueen, coordinate);

                var attackPattern = _moveProvider.GetAttackPattern(WhiteQueen, coordinate);
                if (attackPattern.IsSet(_boards[BlackKing]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                value -= (short)((_moveProvider.GetAttackPattern(WhitePawn, coordinate) &
                                            _boards[WhitePawn]).Count()
                                            * _evaluationService.GetBishopBlockedByPawnValue());

                if ((coordinate.BishopAttacks(~_empty) & _boards[WhiteBishop]).Any())
                {
                    value += _evaluationService.GetBattaryValue();
                }
            }

            if ((_whiteQueenOpening & _boards[WhiteQueen]).IsZero())
            {
                value -= _evaluationService.GetEarlyQueenValue();
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteQueenMiddle()
        {
            _boards[WhiteQueen].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(WhiteQueen, coordinate);

                var attackPattern = _moveProvider.GetAttackPattern(WhiteQueen, coordinate);
                if (attackPattern.IsSet(_boards[BlackKing]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                value -= (short)((_moveProvider.GetAttackPattern(WhitePawn, coordinate) &
                                            _boards[WhitePawn]).Count()
                                            * _evaluationService.GetBishopBlockedByPawnValue());

                if ((coordinate.BishopAttacks(~_empty) & _boards[WhiteBishop]).Any())
                {
                    value += _evaluationService.GetBattaryValue();
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteQueenEnd()
        {
            _boards[WhiteQueen].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(WhiteQueen, coordinate);

                var attackPattern = _moveProvider.GetAttackPattern(WhiteQueen, coordinate);
                if (attackPattern.IsSet(_boards[BlackKing]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                value -= (short)((_moveProvider.GetAttackPattern(WhitePawn, coordinate) &
                                            _boards[WhitePawn]).Count()
                                            * _evaluationService.GetBishopBlockedByPawnValue());

                if ((coordinate.BishopAttacks(~_empty) & _boards[WhiteBishop]).Any())
                {
                    value += _evaluationService.GetBattaryValue();
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteKingOpening()
        {
            return WhiteKingSafety(_boards[WhiteKing].BitScanForward());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteKingMiddle()
        {
            return WhiteKingSafety(_boards[WhiteKing].BitScanForward());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateWhiteKingEnd()
        {
            return -KingPawnTrofism(_boards[WhiteKing].BitScanForward());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackOpening()
        {
            return GetBlackPawnValue() + GetBlackKnightValue() + GetBlackBishopValue() +
                   EvaluateBlackRookOpening() + EvaluateBlackQueenOpening() + EvaluateBlackKingOpening();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackMiddle()
        {
            return GetBlackPawnValue() + GetBlackKnightValue() + GetBlackBishopValue() +
                   EvaluateBlackRookMiddle() + EvaluateBlackQueenMiddle() + EvaluateBlackKingMiddle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackEnd()
        {
            return GetBlackPawnValue() + GetBlackKnightValue() + GetBlackBishopValue() +
                   EvaluateBlackRookEnd() + EvaluateBlackQueenEnd() + EvaluateBlackKingEnd();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackRookOpening()
        {
            _boards[BlackRook].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(BlackRook, coordinate);

                if ((_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn]))
                    .IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();
                }
                else if ((_rookFiles[coordinate] & _boards[BlackPawn]).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();
                }

                if (_boards[WhiteQueen].Any() && _rookFiles[coordinate].IsSet(_boards[WhiteQueen]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                if (_rookFiles[coordinate].IsSet(_boards[WhiteKing]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                if ((coordinate.RookAttacks(~_empty) & _boards[BlackRook]).Any())
                {
                    if ((_rookFiles[coordinate] & _boards[BlackRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookVerticalValue();
                    }
                    else if ((_rookRanks[coordinate] & _boards[BlackRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookHorizontalValue();
                    }
                }

                if ((coordinate.RookAttacks(~_empty) & _boards[BlackQueen]).Any()
                    && (_rookFiles[coordinate] & _boards[BlackQueen]).Any())
                {
                    value += _evaluationService.GetDoubleRookVerticalValue();
                }

                if ((_blackRookKingPattern[coordinate] & _boards[BlackKing]).Any() &&
                    (_blackRookPawnPattern[coordinate] & _boards[BlackPawn]).Any())
                {
                    value -= _evaluationService.GetRookBlockedByKingValue();
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackRookMiddle()
        {
            _boards[BlackRook].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(BlackRook, coordinate);

                if ((_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn]))
                    .IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();
                }
                else if ((_rookFiles[coordinate] & _boards[BlackPawn]).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();
                }

                if (_boards[WhiteQueen].Any() && _rookFiles[coordinate].IsSet(_boards[WhiteQueen]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                if (_rookFiles[coordinate].IsSet(_boards[WhiteKing]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                if ((coordinate.RookAttacks(~_empty) & _boards[BlackRook]).Any())
                {
                    if ((_rookFiles[coordinate] & _boards[BlackRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookVerticalValue();
                    }
                    else if ((_rookRanks[coordinate] & _boards[BlackRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookHorizontalValue();
                    }
                }

                if ((coordinate.RookAttacks(~_empty) & _boards[BlackQueen]).Any()
                    && (_rookFiles[coordinate] & _boards[BlackQueen]).Any())
                {
                    value += _evaluationService.GetDoubleRookVerticalValue();
                }

                if ((_blackRookKingPattern[coordinate] & _boards[BlackKing]).Any() &&
                    (_blackRookPawnPattern[coordinate] & _boards[BlackPawn]).Any())
                {
                    value -= _evaluationService.GetRookBlockedByKingValue();
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackRookEnd()
        {
            _boards[BlackRook].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(BlackRook, coordinate);

                if ((_rookFiles[coordinate] & (_boards[WhitePawn] | _boards[BlackPawn]))
                    .IsZero())
                {
                    value += _evaluationService.GetRookOnOpenFileValue();
                }
                else if ((_rookFiles[coordinate] & _boards[BlackPawn]).IsZero())
                {
                    value += _evaluationService.GetRookOnHalfOpenFileValue();
                }

                if (_boards[WhiteQueen].Any() && _rookFiles[coordinate].IsSet(_boards[WhiteQueen]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                if (_rookFiles[coordinate].IsSet(_boards[WhiteKing]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                if ((coordinate.RookAttacks(~_empty) & _boards[BlackRook]).Any())
                {
                    if ((_rookFiles[coordinate] & _boards[BlackRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookVerticalValue();
                    }
                    else if ((_rookRanks[coordinate] & _boards[BlackRook]).Any())
                    {
                        value += _evaluationService.GetDoubleRookHorizontalValue();
                    }
                }

                if ((coordinate.RookAttacks(~_empty) & _boards[BlackQueen]).Any()
                    && (_rookFiles[coordinate] & _boards[BlackQueen]).Any())
                {
                    value += _evaluationService.GetDoubleRookVerticalValue();
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackQueenOpening()
        {
            _boards[BlackQueen].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(BlackQueen, coordinate);

                var attackPattern = _moveProvider.GetAttackPattern(BlackQueen, coordinate);
                if (attackPattern.IsSet(_boards[WhiteKing]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                value -= (short)((_moveProvider.GetAttackPattern(BlackPawn, coordinate) &
                                            _boards[BlackPawn]).Count()
                                            * _evaluationService.GetBishopBlockedByPawnValue());

                if ((coordinate.BishopAttacks(~_empty) & _boards[BlackBishop]).Any())
                {
                    value += _evaluationService.GetBattaryValue();
                }
            }

            if ((_blackQueenOpening & _boards[BlackQueen]).IsZero())
            {
                value -= _evaluationService.GetEarlyQueenValue();
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackQueenMiddle()
        {
            _boards[BlackQueen].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(BlackQueen, coordinate);

                var attackPattern = _moveProvider.GetAttackPattern(BlackQueen, coordinate);
                if (attackPattern.IsSet(_boards[WhiteKing]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                value -= (short)((_moveProvider.GetAttackPattern(BlackPawn, coordinate) &
                                            _boards[BlackPawn]).Count()
                                            * _evaluationService.GetBishopBlockedByPawnValue());

                if ((coordinate.BishopAttacks(~_empty) & _boards[BlackBishop]).Any())
                {
                    value += _evaluationService.GetBattaryValue();
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackQueenEnd()
        {
            _boards[BlackQueen].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(BlackQueen, coordinate);

                var attackPattern = _moveProvider.GetAttackPattern(BlackQueen, coordinate);
                if (attackPattern.IsSet(_boards[WhiteKing]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                value -= (short)((_moveProvider.GetAttackPattern(BlackPawn, coordinate) &
                                            _boards[BlackPawn]).Count()
                                            * _evaluationService.GetBishopBlockedByPawnValue());

                if ((coordinate.BishopAttacks(~_empty) & _boards[BlackBishop]).Any())
                {
                    value += _evaluationService.GetBattaryValue();
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackKingOpening()
        {
            return BlackKingSafety(_boards[BlackKing].BitScanForward());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackKingMiddle()
        {
            return BlackKingSafety(_boards[BlackKing].BitScanForward());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EvaluateBlackKingEnd()
        {
            return -KingPawnTrofism(_boards[BlackKing].BitScanForward());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private short KingPawnTrofism(byte kingPosition)
        {
            BitList positions = stackalloc byte[16];

            (_boards[0] | _boards[6]).GetPositions(ref positions);

            return _evaluationService.Distance(kingPosition, positions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackKingSafety(byte kingPosition)
        {
            return BlackKingShieldValue(kingPosition) - BlackKingAttackValue(kingPosition) - BlackKingOpenValue(kingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackKingOpenValue(byte kingPosition)
        {
            short value = 0;
            var boards = _blackKingOpenFile[kingPosition];
            for (byte i = 0; i < boards.Length; i++)
            {
                if ((boards[i] & _blacks).IsZero())
                {
                    value += _evaluationService.GetKingZoneOpenFileValue();
                }
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackKingAttackValue(byte kingPosition)
        {
            byte attackingPiecesCount = 0;
            int valueOfAttacks = 0;
            var shield = _blackKingShield[kingPosition];

            var pawnAttacks = GetWhitePawnAttacks() & shield;
            if (pawnAttacks.Any())
            {
                attackingPiecesCount++;
                valueOfAttacks += pawnAttacks.Count() * _evaluationService.GetPawnAttackValue();
            }

            _boards[WhiteKnight].GetPositions(_positionList);
            for (byte i = 0; i < _positionList.Count; i++)
            {
                var attackPattern = _moveProvider.GetAttackPattern(WhiteKnight, _positionList[i]) & shield;
                if (!attackPattern.Any()) continue;

                attackingPiecesCount++;
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetKnightAttackValue();
            }

            _boards[WhiteKing].GetPositions(_positionList);
            for (byte i = 0; i < _positionList.Count; i++)
            {
                var attackPattern = _moveProvider.GetAttackPattern(WhiteKing, _positionList[i]) & shield;
                if (!attackPattern.Any()) continue;

                attackingPiecesCount++;
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetKingAttackValue();
            }

            _boards[WhiteBishop].GetPositions(_positionList);
            attackingPiecesCount++;
            for (byte i = 0; i < _positionList.Count; i++)
            {
                var bishopAttacks = _positionList[i].BishopAttacks(~_empty) & shield;
                if (!bishopAttacks.Any())
                    continue;

                attackingPiecesCount++;
                valueOfAttacks += bishopAttacks.Count() * _evaluationService.GetBishopAttackValue();
            }

            _boards[WhiteRook].GetPositions(_positionList);
            for (byte i = 0; i < _positionList.Count; i++)
            {
                var rookAttacks = _positionList[i].RookAttacks(~_empty) & shield;
                if (!rookAttacks.Any())
                    continue;

                attackingPiecesCount++;
                valueOfAttacks += rookAttacks.Count() * _evaluationService.GetRookAttackValue();
            }

            _boards[WhiteQueen].GetPositions(_positionList);
            for (byte i = 0; i < _positionList.Count; i++)
            {
                var queenAttacks = _positionList[i].QueenAttacks(~_empty) & shield;
                if (!queenAttacks.Any())
                    continue;

                attackingPiecesCount++;
                valueOfAttacks += queenAttacks.Count() * _evaluationService.GetQueenAttackValue();
            }

            if (valueOfAttacks == 0||attackingPiecesCount == 0) return 0;

            int v = (int)(valueOfAttacks * _evaluationService.GetAttackWeight(attackingPiecesCount));
            if (v < 1) return _evaluationService.GetUnitValue();
            return _evaluationService.GetUnitValue() * v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int BlackKingShieldValue(byte kingPosition)
        {
            return _evaluationService.GetKingShieldFaceValue() * (_blackKingFace[kingPosition] & _blacks).Count() +
                _evaluationService.GetKingShieldPreFaceValue() * (_blackKingFaceShield[kingPosition] & _blacks).Count();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackBishopValue()
        {
            _boards[BlackBishop].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;
            if (_positionList.Count > 1)
            {
                value += _evaluationService.GetDoubleBishopValue();
            }

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(BlackBishop, coordinate);

                if ((_blackMinorDefense[coordinate] & _boards[BlackPawn]).Any())
                {
                    value += _evaluationService.GetMinorDefendedByPawnValue();
                }
                var attackPattern = _moveProvider.GetAttackPattern(BlackBishop, coordinate);
                if (_boards[WhiteQueen].Any() && attackPattern.IsSet(_boards[WhiteQueen]))
                {
                    value += _evaluationService.GetRentgenValue();
                }
                if (attackPattern.IsSet(_boards[WhiteKing]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                value -= (short)((_moveProvider.GetAttackPattern(BlackPawn, coordinate) &
                                            _boards[BlackPawn]).Count()
                                            * _evaluationService.GetBishopBlockedByPawnValue());
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackKnightValue()
        {
            _boards[BlackKnight].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(BlackKnight, coordinate);

                if ((_blackMinorDefense[coordinate] & _boards[BlackPawn]).Any())
                {
                    value += _evaluationService.GetMinorDefendedByPawnValue();
                }

                value -= (short)((_empty & _moveProvider.GetAttackPattern(BlackKnight, coordinate) & GetWhitePawnAttacks()).Count() *
                    _evaluationService.GetKnightAttackedByPawnValue());
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackPawnValue()
        {
            BitList positions = stackalloc byte[8];
            short value = 0;
            _boards[BlackPawn].GetPositions(ref positions);

            for (byte i = 0; i < positions.Count; i++)
            {
                byte coordinate = positions[i];
                value += _evaluationService.GetFullValue(BlackPawn, coordinate);
                if ((_blackBlockedPawns[coordinate] & _whites).Any())
                {
                    value -= _evaluationService.GetBlockedPawnValue();
                }

                if ((_blackIsolatedPawns[coordinate] & _boards[BlackPawn]).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue();
                }

                if ((_blackDoublePawns[coordinate] & _boards[BlackPawn]).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue();
                }

                if (coordinate < 32 && (_blackFacing[coordinate] & _boards[WhitePawn]).IsZero())
                {
                    if ((_blackPassedPawns[coordinate] & _boards[WhitePawn]).IsZero())
                    {
                        value += _evaluationService.GetPassedPawnValue();
                    }
                    else
                    {
                        value += _evaluationService.GetOpenPawnValue();
                    }
                }

                for (byte c = 0; c < _blackBackwardPawns[coordinate].Count; c++)
                {
                    if ((_blackBackwardPawns[coordinate][c].Key & _boards[BlackPawn]).IsZero() &&
                        (_blackBackwardPawns[coordinate][c].Value & _boards[WhitePawn]).Any())
                    {
                        value -= _evaluationService.GetBackwardPawnValue();
                        break;
                    }
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WhiteKingSafety(byte kingPosition)
        {
            return WhiteKingShieldValue(kingPosition) - WhiteKingAttackValue(kingPosition) - WhiteKingOpenValue(kingPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WhiteKingOpenValue(byte kingPosition)
        {
            short value = 0;
            var boards = _whiteKingOpenFile[kingPosition];
            for (byte i = 0; i < boards.Length; i++)
            {
                if ((boards[i] & _whites).IsZero())
                {
                    value += _evaluationService.GetKingZoneOpenFileValue();
                }
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WhiteKingAttackValue(byte kingPosition)
        {
            byte attackingPiecesCount = 0;
            int valueOfAttacks = 0;
            var shield = _whiteKingShield[kingPosition];

            var pawnAttacks = GetBlackPawnAttacks() & shield;
            if (pawnAttacks.Any())
            {
                attackingPiecesCount++;
                valueOfAttacks += pawnAttacks.Count() * _evaluationService.GetPawnAttackValue();
            }

            _boards[BlackKnight].GetPositions(_positionList);
            for (byte i = 0; i < _positionList.Count; i++)
            {
                var attackPattern = _moveProvider.GetAttackPattern(BlackKnight, _positionList[i]) & shield;
                if (!attackPattern.Any()) continue;

                attackingPiecesCount++;
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetKnightAttackValue();
            }

            _boards[BlackKing].GetPositions(_positionList);
            for (byte i = 0; i < _positionList.Count; i++)
            {
                var attackPattern = _moveProvider.GetAttackPattern(BlackKing, _positionList[i]) & shield;
                if (!attackPattern.Any()) continue;

                attackingPiecesCount++;
                valueOfAttacks += attackPattern.Count() * _evaluationService.GetKingAttackValue();
            }

            _boards[BlackBishop].GetPositions(_positionList);
            for (byte i = 0; i < _positionList.Count; i++)
            {
                var bishopAttacks = _positionList[i].BishopAttacks(~_empty) & shield;
                if (!bishopAttacks.Any())
                    continue;
                attackingPiecesCount++;
                valueOfAttacks += bishopAttacks.Count() * _evaluationService.GetBishopAttackValue();
            }

            _boards[BlackRook].GetPositions(_positionList);
            for (byte i = 0; i < _positionList.Count; i++)
            {
                var rookAttacks = _positionList[i].RookAttacks(~_empty) & shield;
                if (!rookAttacks.Any())
                    continue;

                attackingPiecesCount++;
                valueOfAttacks += rookAttacks.Count() * _evaluationService.GetRookAttackValue();
            }

            _boards[BlackQueen].GetPositions(_positionList);
            for (byte i = 0; i < _positionList.Count; i++)
            {
                var queenAttacks = _positionList[i].QueenAttacks(~_empty) & shield;
                if (!queenAttacks.Any())
                    continue;

                attackingPiecesCount++;
                valueOfAttacks += queenAttacks.Count() * _evaluationService.GetQueenAttackValue();
            }

            if (valueOfAttacks == 0 || attackingPiecesCount == 0) return 0;

            int v = (int)(valueOfAttacks * _evaluationService.GetAttackWeight(attackingPiecesCount));
            if (v < 1) return _evaluationService.GetUnitValue();
            return _evaluationService.GetUnitValue() * v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int WhiteKingShieldValue(byte kingPosition)
        {
            return _evaluationService.GetKingShieldFaceValue() * (_whiteKingFace[kingPosition] & _whites).Count()
                + _evaluationService.GetKingShieldPreFaceValue() * (_whiteKingFaceShield[kingPosition] & _whites).Count();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteBishopValue()
        {
            _boards[WhiteBishop].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;

            if (_positionList.Count > 1)
            {
                value += _evaluationService.GetDoubleBishopValue();
            }

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(WhiteBishop, coordinate);
                if ((_whiteMinorDefense[coordinate] & _boards[WhitePawn]).Any())
                {
                    value += _evaluationService.GetMinorDefendedByPawnValue();
                }
                var attackPattern = _moveProvider.GetAttackPattern(WhiteBishop, coordinate);
                if (_boards[BlackQueen].Any() && attackPattern.IsSet(_boards[BlackQueen]))
                {
                    value += _evaluationService.GetRentgenValue();
                }
                if (attackPattern.IsSet(_boards[BlackKing]))
                {
                    value += _evaluationService.GetRentgenValue();
                }

                value -= (short)((_moveProvider.GetAttackPattern(WhitePawn, coordinate) &
                                            _boards[WhitePawn]).Count()
                                            * _evaluationService.GetBishopBlockedByPawnValue());
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteKnightValue()
        {
            _boards[WhiteKnight].GetPositions(_positionList);
            if (_positionList.Count < 1) return 0;

            short value = 0;

            for (byte i = 0; i < _positionList.Count; i++)
            {
                byte coordinate = _positionList[i];
                value += _evaluationService.GetFullValue(WhiteKnight, coordinate);
                if ((_whiteMinorDefense[coordinate] & _boards[WhitePawn]).Any())
                {
                    value += _evaluationService.GetMinorDefendedByPawnValue();
                }

                value -= (short)((_empty & _moveProvider.GetAttackPattern(WhiteKnight, coordinate) & GetBlackPawnAttacks()).Count() *
                    _evaluationService.GetKnightAttackedByPawnValue());
            }
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhitePawnValue()
        {
            BitList positions = stackalloc byte[8];
            short value = 0;
            _boards[WhitePawn].GetPositions(ref positions);

            for (byte i = 0; i < positions.Count; i++)
            {
                byte coordinate = positions[i];
                value += _evaluationService.GetFullValue(WhitePawn, coordinate);

                if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
                {
                    value -= _evaluationService.GetBlockedPawnValue();
                }

                if ((_whiteIsolatedPawns[coordinate] & _boards[WhitePawn]).IsZero())
                {
                    value -= _evaluationService.GetIsolatedPawnValue();
                }

                if ((_whiteDoublePawns[coordinate] & _boards[WhitePawn]).Any())
                {
                    value -= _evaluationService.GetDoubledPawnValue();
                }

                if (coordinate > 31 && (_whiteFacing[coordinate] & _boards[BlackPawn]).IsZero())
                {
                    if ((_whitePassedPawns[coordinate] & _boards[BlackPawn]).IsZero())
                    {
                        value += _evaluationService.GetPassedPawnValue();
                    }
                    else
                    {
                        value += _evaluationService.GetOpenPawnValue();
                    }
                }

                for (byte c = 0; c < _whiteBackwardPawns[coordinate].Count; c++)
                {
                    if ((_whiteBackwardPawns[coordinate][c].Key & _boards[WhitePawn]).IsZero() &&
                        (_whiteBackwardPawns[coordinate][c].Value & _boards[BlackPawn]).Any())
                    {
                        value -= _evaluationService.GetBackwardPawnValue();
                        break;
                    }
                }
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetStaticValue()
        {
            _evaluationService = _evaluationServices[_phase];
            return GetWhiteStaticValue() - GetBlackStaticValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKingSafetyValue()
        {
            _evaluationService = _evaluationServices[_phase];
            return WhiteKingSafety(_boards[5].BitScanForward()) - BlackKingSafety(_boards[11].BitScanForward());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPawnValue()
        {
            _evaluationService = _evaluationServices[_phase];
            return GetWhitePawnValue() - GetBlackPawnValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBlackStaticValue()
        {
            int value = 0;
            for (byte i = 6; i < 11; i++)
            {
                value += _evaluationService.GetValue(i) * _boards[i].Count();
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWhiteStaticValue()
        {
            int value = 0;
            for (byte i = 0; i < 5; i++)
            {
                value += _evaluationService.GetValue(i) * _boards[i].Count();
            }

            return value;
        }

        #endregion

        public override string ToString()
        {
            char[] pieceUnicodeChar =
            {
                '\u2659', '\u2658', '\u2657', '\u2656', '\u2655', '\u2654',
                '\u265F', '\u265E', '\u265D', '\u265C', '\u265B', '\u265A', ' '
            };
            var piecesNames = pieceUnicodeChar.Select(c => c.ToString()).ToArray();

            StringBuilder builder = new StringBuilder();
            for (byte y = 7; y >= 0; y--)
            {
                for (byte x = 0; x < 8; x++)
                {
                    byte i = (byte)(y * 8 + x);
                    string v = piecesNames.Last();
                    if (!_empty.IsSet(i.AsBitBoard()))
                    {
                        v = piecesNames[_pieces[i]];
                    }

                    builder.Append($"[ {v} ]");
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
