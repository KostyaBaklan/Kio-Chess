using System;
using System.Collections.Generic;

namespace Chess.Helpers
{
    public static class MathExtensions
    {
        public static double StandardDeviation(this IEnumerable<double> samples)
        {
            return Math.Sqrt(Variance(samples));
        }

        public static double Variance(IEnumerable<double> samples)
        {
            double num = 0.0;
            double num2 = 0.0;
            ulong num3 = 0uL;
            using (IEnumerator<double> enumerator = samples.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    num3++;
                    num2 = enumerator.Current;
                }

                while (enumerator.MoveNext())
                {
                    num3++;
                    double current = enumerator.Current;
                    num2 += current;
                    double num4 = num3 * current - num2;
                    num += num4 * num4 / (num3 * (num3 - 1));
                }
            }

            if (num3 <= 1)
            {
                return double.NaN;
            }

            return num / (num3 - 1);
        }
    }
}
