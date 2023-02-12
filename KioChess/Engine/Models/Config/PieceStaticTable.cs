using System.Text;
using Engine.Models.Enums;

namespace Engine.Models.Config
{
    public class PieceStaticTable
    {
        public Piece Piece { get; set; }

        public Dictionary<Phase, PhaseStaticTable> Values { get; set; }

        public PieceStaticTable(Piece piece)
        {
            Piece = piece;
            Values = new Dictionary<Phase, PhaseStaticTable>();
        }

        public void AddPhase(Phase phase)
        {
            Values.Add(phase, new PhaseStaticTable(phase));
        }

        public void AddValue(Phase phase, string square, short value)
        {
            Values[phase].AddValue(square, value);
        }

        #region Overrides of Object

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var pair in Values)
            {
                builder.AppendLine(pair.Key.ToString());
                builder.AppendLine(pair.Value.ToString());
                builder.AppendLine();
            }
            return builder.ToString();
        }

        #endregion
    }
}