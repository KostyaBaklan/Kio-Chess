using Engine.Models.Helpers;
using System.Numerics;
using TexelTool.Models;

namespace TexelTool
{
    internal static partial class Program
    {
        private static int[] _scores;    // current score of every position (same perspective as EvaluateUncached)
        private static int[] _scratch;   // scores written by the last full-evaluation trial
        private static readonly double[] _trialErrors = new double[2];
        private static bool _useIncremental;
        private static readonly List<(int Index, int Coef)> _touched = new();
        private static readonly Dictionary<string, byte> SquareIndex = BuildSquareIndex();

        private static Dictionary<string, byte> BuildSquareIndex()
        {
            var map = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
            for (byte i = 0; i < 64; i++) map[i.AsString()] = i;
            return map;
        }

        // Fills _scores and returns per-phase error for the current parameter values.
        private static double[] InitScores(TexelDataSet data, double k)
        {
            _scores ??= new int[data.Count];
            _scratch ??= new int[data.Count];
            var phaseError = new double[2];
            for (int phase = 0; phase < 2; phase++)
                phaseError[phase] = PhaseError(data, phase, k, _scores);
            return phaseError;
        }

        private static void CopyScores(TexelDataSet data, int phase, int[] from, int[] to)
        {
            foreach (int i in data.PhaseIndices[phase]) to[i] = from[i];
        }

        // Error change if the linear parameter changes by d. Fills _touched with (index, coefficient).
        private static double LinearDelta(TexelDataSet data, Parameter p, int d, double k)
        {
            var pieces = data.Pieces;
            var results = data.Results;
            int w = p.LinearPiece, b = p.LinearPiece + 6;
            double delta = 0;
            _touched.Clear();
            foreach (int i in data.PhaseIndices[p.Phase])
            {
                int c = BitOperations.PopCount(pieces[i * 12 + w] & p.WhiteMask)
                      - BitOperations.PopCount(pieces[i * 12 + b] & p.BlackMask);
                if (c == 0) continue;
                _touched.Add((i, c));
                double r = results[i];
                double e0 = r - Sigmoid(_scores[i], k);
                double e1 = r - Sigmoid(_scores[i] + c * d, k);
                delta += e1 * e1 - e0 * e0;
            }
            return delta;
        }

        private static bool TryLinear(Parameter p, int candidate, TexelDataSet data, double k, double[] phaseError)
        {
            int d = candidate - p.Get();
            if (d == 0) return false;
            double delta = LinearDelta(data, p, d, k);
            if (delta >= 0) return false;            // rejected: engine state untouched
            Apply(p, candidate);
            foreach (var (i, c) in _touched) _scores[i] += c * d;
            phaseError[p.Phase] += delta;
            return true;
        }

        // Startup self-test: incremental scores must equal full evaluation exactly.
        private static void VerifyIncremental(TexelDataSet data, List<Parameter> parameters, double k)
        {
            _useIncremental = false;
            if (!_cfg.UseIncrementalLinear) { Console.WriteLine("[Incremental] disabled by config"); return; }
            _board.ResetEvaluationCaches();
            InitScores(data, k);
            var linear = parameters.Where(p => p.LinearPiece >= 0 && !p.NonLinear).ToList();
            if (linear.Count == 0) { _cfg.UseIncrementalLinear = false; return; }
            int samples = Math.Min(40, linear.Count), stride = Math.Max(1, linear.Count / samples);
            long mismatches = 0, checkedPositions = 0;
            for (int s = 0; s < linear.Count; s += stride)
            {
                var p = linear[s];
                int original = p.Get();
                int d = original + 8 <= p.Max ? 8 : -8;
                LinearDelta(data, p, d, k);
                var coef = new Dictionary<int, int>();
                foreach (var (i, c) in _touched) coef[i] = c;
                Apply(p, original + d);
                PhaseError(data, p.Phase, k, _scratch);
                foreach (int i in data.PhaseIndices[p.Phase])
                {
                    checkedPositions++;
                    int expected = _scores[i] + coef.GetValueOrDefault(i) * d;
                    if (_scratch[i] != expected)
                    {
                        if (mismatches < 10) Console.WriteLine($"[Incremental] mismatch {p.Name} pos {i}: full {_scratch[i]}, incremental {expected}");
                        mismatches++;
                    }
                }
                Apply(p, original);
            }
            _useIncremental = mismatches == 0;
            if (!_useIncremental) _cfg.UseIncrementalLinear = false;
            Console.WriteLine($"[Incremental] self-test: {mismatches} mismatches in {checkedPositions} checks -> fast path {(_useIncremental ? "ON" : "OFF")}");
        }

        // Recomputes all scores and reports drift between incremental and full evaluation.
        private static void Resync(TexelDataSet data, double k, double[] phaseError)
        {
            long drift = 0;
            for (int phase = 0; phase < 2; phase++)
            {
                phaseError[phase] = PhaseError(data, phase, k, _scratch);
                foreach (int i in data.PhaseIndices[phase])
                {
                    if (_scratch[i] != _scores[i]) drift++;
                    _scores[i] = _scratch[i];
                }
            }
            if (drift > 0) Console.WriteLine($"WARN\t[Incremental] drift in {drift} positions, resynced");
        }
    }
}
