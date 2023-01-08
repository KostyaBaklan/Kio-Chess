using Engine.DataStructures.Killers;
using Engine.Interfaces;
using Engine.Interfaces.Config;

namespace Engine.Services
{
    public class KillerMoveCollectionFactory : IKillerMoveCollectionFactory
    {
        private readonly int _killerCapacity;
        private readonly int _movesCount;
        private readonly int _gameDepth;
        private IKillerMoveCollection[] _moves;

        public KillerMoveCollectionFactory(IConfigurationProvider configurationProvider, IMoveProvider moveProvider)
        {
            _killerCapacity = configurationProvider.GeneralConfiguration.KillerCapacity;
            _gameDepth = configurationProvider.GeneralConfiguration.GameDepth;
            _movesCount = moveProvider.MovesCount;
        }

        #region Implementation of IKillerMoveCollectionFactory

        public IKillerMoveCollection Create()
        {
            if (_killerCapacity == 2)
                return new BiKillerMoves(_movesCount);
            return new TiKillerMoves(_movesCount);
        }

        public IKillerMoveCollection[] CreateMoves()
        {
            if (_moves != null) return _moves;

            _moves = new IKillerMoveCollection[_gameDepth];
            for (var i = 0; i < _moves.Length; i++)
            {
                _moves[i] = Create();
            }

            return _moves;
        }

        #endregion
    }
}