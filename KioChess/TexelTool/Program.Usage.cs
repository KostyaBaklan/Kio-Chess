namespace TexelTool;

internal static partial class Program
{
    private static string UsagePath => Path.Combine(_cfg.OutputDirectory, "usage.txt");

    // For every parameter, find the phases where changing it changes at least one score.
    // Parameters with no such phase are dead and are removed. Result is saved so workers reuse it.
    private static void ProbeUsage(TexelDataSet data, List<Parameter> parameters)
    {
        _board.ResetEvaluationCaches();
        var baseline = new int[data.Count];
        for (int phase = 0; phase < 3; phase++) PhaseError(data, phase, 1.0, baseline);
        var lines = new List<string>();
        int dead = 0;
        foreach (var p in parameters)
        {
            int original = p.Get();
            int up = Math.Min(original + 8, p.Max);
            int down = Math.Max(original - 8, p.Min);
            var used = new List<byte>();
            foreach (var phase in p.PhaseSet)
            {
                if ((up != original && ChangesAnyScore(data, p, phase, up, baseline))
                    || (down != original && ChangesAnyScore(data, p, phase, down, baseline)))
                    used.Add(phase);
            }
            ForceApply(p, original);
            lines.Add(p.Name + "\t" + string.Join(",", used));
            if (used.Count == 0) { dead++; Console.WriteLine($"[Usage] dead: {p.Name}"); }
        }
        File.WriteAllLines(UsagePath, lines);
        Console.WriteLine($"[Usage] probed {parameters.Count} parameters, dead {dead}");
    }

    private static bool ChangesAnyScore(TexelDataSet data, Parameter p, byte phase, int value, int[] baseline)
    {
        ForceApply(p, value);
        foreach (int i in data.PhaseIndices[phase])
        {
            if (ScoreOne(data, i) != baseline[i]) return true;
        }
        return false;
    }

    // Applies to all three phases, ignores IsValid: measurement only.
    private static void ForceApply(Parameter p, int value)
    {
        p.Set(value);
        for (byte phase = 0; phase < 3; phase++) _factory.GetEvaluationService(phase).Reload(_configuration, _staticValueProvider);
        if (p.Name.Contains("Shield")) _board.RebuildKingShieldLookup();
        for (byte phase = 0; phase < 3; phase++) _board.ResetPawnHash(phase);
    }

    // Applies usage.txt: sets Phases and removes dead parameters. Identical in main process and workers.
    private static void ApplyUsage(List<Parameter> parameters)
    {
        if (!File.Exists(UsagePath)) return;
        var map = new Dictionary<string, byte[]>();
        foreach (var line in File.ReadAllLines(UsagePath))
        {
            var parts = line.Split('\t');
            map[parts[0]] = parts.Length < 2 || parts[1].Length == 0
                ? Array.Empty<byte>()
                : parts[1].Split(',').Select(byte.Parse).ToArray();
        }
        parameters.RemoveAll(p => map.TryGetValue(p.Name, out var ph) && ph.Length == 0);
        foreach (var p in parameters)
        {
            if (map.TryGetValue(p.Name, out var ph)) p.Phases = ph;
        }
    }
}
