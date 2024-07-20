using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Interfaces.Config;

namespace Engine.Services;

public class KillerMoveCollectionFactory : IKillerMoveCollectionFactory
{
    private readonly int _gameDepth;

    public KillerMoveCollectionFactory(IConfigurationProvider configurationProvider)
    {
        _gameDepth = configurationProvider.GeneralConfiguration.GameDepth;
    }

    #region Implementation of IKillerMoveCollectionFactory

    public KillerMoves[] CreateMoves()
    {
        var _moves = new KillerMoves[_gameDepth];
        for (var i = 0; i < _moves.Length; i++)
        {
            _moves[i] = new KillerMoves();
        }

        return _moves;
    }

    #endregion
}