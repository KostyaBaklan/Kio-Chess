using System.Text;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Models.Config
{
    public class PhaseStaticTable
    {
        public byte Phase { get; set; }

        public Dictionary<string, short> Values { get; set; }

        public PhaseStaticTable(byte phase)
        {
            Phase = phase;
            Values = new Dictionary<string, short>(64);
        }

        public void AddValue(string square, short value)
        {
            Values.Add(square, value);
        }

        #region Overrides of Object

        public override string ToString()
        {
            string ranks = "ABCDEFGH";
            string files = "12345678";
            StringBuilder builder = new StringBuilder();
            for (int y = 7; y >= 0; y--)
            {
                builder.Append($"{files[y]}  ");
                for (int x = 0; x < 8; x++)
                {
                    byte i = (byte)(y * 8 + x);
                    var k = i.AsString();

                    builder.Append($"[ {Values[k]} ]");
                }

                builder.AppendLine();
            }

            builder.Append("    ");
            for (int i = 0; i < 8; i++)
            {
                builder.Append($"  {ranks[i]}  ");
            }

            return builder.ToString();
        }

        #endregion
    }
}