#if NULL_STATS
namespace Engine.Strategies.Base;

public static class NullStats
{
    public static long Nodes;            // all SearchWhite/SearchBlack entries
    public static long NullNodes;        // nodes visited while inside a null search
    public static long Tries;
    public static long FailHighs;
    public static readonly long[] TriesByDepth = new long[128];
    public static readonly long[] FailHighsByDepth = new long[128];
    public static int Nesting;           // > 0 while inside a null search

    public static void Reset()
    {
        Nodes = NullNodes = Tries = FailHighs = 0; Nesting = 0;
        Array.Clear(TriesByDepth); Array.Clear(FailHighsByDepth);
    }

    public static string Report()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Nodes={Nodes} NullNodes={NullNodes} ({(Nodes == 0 ? 0 : 100.0 * NullNodes / Nodes):F1}%) Tries={Tries} FailHighs={FailHighs} ({(Tries == 0 ? 0 : 100.0 * FailHighs / Tries):F1}%)");
        for (int d = 0; d < TriesByDepth.Length; d++)
            if (TriesByDepth[d] > 0)
                sb.AppendLine($"  depth {d}: tries={TriesByDepth[d]} fh={FailHighsByDepth[d]} ({100.0 * FailHighsByDepth[d] / TriesByDepth[d]:F1}%)");
        return sb.ToString();
    }
}
#endif
