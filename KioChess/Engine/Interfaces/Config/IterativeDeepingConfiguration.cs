namespace Engine.Interfaces.Config
{
    public class IterativeDeepingConfiguration
    {
        public int InitialDepth { get; set; }
        public int DepthStep { get; set; }
        public string[] Strategies { get; set; }
    }
}