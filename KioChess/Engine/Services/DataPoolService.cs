using Engine.Book.Interfaces;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Strategies.Models;
using System.Runtime.CompilerServices;

namespace Engine.Services
{
    public class DataPoolService : IDataPoolService
    {
        private readonly MoveList[] _moveLists;
        private readonly SearchContext[] _searchContexts;
        private readonly SortContext[][][] _sortContexts;
        private readonly IMoveHistoryService _moveHistory;
        private IPosition _position;

        public DataPoolService(IMoveHistoryService moveHistory, IConfigurationProvider configuration)
        {
            var UseBooking = !configuration.BookConfiguration.UseBooking;
            SortContext.SearchDepth = configuration.BookConfiguration.SearchDepth;
            _searchContexts = new SearchContext[configuration.GeneralConfiguration.GameDepth];
            _moveLists = new MoveList[configuration.GeneralConfiguration.GameDepth];
            _sortContexts = new SortContext[2][][];

            for (int i = 0; i < _sortContexts.Length; i++)
            {
                _sortContexts[i] = new SortContext[3][];
                for (int j = 0; j < _sortContexts[i].Length; j++)
                {
                    _sortContexts[i][j] = new SortContext[configuration.GeneralConfiguration.GameDepth];
                }
            }

            for (int i = 0; i < SortContext.SearchDepth; i++)
            {
                _searchContexts[i] = new SearchContext { Ply = i };
                _moveLists[i] = new MoveList();
                _sortContexts[0][0][i] = new WhiteOpeningSortContext { Ply = i, IsRegular = UseBooking };
                _sortContexts[0][1][i] = new WhiteMiddleSortContext { Ply = i, IsRegular = UseBooking };
                _sortContexts[0][2][i] = new WhiteEndSortContext { Ply = i, IsRegular = UseBooking };
                _sortContexts[1][0][i] = new BlackOpeningSortContext { Ply = i, IsRegular = UseBooking };
                _sortContexts[1][1][i] = new BlackMiddleSortContext { Ply = i, IsRegular = UseBooking };
                _sortContexts[1][2][i] = new BlackEndSortContext { Ply = i, IsRegular = UseBooking };
            }

            for (int i = SortContext.SearchDepth; i < _searchContexts.Length; i++)
            {
                _searchContexts[i] = new SearchContext { Ply = i };
                _moveLists[i] = new MoveList();
                _sortContexts[0][0][i] = new WhiteOpeningSortContext { Ply = i, IsRegular = true };
                _sortContexts[0][1][i] = new WhiteMiddleSortContext { Ply = i, IsRegular = true };
                _sortContexts[0][2][i] = new WhiteEndSortContext { Ply = i, IsRegular = true };
                _sortContexts[1][0][i] = new BlackOpeningSortContext { Ply = i, IsRegular = true };
                _sortContexts[1][1][i] = new BlackMiddleSortContext { Ply = i, IsRegular = true };
                _sortContexts[1][2][i] = new BlackEndSortContext { Ply = i, IsRegular = true };
            }

            _moveHistory = moveHistory;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SearchContext GetCurrentContext()
        {
            return _searchContexts[_moveHistory.GetPly()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveList GetCurrentMoveList()
        {
            return _moveLists[_moveHistory.GetPly()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SortContext GetCurrentSortContext()
        {
            return _sortContexts[(byte)_position.GetTurn()][_position.GetPhase()][_moveHistory.GetPly()];
        }

        public void Initialize(IPosition position, IBookService bookService, IMoveHistoryService moveHistoryService)
        {
            _position = position;

            SortContext.Position = position;
            SortContext.BookService = bookService;
            SortContext.MoveHistory = moveHistoryService;
        }
    }
}
