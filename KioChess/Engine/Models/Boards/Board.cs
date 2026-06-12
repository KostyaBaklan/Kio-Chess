using Engine.Models.Helpers;
using System.Runtime.CompilerServices;
using System.Text;

namespace Engine.Models.Boards;

[SkipLocalsInit]
public partial class Board
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Round(int value) => value + _round[value % 10];

    public override string ToString()
    {
        char[] pieceUnicodeChar =
        {
            '\u2659', '\u2658', '\u2657', '\u2656', '\u2655', '\u2654',
            '\u265F', '\u265E', '\u265D', '\u265C', '\u265B', '\u265A', ' '
        };
        var piecesNames = pieceUnicodeChar.Select(c => c.ToString()).ToArray();

        StringBuilder builder = new();
        for (short y = 7; y >= 0; y--)
        {
            for (byte x = 0; x < 8; x++)
            {
                byte i = (byte)(y * 8 + x);
                string v = piecesNames.Last();
                if (!Empty.IsSet(i.AsBitBoard()))
                {
                    v = piecesNames[_pieces[i]];
                }

                builder.Append($"[ {v} ]");
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }
}
