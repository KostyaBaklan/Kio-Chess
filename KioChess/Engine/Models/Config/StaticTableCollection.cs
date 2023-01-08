using System.Text;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Models.Config
{
    public class StaticTableCollection
    {
        public PieceStaticTable[] Values { get; set; }

        public StaticTableCollection()
        {
            Values = new PieceStaticTable[12];
        }

        public void Add(Piece piece, PieceStaticTable table)
        {
            Values[piece.AsByte()] = table;
        }

        #region Overrides of Object

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (var i = 0; i < Values.Length; i++)
            {
                builder.AppendLine(((Piece)i).ToString());
                builder.AppendLine(Values[i].ToString());
                builder.AppendLine();
            }
            return builder.ToString();
        }

        #endregion
    }
}