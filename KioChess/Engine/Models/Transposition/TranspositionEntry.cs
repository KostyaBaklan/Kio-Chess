using System.Runtime.InteropServices;

namespace Engine.Models.Transposition;

[StructLayout(LayoutKind.Sequential)]
public struct TranspositionEntry
{
    public short Value { get; set; }
    public sbyte Depth { get; set; }
    //public TranspositionEntryType Type { get; set; }
    public short PvMove { get; set; }
}
