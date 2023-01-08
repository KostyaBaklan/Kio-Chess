namespace Engine.Interfaces.Config
{
    public interface IPieceEvaluation
    {
        short Pawn { get; }
        short Knight { get; }
        short Bishop { get; }
        short Rook { get; }
        short Queen { get; }
        short King { get; }
    }
}