namespace Engine.Models.Transposition
{
    public struct TranspositionEntry
    {
        public short Value { get; set; }
        public byte Depth { get; set; }
        //public TranspositionEntryType Type { get; set; }
        public short PvMove { get; set; }
    }
}
