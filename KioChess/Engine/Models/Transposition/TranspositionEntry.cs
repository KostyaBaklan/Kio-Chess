using System.Runtime.InteropServices;

namespace Engine.Models.Transposition;

[StructLayout(LayoutKind.Sequential)]
public struct TranspositionEntry
{
    public short Value;
    public short PvMove;
    public sbyte Depth;
    public TranspositionEntryType Type;

    public override string ToString()
    {
        return $"V={Value},D={Depth},T={Type},P={PvMove}";
    }
}