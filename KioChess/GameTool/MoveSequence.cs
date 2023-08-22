using Engine.Interfaces;
using System.Text;

public class MoveSequence
{
    public short M1 { get; set; }
    public short M2 { get; set; }
    public short M3 { get; set; }
    public short M4 { get; set; }

    internal string GetSequence(IMoveProvider moveProvider)
    {
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.Append(moveProvider.Get(M1).ToString()).Append(" ---> ")
            .Append(moveProvider.Get(M2).ToString()).Append(" ---> ")
            .Append(moveProvider.Get(M3).ToString()).Append(" ---> ")
            .Append(moveProvider.Get(M4));

        return stringBuilder.ToString();
    }
}
