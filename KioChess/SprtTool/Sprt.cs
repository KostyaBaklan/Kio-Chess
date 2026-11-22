namespace SprtTool;

public enum SprtState { Continue, AcceptH1, AcceptH0 }

/// <summary>
/// SPRT on game pairs (pentanomial). Every opening is played twice with colours swapped,
/// so one pair gives the candidate a score of 0, 0.25, 0.5, 0.75 or 1.
/// ptn[i] = number of pairs with pair score i / 4 (index = candidate game points * 2).
/// </summary>
public static class Sprt
{
    private static readonly double[] PairScore = { 0.0, 0.25, 0.5, 0.75, 1.0 };

    public static double EloToScore(double elo) => 1.0 / (1.0 + Math.Pow(10.0, -elo / 400.0));
    public static double ScoreToElo(double score) => -400.0 * Math.Log10(1.0 / score - 1.0);

    private static (double mean, double variance, long pairs) Moments(long[] ptn)
    {
        long n = 0;
        double sum = 0;
        for (int i = 0; i < 5; i++) { n += ptn[i]; sum += ptn[i] * PairScore[i]; }
        if (n == 0) return (0.5, 0, 0);
        double mean = sum / n;
        double variance = 0;
        for (int i = 0; i < 5; i++) variance += ptn[i] * (PairScore[i] - mean) * (PairScore[i] - mean);
        return (mean, variance / n, n);
    }

    public static (double llr, double lower, double upper, SprtState state) Evaluate(
        long[] ptn, double elo0, double elo1, double alpha, double beta)
    {
        double lower = Math.Log(beta / (1 - alpha));      // accept H0 at or below
        double upper = Math.Log((1 - beta) / alpha);      // accept H1 at or above
        var (mean, variance, pairs) = Moments(ptn);
        if (pairs == 0 || variance < 1e-12) return (0, lower, upper, SprtState.Continue);

        double s0 = EloToScore(elo0), s1 = EloToScore(elo1);
        double llr = pairs * (s1 - s0) * (2 * mean - s0 - s1) / (2 * variance);

        var state = llr >= upper ? SprtState.AcceptH1
                  : llr <= lower ? SprtState.AcceptH0
                  : SprtState.Continue;
        return (llr, lower, upper, state);
    }

    /// <summary>Elo of the candidate with a 95% error bar.</summary>
    public static (double elo, double error95) Estimate(long[] ptn)
    {
        var (mean, variance, pairs) = Moments(ptn);
        if (pairs == 0 || mean <= 0 || mean >= 1) return (0, double.NaN);
        double se = Math.Sqrt(variance / pairs);
        double lo = Math.Clamp(mean - 1.96 * se, 1e-6, 1 - 1e-6);
        double hi = Math.Clamp(mean + 1.96 * se, 1e-6, 1 - 1e-6);
        return (ScoreToElo(mean), (ScoreToElo(hi) - ScoreToElo(lo)) / 2);
    }
}
