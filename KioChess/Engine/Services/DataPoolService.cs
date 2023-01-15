using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Strategies.Models;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Engine.Services
{
    public class DataPoolService : IDataPoolService
    {
        private readonly MoveList[] _moveLists;
        private readonly SearchContext[] _searchContexts;
        private readonly IMoveHistoryService _moveHistory;

        public DataPoolService(IMoveHistoryService moveHistory, IConfigurationProvider configuration)
        {
            _searchContexts = new SearchContext[configuration.GeneralConfiguration.GameDepth];
            _moveLists = new MoveList[configuration.GeneralConfiguration.GameDepth];
            for (int i = 0; i < _searchContexts.Length; i++)
            {
                _searchContexts[i] = new SearchContext { Ply = i };
                _moveLists[i] = new MoveList();
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
