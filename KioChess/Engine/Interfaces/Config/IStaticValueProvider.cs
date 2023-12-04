namespace Engine.Interfaces.Config;

public interface IStaticValueProvider
{
    int GetValue(byte piece, byte phase, byte square);
}