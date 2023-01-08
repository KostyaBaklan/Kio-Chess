using Engine.Interfaces.Config;

namespace Engine.Models.Config
{
    public class PieceEvaluation: IPieceEvaluation
    {
        #region Implementation of IPieceEvaluation

        public short Pawn { get; set; }
        public short Knight { get; set; }
        public short Bishop { get; set; }
        public short Rook { get; set; }
        public short Queen { get; set; }
        public short King { get; set; }

        #endregion
    }
}