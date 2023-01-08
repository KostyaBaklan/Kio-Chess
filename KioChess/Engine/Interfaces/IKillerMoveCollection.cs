namespace Engine.Interfaces
{
    public interface IKillerMoveCollection
    {
        void Add(short move);
        bool Contains(short move);
    }
}
