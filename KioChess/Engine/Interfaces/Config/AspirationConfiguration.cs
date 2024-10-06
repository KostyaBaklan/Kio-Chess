namespace Engine.Interfaces.Config;

public class AspirationConfiguration
{
    public int AspirationWindow { get; set; }
    public int AspirationDepth { get; set; }
    public sbyte[] AspirationMinDepth { get; set; }
    public string[] Strategies { get; set; }
}