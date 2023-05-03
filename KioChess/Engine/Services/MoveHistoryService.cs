﻿using System.Runtime.CompilerServices;
using System.Text;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Helpers;
using Engine.Models.Moves;

namespace Engine.Services
{
    public class MoveHistoryService: IMoveHistoryService
    {
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

                if (piece == WhiteKing)
                {
                    _whiteSmallCastleHistory[_ply] = false;
                    _whiteBigCastleHistory[_ply] = false;
                    return;
                }

                _whiteSmallCastleHistory[_ply] = _whiteSmallCastleHistory[ply] && (piece != WhiteRook || move.From != H1);
                _whiteBigCastleHistory[_ply] = _whiteBigCastleHistory[ply] && (piece != WhiteRook || move.From != A1);
            }
            else
            {
                _whiteSmallCastleHistory[_ply] = _whiteSmallCastleHistory[ply];
                _whiteBigCastleHistory[_ply] = _whiteBigCastleHistory[ply];

                if (piece == BlackKing)
                {
                    _blackSmallCastleHistory[_ply] = false;
                    _blackBigCastleHistory[_ply] = false;
                    return;
                }

                _blackSmallCastleHistory[_ply] = _blackSmallCastleHistory[ply] && (piece != BlackRook || move.From != H8);
                _blackBigCastleHistory[_ply] = _blackBigCastleHistory[ply] && (piece != BlackRook || move.From != A8);
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

            byte count = 1;
            int offset = _ply - _reversibleMovesHistory[_ply];

            for (var i = _boardHistory.Count - 5; i > offset; i-=2)
            {
                if (_boardHistory[i] != board)
                {
                    continue;
                }

                count++;
                if (count > 2)
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