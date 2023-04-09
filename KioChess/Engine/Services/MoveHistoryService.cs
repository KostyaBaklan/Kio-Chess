using System.Runtime.CompilerServices;
using System.Text;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Engine.Services
{
    public class MoveHistoryService: IMoveHistoryService
    {
        private int _ply = -1;
        private readonly bool[] _whiteSmallCastleHistory;
        private readonly bool[] _whiteBigCastleHistory;
        private readonly bool[] _blackSmallCastleHistory;
        private readonly bool[] _blackBigCastleHistory;
        private readonly ArrayStack<MoveBase> _history;
        private readonly ArrayStack<ulong> _boardHistory;
        private readonly int[] _reversibleMovesHistory;

        public MoveHistoryService()
        {
            var historyDepth = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .GeneralConfiguration.GameDepth;
            _whiteSmallCastleHistory = new bool[historyDepth];
            _whiteBigCastleHistory = new bool[historyDepth];
            _blackSmallCastleHistory = new bool[historyDepth];
            _blackBigCastleHistory = new bool[historyDepth];
            _history = new ArrayStack<MoveBase>(historyDepth);
            _boardHistory = new ArrayStack<ulong>(historyDepth); 
            _reversibleMovesHistory = new int[historyDepth]; 
        }

        #region Implementation of IMoveHistoryService

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPly()
        {
            return _ply;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveBase GetLastMove()
        {
            return _history.Peek();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddFirst(MoveBase move)
        {
            _history.Push(move);
            _ply++;

            AddMoveHistory(move.IsIrreversible);

            _whiteSmallCastleHistory[0] = true;
            _whiteBigCastleHistory[0] = true;
            _blackSmallCastleHistory[0] = true;
            _blackBigCastleHistory[0] = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(MoveBase move)
        {
            _history.Push(move);
            var ply = _ply;
            _ply++;

            AddMoveHistory(move.IsIrreversible);

            var piece = move.Piece;
            if (piece.IsWhite())
            {
                _blackSmallCastleHistory[_ply] = _blackSmallCastleHistory[ply];
                _blackBigCastleHistory[_ply] = _blackBigCastleHistory[ply];

                if (piece == Piece.WhiteKing)
                {
                    _whiteSmallCastleHistory[_ply] = false;
                    _whiteBigCastleHistory[_ply] = false;
                    return;
                }

                _whiteSmallCastleHistory[_ply] = _whiteSmallCastleHistory[ply] && (piece != Piece.WhiteRook || move.From != Squares.H1);
                _whiteBigCastleHistory[_ply] = _whiteBigCastleHistory[ply] && (piece != Piece.WhiteRook || move.From != Squares.A1);
            }
            else
            {
                _whiteSmallCastleHistory[_ply] = _whiteSmallCastleHistory[ply];
                _whiteBigCastleHistory[_ply] = _whiteBigCastleHistory[ply];

                if (piece == Piece.BlackKing)
                {
                    _blackSmallCastleHistory[_ply] = false;
                    _blackBigCastleHistory[_ply] = false;
                    return;
                }

                _blackSmallCastleHistory[_ply] = _blackSmallCastleHistory[ply] && (piece != Piece.BlackRook || move.From != Squares.H8);
                _blackBigCastleHistory[_ply] = _blackBigCastleHistory[ply] && (piece != Piece.BlackRook || move.From != Squares.A8);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveBase Remove()
        {
            _ply--;
            return _history.Pop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackCastle()
        {
            return _blackSmallCastleHistory[_ply] || _blackBigCastleHistory[_ply];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteCastle()
        {
            return _whiteSmallCastleHistory[_ply] || _whiteBigCastleHistory[_ply];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteSmallCastle()
        {
            return _whiteSmallCastleHistory[_ply];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoWhiteBigCastle()
        {
            return _whiteBigCastleHistory[_ply];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackSmallCastle()
        {
            return _blackSmallCastleHistory[_ply];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanDoBlackBigCastle()
        {
            return _blackBigCastleHistory[_ply];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<MoveBase> GetHistory()
        {
            return _history.Items();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsThreefoldRepetition(ulong board)
        {
            if (_reversibleMovesHistory[_ply] < 8)
            {
                return false;
            }

            int count = 1;
            int offset = _ply - _reversibleMovesHistory[_ply];

            for (var i = _boardHistory.Count - 5; i > offset; i-=2)
            {
                if (_boardHistory[i] != board)
                {
                    continue;
                }

                count++;
                if (count >= 3)
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFiftyMoves()
        {
            return _reversibleMovesHistory[_ply] > 99;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ulong board)
        {
            _boardHistory.Push(board);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(ulong board)
        {
            _boardHistory.Pop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLastMoveWasCheck()
        {
            return _history.Peek().IsCheck;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLastMoveWasPassed()
        {
            return _history.Peek().IsPassed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLastMoveNotReducible()
        {
            var peek = _history.Peek();
            return peek.IsCheck||peek.IsPassed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLast(short key)
        {
            return _history.Peek().Key == key;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddMoveHistory(bool isIrreversible)
        {
            if (isIrreversible)
            {
                _reversibleMovesHistory[_ply] = 0;
            }
            else
            {
                if (_ply > 0)
                {
                    _reversibleMovesHistory[_ply] = _reversibleMovesHistory[_ply - 1] + 1;
                }
                else
                {
                    _reversibleMovesHistory[_ply] = 1;
                }
            }
        }

        #region Overrides of Object

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (var i = 0; i < _history.Count; i++)
            {
                builder.Append(_history[i]);
            }
            return builder.ToString();
        }

        #endregion
    }
}