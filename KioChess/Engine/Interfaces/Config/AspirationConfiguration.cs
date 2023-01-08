namespace Engine.Interfaces.Config
{
    public class AspirationConfiguration
    {
        public int[] AspirationWindow { get; set; }
        public int AspirationDepth { get; set; }
        public int AspirationMinDepth { get; set; }
        public int[] AspirationIterations { get; set; }
    }
}