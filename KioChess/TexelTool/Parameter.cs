namespace TexelTool;

internal sealed class Parameter
{
    public string Name;
    public byte Phase;
    public int Min;
    public int Max;
    public Func<int> Get;
    public Action<int> Set;
    public string JsonPath;
    public bool Global;
    public Func<int, bool> IsValid;
}
