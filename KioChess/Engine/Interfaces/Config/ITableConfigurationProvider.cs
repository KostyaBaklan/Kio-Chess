namespace Engine.Interfaces.Config
{
    public interface ITableConfigurationProvider
    {
        int[] GetValues(int depth);
    }
}