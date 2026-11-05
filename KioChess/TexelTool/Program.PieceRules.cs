using Engine.Models.Config;

namespace TexelTool;

internal static partial class Program
{
    // Rules apply to the effective piece value (piece value + mean of its PST), which is what Recentre folds into the piece value.
    // A move is allowed if the rules hold afterwards, or if it reduces an existing violation.

    private static double EffectivePiece(byte phase, int piece)
    {
        var pieces = (PieceEvaluation)_configuration.Evaluation.GetPiece(phase);
        int raw = piece switch { 1 => pieces.Knight, 2 => pieces.Bishop, 3 => pieces.Rook, _ => pieces.Queen };
        return raw + _collection.Values[piece].Values[phase].Values.Values.Average(v => (double)v);
    }

    private static double[] EffectiveSet(byte phase) =>
    [
        EffectivePiece(phase, 1), EffectivePiece(phase, 2), EffectivePiece(phase, 3), EffectivePiece(phase, 4)
    ];

    private static double Shortfall(double need) => need > 0 ? need : 0;

    private static double Outside(double value, ValueRange range) => Shortfall(range.Min - value) + Shortfall(value - range.Max);

    private static double PieceRuleViolation(byte phase, double[] e)
    {
        var pieces = (PieceEvaluation)_configuration.Evaluation.GetPiece(phase);
        var ranges = _cfg.Bounds.PieceRanges;
        double n = e[0], b = e[1], r = e[2], q = e[3], p = pieces.Pawn;
        double minor = Math.Max(n, b);

        return Outside(n, ranges.Knight) + Outside(b, ranges.Bishop) + Outside(r, ranges.Rook) + Outside(q, ranges.Queen)
            + Shortfall(r - (n + b) + 1)                           // N + B > R
            + Shortfall(minor + p - r + 1)                         // R > minor + P
            + Shortfall(r + minor - q + 1)                         // Q > R + minor
            + Shortfall(q - (r + n + b) + 1)                       // Q < R + N + B
            + Shortfall(Math.Abs(n - b) - _cfg.Bounds.PieceRules.MaxMinorDifference)
            + Shortfall(n + Math.Clamp(_cfg.Bounds.PieceRules.MinBishopOverKnight, 0, 25) - b);   // B >= N + k
    }

    private static bool PieceRuleAllows(byte phase, int piece, double delta)
    {
        var e = EffectiveSet(phase);
        double before = PieceRuleViolation(phase, e);
        e[piece - 1] += delta;
        double after = PieceRuleViolation(phase, e);
        return after == 0 || after < before;
    }

    private static void ApplyPieceRules(List<Parameter> list)
    {
        if (!_cfg.Bounds.PieceRules.Enabled) return;

        foreach (var p in list)
        {
            if (p.LinearPiece < 1 || p.LinearPiece > 4) continue;

            byte phase = p.Phase;
            int piece = p.LinearPiece;
            if (p.WhiteMask == ulong.MaxValue)
            {
                p.IsValid = candidate => PieceRuleAllows(phase, piece, candidate - p.Get());
            }
            else
            {
                int count = _collection.Values[piece].Values[phase].Values.Count;
                p.IsValid = candidate => PieceRuleAllows(phase, piece, (candidate - p.Get()) / (double)count);
            }
        }
    }

    // Per parameter index: which effective value it shifts and by how much per unit.
    private static (byte Phase, int Piece, double Weight)[] BuildPieceRuleMeta(List<Parameter> parameters)
    {
        var meta = new (byte, int, double)[parameters.Count];
        for (int i = 0; i < meta.Length; i++)
        {
            var p = parameters[i];
            if (p.LinearPiece < 1 || p.LinearPiece > 4) continue;
            double weight = p.WhiteMask == ulong.MaxValue ? 1.0 : 1.0 / _collection.Values[p.LinearPiece].Values[p.Phase].Values.Count;
            meta[i] = (p.Phase, p.LinearPiece, weight);
        }
        return meta;
    }

    // Used when merging worker results; the engine state must equal current.
    private static bool CandidateKeepsPieceRules(int[] candidate, int[] current, (byte Phase, int Piece, double Weight)[] meta)
    {
        if (!_cfg.Bounds.PieceRules.Enabled) return true;

        for (byte phase = 0; phase < 2; phase++)
        {
            var e = EffectiveSet(phase);
            double before = PieceRuleViolation(phase, e);
            for (int i = 0; i < meta.Length; i++)
            {
                if (meta[i].Piece == 0 || meta[i].Phase != phase || candidate[i] == current[i]) continue;
                e[meta[i].Piece - 1] += (candidate[i] - current[i]) * meta[i].Weight;
            }
            double after = PieceRuleViolation(phase, e);
            if (after != 0 && after > before) return false;
        }
        return true;
    }

    private static void ReportPieceRules(string label)
    {
        if (!_cfg.Bounds.PieceRules.Enabled) return;

        for (byte phase = 0; phase < 2; phase++)
        {
            var e = EffectiveSet(phase);
            Console.WriteLine($"[Rules] {label} {PhaseNames[phase]}: N {e[0]:F0}, B {e[1]:F0}, R {e[2]:F0}, Q {e[3]:F0}, violation {PieceRuleViolation(phase, e):F1}");
        }
    }
}
