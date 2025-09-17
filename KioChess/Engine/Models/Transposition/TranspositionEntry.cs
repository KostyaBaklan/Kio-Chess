using System.Runtime.InteropServices;

namespace Engine.Models.Transposition;

[StructLayout(LayoutKind.Sequential)]
public struct TranspositionEntry
{
    public short Value;
    public sbyte Depth;
    public TranspositionEntryType Type;
    public short PvMove;
}