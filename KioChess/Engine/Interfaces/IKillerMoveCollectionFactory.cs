using Engine.DataStructures.Moves;

namespace Engine.Interfaces;

public interface IKillerMoveCollectionFactory
{
    KillerMoves[] CreateMoves();
}