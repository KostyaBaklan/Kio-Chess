using System.Text;
using Engine.Models.Helpers;

namespace Engine.Models.Config;

public class StaticTableCollection
{
    public PieceStaticTable[] Values { get; set; }

    public StaticTableCollection()
    {
        Values = new PieceStaticTable[12];
    }

    public void Add(byte piece, PieceStaticTable table) => Values[piece] = table;

    #region Overrides of Object

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        for (byte i = 0; i < Values.Length; i++)
        {
            builder.AppendLine(i.AsString());
            builder.AppendLine(Values[i].ToString());
            builder.AppendLine();
        }
        return builder.ToString();
    }

    #endregion
}