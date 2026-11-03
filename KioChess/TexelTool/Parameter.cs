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
    public string MirrorJsonPath;
    public byte[] Phases;          // phases whose score this parameter can change; null = { Phase }
    public int LinearPiece = -1;   // white piece index (0..5) if the parameter is linear in the score, else -1
    public ulong WhiteMask;        // squares counted for the white piece
    public ulong BlackMask;        // squares counted for the black piece
    public byte[] PhaseSet => Phases ??= new[] { Phase };
    public Func<int, bool> IsValid;
}
