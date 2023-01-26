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
            for (int i = 0; i < _searchContexts.Length; i++)
            {
                _searchContexts[i] = new SearchContext { Ply = i };
                _moveLists[i] = new MoveList();
                _sortContexts[0][0][i] = new WhiteOpeningSortContext();
                _sortContexts[0][1][i] = new WhiteMiddleSortContext();
                _sortContexts[0][2][i] = new WhiteEndSortContext();
                _sortContexts[1][0][i] = new BlackOpeningSortContext();
                _sortContexts[1][1][i] = new BlackMiddleSortContext();
                _sortContexts[1][2][i] = new BlackEndSortContext();
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
            return _sortContexts[(byte)_position.GetTurn()][(byte)_position.GetPhase()][_moveHistory.GetPly()];
        }

        public void Initialize(IPosition position)
        {
            _position = position;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public T[] Get<T>(int count)
        //{
        //   return ArrayPool<T>.Shared.Rent(count);
        //}


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public void Return<T>(T[] data)
        //{
        //    ArrayPool<T>.Shared.Return(data);
        //}
    }
}
