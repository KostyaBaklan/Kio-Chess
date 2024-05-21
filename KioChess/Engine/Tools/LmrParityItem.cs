namespace Engine.Tools
{
    public class LmrParityItem
    {
        public int Depth { get; set; }
        public int Index { get; set; }
        public int Count { get; set; }

        public override string ToString()
        {
            return $"D={Depth} I={Index} C={Count}";
        }
    }
}
