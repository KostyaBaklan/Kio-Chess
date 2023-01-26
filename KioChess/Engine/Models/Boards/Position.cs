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
using Engine.Sorting.Sorters;
using Engine.Strategies.Models;

namespace Engine.Models.Boards
{
    public class Position : IPosition
    {
        private Turn _turn;
        private Phase _phase;
        private SortContext _sortContext;
        private readonly ArrayStack<Piece> _figureHistory;

        private readonly byte[][] _white;
        private readonly byte[][] _black;
        private readonly byte[][] _whiteAttacks;
        private readonly byte[][] _blackAttacks;

        private readonly SquareList[] _squares;

        private readonly AttackList _attacks;
        private readonly AttackList _attacksTemp;

        private readonly MoveList _moves;
        private readonly MoveList _movesTemp;

        private readonly PromotionList _promotions;
        private readonly PromotionList _promotionsTemp;

        private readonly IBoard _board;
        private readonly IMoveProvider _moveProvider;
        private readonly IMoveHistoryService _moveHistoryService;

        public Position()
        {
            _turn = Turn.White;

            IPieceOrderConfiguration pieceOrderConfiguration = ServiceLocator.Current.GetInstance<IConfigurationProvider>().PieceOrderConfiguration;

            _white = pieceOrderConfiguration.Whites.Select(pair => pair.Value.Select(p => p.AsByte()).ToArray()).ToArray();
            _black = pieceOrderConfiguration.Blacks.Select(pair => pair.Value.Select(p => p.AsByte()).ToArray()).ToArray();
            _whiteAttacks = pieceOrderConfiguration.WhitesAttacks.Select(pair => pair.Value.Select(p => p.AsByte()).ToArray()).ToArray();
            _blackAttacks = pieceOrderConfiguration.BlacksAttacks.Select(pair => pair.Value.Select(p => p.AsByte()).ToArray()).ToArray();

            _squares = new SquareList[6];
            for (int i = 0; i < _squares.Length; i++)
            {
                _squares[i] = new SquareList();
            }

            _attacks = new AttackList();
            _attacksTemp = new AttackList();
            _moves = new MoveList();
            _movesTemp = new MoveList();
            _promotions = new PromotionList();
            _promotionsTemp = new PromotionList();

            _board = new Board();
            _figureHistory = new ArrayStack<Piece>();
            _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
            _moveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        #region Implementation of IPosition

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetPiece(Square cell, out Piece? piece)
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
            return (short) -_board.GetValue();
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
        public IEnumerable<AttackBase> GetAllAttacks(Square cell, Piece piece)
        {
            var attacks = _moveProvider.GetAttacks(piece, cell);
            return _turn == Turn.White
                ? attacks.Where(a => IsWhiteLigal(a))
                : attacks.Where(a => IsBlackLigal(a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<MoveBase> GetAllMoves(Square cell, Piece piece)
        {
            var moves = _moveProvider.GetMoves(piece, cell);
            return _turn == Turn.White
                ? moves.Where(a => IsWhiteLigal(a))
                : moves.Where(a => IsBlackLigal(a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveList GetAllAttacks(IMoveSorter sorter)
        {
            return _turn == Turn.White ? GetAllWhiteAttacks(sorter) : GetAllBlackAttacks(sorter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MoveList GetAllBlackAttacks(IMoveSorter sorter)
        {
            GetSquares(_blackAttacks[(byte)_phase]);
            return sorter.Order(PossibleBlackAttacks(_blackAttacks[(byte)_phase]));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MoveList GetAllWhiteAttacks(IMoveSorter sorter)
        {
            GetSquares(_whiteAttacks[(byte)_phase]);
            return sorter.Order(PossibleWhiteAttacks(_whiteAttacks[(byte)_phase]));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AttackList GetWhiteAttacks()
        {
            GetSquares(_whiteAttacks[(byte)_phase]);
            return PossibleSingleWhiteAttacks(_whiteAttacks[(byte)_phase]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AttackList GetBlackAttacks()
        {
            GetSquares(_blackAttacks[(byte)_phase]);
            return PossibleSingleBlackAttacks(_blackAttacks[(byte)_phase]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveList GetAllMoves(SortContext sortContext)
        {
            _sortContext = sortContext;
            sortContext.InitializeSort();
            if(_turn == Turn.White)
            {
                _sortContext.Pieces = _white[(byte)_phase];
                GetSquares(_sortContext.Pieces, _sortContext.Squares);

                if (sortContext.HasPv)
                {
                    if (sortContext.IsPvCapture)
                    {
                        ProcessWhiteCapuresWithPv();
                        if (_board.CanWhitePromote())
                        {
                            _board.GetWhitePromotionSquares(sortContext.PromotionSquares);
                            ProcessWhitePromotionsWithoutPv();
                        }
                        ProcessWhiteMovesWithoutPv();
                    }
                    else
                    {
                        ProcessWhiteCapuresWithoutPv(); 
                        if (_board.CanWhitePromote())
                        {
                            _board.GetWhitePromotionSquares(sortContext.PromotionSquares);
                            ProcessWhitePromotionsWithPv();
                        }
                        ProcessWhiteMovesWithPv();
                    }
                }
                else
                {
                    ProcessWhiteCapuresWithoutPv(); 
                    if (_board.CanWhitePromote())
                    {
                        _board.GetWhitePromotionSquares(sortContext.PromotionSquares);
                        ProcessWhitePromotionsWithoutPv();
                    }
                    ProcessWhiteMovesWithoutPv();
                }
            }
            else
            {
                _sortContext.Pieces = _black[(byte)_phase];
                GetSquares(_sortContext.Pieces, _sortContext.Squares);

                if (sortContext.HasPv)
                {
                    if (sortContext.IsPvCapture)
                    {
                        ProcessBlackCapuresWithPv();
                        if (_board.CanBlackPromote())
                        {
                            _board.GetBlackPromotionSquares(sortContext.PromotionSquares);
                            ProcessBlackPromotionsWithoutPv();
                        }
                        ProcessBlackMovesWithoutPv();
                    }
                    else
                    {
                        ProcessBlackCapuresWithoutPv();
                        if (_board.CanBlackPromote())
                        {
                            _board.GetBlackPromotionSquares(sortContext.PromotionSquares);
                            ProcessBlackPromotionsWithPv();
                        }
                        ProcessBlackMovesWithPv();
                    }
                }
                else
                {
                    ProcessBlackCapuresWithoutPv();
                    if (_board.CanBlackPromote())
                    {
                        _board.GetBlackPromotionSquares(sortContext.PromotionSquares);
                        ProcessBlackPromotionsWithoutPv();
                    }
                    ProcessBlackMovesWithoutPv();
                }
            }
            return sortContext.GetMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhitePromotionsWithPv()
        {
            for (var f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                _moveProvider.GetPromotions(0, _sortContext.PromotionSquares[f], _promotionsTemp);

                if (_promotionsTemp.Count == 0 || !IsWhiteLigal(_promotionsTemp[0]))
                    continue;
                for (var i = 0; i < _promotionsTemp.Count; i++)
                {
                    var move = _promotionsTemp[i];

                    if (_sortContext.Pv == move.Key)
                    {
                        _sortContext.ProcessHashMove(move);
                    }
                    else
                    {
                        _sortContext.ProcessPromotionMove(move);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhitePromotionsWithoutPv()
        {
            for (var f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                _moveProvider.GetPromotions(0, _sortContext.PromotionSquares[f], _promotionsTemp);

                if (_promotionsTemp.Count > 0 && IsWhiteLigal(_promotionsTemp[0]))
                {
                    _sortContext.ProcessPromotionMoves(_promotionsTemp);
                }
                //for (var i = 0; i < _promotionsTemp.Count; i++)
                //{
                //    var move = _promotionsTemp[i];
                //    if (IsWhiteLigal(move))
                //        _sortContext.ProcessPromotionMove(move);
                //}
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackPromotionsWithPv()
        {
            for (var f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                _moveProvider.GetPromotions(6, _sortContext.PromotionSquares[f], _promotionsTemp);

                if (_promotionsTemp.Count <= 0 || !IsBlackLigal(_promotionsTemp[0]))
                    continue;
                for (var i = 0; i < _promotionsTemp.Count; i++)
                {
                    var move = _promotionsTemp[i];

                    if (_sortContext.Pv == move.Key)
                    {
                        _sortContext.ProcessHashMove(move);
                    }
                    else
                    {
                        _sortContext.ProcessPromotionMove(move);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackPromotionsWithoutPv()
        {
            for (var f = 0; f < _sortContext.PromotionSquares.Length; f++)
            {
                _moveProvider.GetPromotions(6, _sortContext.PromotionSquares[f], _promotionsTemp);

                if (_promotionsTemp.Count > 0 && IsBlackLigal(_promotionsTemp[0]))
                {
                    _sortContext.ProcessPromotionMoves(_promotionsTemp);
                }

                //for (var i = 0; i < _promotionsTemp.Count; i++)
                //{
                //    var move = _promotionsTemp[i];
                //    if (IsBlackLigal(move))
                //        _sortContext.ProcessPromotionMove(move);
                //}
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteCapuresWithPv()
        {
            for (var index = 0; index < _sortContext.Pieces.Length; index++)
            {
                var p = _sortContext.Pieces[index];

                var square = _sortContext.Squares[p % 6];
                for (var f = 0; f < square.Length; f++)
                {
                    _moveProvider.GetAttacks(p, square[f], _attacksTemp);

                    for (var i = 0; i < _attacksTemp.Count; i++)
                    {
                        var capture = _attacksTemp[i];
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
            }

            _sortContext.FinalizeSort();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteMovesWithPv()
        {
            if (_moveHistoryService.CanDoWhiteCastle())
            {
                for (var index = 0; index < _sortContext.Pieces.Length; index++)
                {
                    var p = _sortContext.Pieces[index];
                    var from = _sortContext.Squares[p % 6];

                    for (var f = 0; f < from.Length; f++)
                    {
                        _moveProvider.GetMoves(p, @from[f], _movesTemp);
                        for (var i = 0; i < _movesTemp.Count; i++)
                        {
                            var move = _movesTemp[i];
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
                            else if (move.IsCastle)
                            {
                                _sortContext.ProcessCastleMove(move);
                            }
                            else
                            {
                                _sortContext.ProcessMove(move);
                            }
                        }
                    }
                }
            }
            else
            {
                for (var index = 0; index < _sortContext.Pieces.Length; index++)
                {
                    var p = _sortContext.Pieces[index];
                    var from = _sortContext.Squares[p % 6];

                    for (var f = 0; f < from.Length; f++)
                    {
                        _moveProvider.GetMoves(p, @from[f], _movesTemp);
                        for (var i = 0; i < _movesTemp.Count; i++)
                        {
                            var move = _movesTemp[i];
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
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteCapuresWithoutPv()
        {
            for (var index = 0; index < _sortContext.Pieces.Length; index++)
            {
                var p = _sortContext.Pieces[index];

                var square = _sortContext.Squares[p % 6];
                for (var f = 0; f < square.Length; f++)
                {
                    _moveProvider.GetAttacks(p, square[f], _attacksTemp);

                    for (var i = 0; i < _attacksTemp.Count; i++)
                    {
                        if (IsWhiteLigal(_attacksTemp[i]))
                        {
                            _sortContext.ProcessCaptureMove(_attacksTemp[i]);
                        }
                    }
                }
            }

            _sortContext.FinalizeSort();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessWhiteMovesWithoutPv()
        {
            if (_moveHistoryService.CanDoWhiteCastle())
            {
                for (var index = 0; index < _sortContext.Pieces.Length; index++)
                {
                    var p = _sortContext.Pieces[index];
                    var from = _sortContext.Squares[p % 6];

                    for (var f = 0; f < from.Length; f++)
                    {
                        _moveProvider.GetMoves(p, @from[f], _movesTemp);
                        for (var i = 0; i < _movesTemp.Count; i++)
                        {
                            var move = _movesTemp[i];
                            if (!IsWhiteLigal(move))
                                continue;

                            if (_sortContext.IsKiller(move.Key))
                            {
                                _sortContext.ProcessKillerMove(move);
                            }
                            else if (move.IsCastle)
                            {
                                _sortContext.ProcessCastleMove(move);
                            }
                            else
                            {
                                _sortContext.ProcessMove(move);
                            }
                        }
                    }
                }
            }
            else
            {
                for (var index = 0; index < _sortContext.Pieces.Length; index++)
                {
                    var p = _sortContext.Pieces[index];
                    var from = _sortContext.Squares[p % 6];

                    for (var f = 0; f < from.Length; f++)
                    {
                        _moveProvider.GetMoves(p, @from[f], _movesTemp);
                        for (var i = 0; i < _movesTemp.Count; i++)
                        {
                            var move = _movesTemp[i];
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
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackCapuresWithPv()
        {
            for (var index = 0; index < _sortContext.Pieces.Length; index++)
            {
                var p = _sortContext.Pieces[index];

                var square = _sortContext.Squares[p % 6];
                for (var f = 0; f < square.Length; f++)
                {
                    _moveProvider.GetAttacks(p, square[f], _attacksTemp);

                    for (var i = 0; i < _attacksTemp.Count; i++)
                    {
                        var capture = _attacksTemp[i];
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
            }

            _sortContext.FinalizeSort();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackMovesWithPv()
        {
            if (_moveHistoryService.CanDoBlackCastle())
            {
                for (var index = 0; index < _sortContext.Pieces.Length; index++)
                {
                    var p = _sortContext.Pieces[index];
                    var from = _sortContext.Squares[p % 6];

                    for (var f = 0; f < from.Length; f++)
                    {
                        _moveProvider.GetMoves(p, @from[f], _movesTemp);
                        for (var i = 0; i < _movesTemp.Count; i++)
                        {
                            var move = _movesTemp[i];
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
                            else if (move.IsCastle)
                            {
                                _sortContext.ProcessCastleMove(move);
                            }
                            else
                            {
                                _sortContext.ProcessMove(move);
                            }
                        }
                    }
                }
            }
            else
            {
                for (var index = 0; index < _sortContext.Pieces.Length; index++)
                {
                    var p = _sortContext.Pieces[index];
                    var from = _sortContext.Squares[p % 6];

                    for (var f = 0; f < from.Length; f++)
                    {
                        _moveProvider.GetMoves(p, @from[f], _movesTemp);
                        for (var i = 0; i < _movesTemp.Count; i++)
                        {
                            var move = _movesTemp[i];
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
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackCapuresWithoutPv()
        {
            for (var index = 0; index < _sortContext.Pieces.Length; index++)
            {
                var p = _sortContext.Pieces[index];

                var square = _sortContext.Squares[p % 6];
                for (var f = 0; f < square.Length; f++)
                {
                    _moveProvider.GetAttacks(p, square[f], _attacksTemp);

                    for (var i = 0; i < _attacksTemp.Count; i++)
                    {
                        if (IsBlackLigal(_attacksTemp[i]))
                        {
                            _sortContext.ProcessCaptureMove(_attacksTemp[i]);
                        }
                    }
                }
            }

            _sortContext.FinalizeSort();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessBlackMovesWithoutPv()
        {
            if (_moveHistoryService.CanDoBlackCastle())
            {
                for (var index = 0; index < _sortContext.Pieces.Length; index++)
                {
                    var p = _sortContext.Pieces[index];
                    var from = _sortContext.Squares[p % 6];

                    for (var f = 0; f < from.Length; f++)
                    {
                        _moveProvider.GetMoves(p, @from[f], _movesTemp);
                        for (var i = 0; i < _movesTemp.Count; i++)
                        {
                            var move = _movesTemp[i];
                            if (!IsBlackLigal(move))
                                continue;

                            if (_sortContext.IsKiller(move.Key))
                            {
                                _sortContext.ProcessKillerMove(move);
                            }
                            else if (move.IsCastle)
                            {
                                _sortContext.ProcessCastleMove(move);
                            }
                            else
                            {
                                _sortContext.ProcessMove(move);
                            }
                        }
                    }
                }
            }
            else
            {
                for (var index = 0; index < _sortContext.Pieces.Length; index++)
                {
                    var p = _sortContext.Pieces[index];
                    var from = _sortContext.Squares[p % 6];

                    for (var f = 0; f < from.Length; f++)
                    {
                        _moveProvider.GetMoves(p, @from[f], _movesTemp);
                        for (var i = 0; i < _movesTemp.Count; i++)
                        {
                            var move = _movesTemp[i];
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
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveList GetAllMoves(IMoveSorter sorter, MoveBase pvMove = null)
        {
            return _turn == Turn.White ? GetAllWhiteMoves(sorter, pvMove) : GetAllBlackMoves(sorter, pvMove);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MoveList GetAllBlackMoves(IMoveSorter sorter, MoveBase pvMove)
        {
            var pieces = _black[(byte)_phase];
            GetSquares(pieces);
            return sorter.Order(PossibleBlackAttacks(pieces), PossibleBlackMoves(pieces), pvMove);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MoveList GetAllWhiteMoves(IMoveSorter sorter, MoveBase pvMove)
        {
            var pieces =  _white[(byte)_phase];
            GetSquares(pieces);
            return sorter.Order(PossibleWhiteAttacks(pieces), PossibleWhiteMoves(pieces), pvMove);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MoveList PossibleWhiteMoves(byte[] pieces)
        {
            _moves.Clear();
            for (var index = 0; index < pieces.Length; index++)
            {
                var p = pieces[index];
                var from = _squares[p % 6];

                for (var f = 0; f < from.Length; f++)
                {
                    _moveProvider.GetMoves(p, @from[f], _movesTemp);
                    for (var i = 0; i < _movesTemp.Count; i++)
                    {
                        if (IsWhiteLigal(_movesTemp[i]))
                        {
                            _moves.Add(_movesTemp[i]);
                        }
                    }
                }
            }

            return _moves;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MoveList PossibleBlackMoves(byte[] pieces)
        {
            _moves.Clear();
            for (var index = 0; index < pieces.Length; index++)
            {
                var p = pieces[index];
                var from = _squares[p % 6];

                for (var f = 0; f < from.Length; f++)
                {
                    _moveProvider.GetMoves(p, @from[f], _movesTemp);
                    for (var i = 0; i < _movesTemp.Count; i++)
                    {
                        if (IsBlackLigal(_movesTemp[i]))
                        {
                            _moves.Add(_movesTemp[i]);
                        }
                    }
                }
            }

            return _moves;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AttackList PossibleSingleWhiteAttacks(byte[] pieces)
        {
            BitBoard to = new BitBoard();
            _attacks.Clear();
            for (var index = 0; index < pieces.Length; index++)
            {
                var p = pieces[index];

                var square = _squares[p % 6];
                for (var f = 0; f < square.Length; f++)
                {
                    _moveProvider.GetAttacks(p, square[f], _attacksTemp);

                    for (var i = 0; i < _attacksTemp.Count; i++)
                    {
                        var attack = _attacksTemp[i];
                        if (to.IsSet(attack.To.AsBitBoard())) continue;

                        if (IsWhiteLigal(attack))
                        {
                            _attacks.Add(attack);
                        }
                        to |= attack.To.AsBitBoard();
                    }
                }
            }

            return _attacks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AttackList PossibleSingleBlackAttacks(byte[] pieces)
        {
            BitBoard to = new BitBoard();
            _attacks.Clear();
            for (var index = 0; index < pieces.Length; index++)
            {
                var p = pieces[index];

                var square = _squares[p % 6];
                for (var f = 0; f < square.Length; f++)
                {
                    _moveProvider.GetAttacks(p, square[f], _attacksTemp);

                    for (var i = 0; i < _attacksTemp.Count; i++)
                    {
                        var attack = _attacksTemp[i];
                        if (to.IsSet(attack.To.AsBitBoard())) continue;

                        if (IsBlackLigal(attack))
                        {
                            _attacks.Add(attack);
                        }
                        to |= attack.To.AsBitBoard();
                    }
                }
            }

            return _attacks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AttackList PossibleBlackAttacks(byte[] pieces)
        {
            _attacks.Clear();
            for (var index = 0; index < pieces.Length; index++)
            {
                var p = pieces[index];

                var square = _squares[p % 6];
                for (var f = 0; f < square.Length; f++)
                {
                    _moveProvider.GetAttacks(p, square[f],_attacksTemp);

                    for (var i = 0; i < _attacksTemp.Count; i++)
                    {
                        if (IsBlackLigal(_attacksTemp[i]))
                        {
                            _attacks.Add(_attacksTemp[i]);
                        }
                    }
                }
            }

            return _attacks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AttackList PossibleWhiteAttacks(byte[] pieces)
        {
            _attacks.Clear();
            for (var index = 0; index < pieces.Length; index++)
            {
                var p = pieces[index];

                var square = _squares[p % 6];
                for (var f = 0; f < square.Length; f++)
                {
                    _moveProvider.GetAttacks(p, square[f], _attacksTemp);

                    for (var i = 0; i < _attacksTemp.Count; i++)
                    {
                        if (IsWhiteLigal(_attacksTemp[i]))
                        {
                            _attacks.Add(_attacksTemp[i]);
                        }
                    }
                }
            }

            return _attacks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPieceValue(Square square)
        {
            return _board.GetPiece(square).AsValue();
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
            return  _moveProvider.AnyBlackCheck() || move.IsCastle &&
                  _moveProvider.IsWhiteUnderAttack(move.To == Squares.C1 ? Squares.D1 : Squares.F1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBlackNotLegal(MoveBase move)
        {
           return _moveProvider.AnyWhiteCheck() || move.IsCastle &&
                  _moveProvider.IsBlackUnderAttack(move.To == Squares.C8 ? Squares.D8 : Squares.F8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Phase GetPhase()
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
        private void GetSquares(byte[] pieces, SquareList[] squares)
        {
            for (var i = 0; i < squares.Length; i++)
            {
                _board.GetSquares(pieces[i], squares[pieces[i] % squares.Length]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetSquares(byte[] pieces)
        {
            for (var i = 0; i < _squares.Length; i++)
            {
                _board.GetSquares(pieces[i], _squares[pieces[i] % _squares.Length]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MakeFirst(MoveBase move)
        {
            _moveHistoryService.AddFirst(move);

            move.Make(_board, _figureHistory);

            move.IsCheck = _turn != Turn.White ? _moveProvider.AnyBlackCheck() : _moveProvider.AnyWhiteCheck();

            _phase = _board.UpdatePhase();

            _moveHistoryService.Add(_board.GetKey());

            SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Make(MoveBase move)
        {
            _moveHistoryService.Add(move);

            move.Make(_board, _figureHistory);

            move.IsCheck = _turn != Turn.White ? _moveProvider.AnyBlackCheck() : _moveProvider.AnyWhiteCheck();

            _phase = _board.UpdatePhase();

            _moveHistoryService.Add(_board.GetKey());

            SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnMake()
        {
            _moveHistoryService.Remove(_board.GetKey());
            MoveBase move = _moveHistoryService.Remove();

            move.UnMake(_board, _figureHistory);

            move.IsCheck = false;

            _phase = _board.UpdatePhase();

            SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Do(MoveBase move)
        {
            move.Make(_board, _figureHistory);

            SwapTurn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnDo(MoveBase move)
        {
            move.UnMake(_board, _figureHistory);

            SwapTurn();
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
    }
}