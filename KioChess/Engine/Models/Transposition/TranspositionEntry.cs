using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Models.Transposition;

[StructLayout(LayoutKind.Sequential)]
public struct TranspositionEntry
{
    public short Value = 0;
    public short PvMove = -1;
    public sbyte Depth = 0;
    public TranspositionEntryType Type = TranspositionEntryType.Exact;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TranspositionEntry(sbyte depth, short value, short pvMove,  TranspositionEntryType type)
    {
        Value = value;
        PvMove = pvMove;
        Depth = depth;
        Type = type;
    }

    public override string ToString()
    {
        return $"V={Value},D={Depth},T={Type},P={PvMove}";
    }
}