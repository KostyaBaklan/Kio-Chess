namespace Engine.Interfaces
{
    public interface IKillerMoveCollectionFactory
    {
        IKillerMoveCollection Create();

        IKillerMoveCollection[] CreateMoves();
    }
}