namespace Engine.Interfaces.Config
{
    public class PvsConfiguration
    {
        public int NonPvIterations { get; set; }
        public int PvsMinDepth { get; set; }
        public int PvsDepthStep { get; set; }
        public int PvsDepthIterations { get; set; }
    }
}