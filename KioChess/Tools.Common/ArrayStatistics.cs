
namespace Tools.Common
{
    public static  class ArrayStatistics
    {
        public static double Minimum(double[] data)
        {
            if (data.Length == 0)
            {
                return double.NaN;
            }

            double num = double.PositiveInfinity;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] < num || double.IsNaN(data[i]))
                {
                    num = data[i];
                }
            }

            return num;
        }

        public static double Maximum(double[] data)
        {
            if (data.Length == 0)
            {
                return double.NaN;
            }

            double num = double.NegativeInfinity;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] > num || double.IsNaN(data[i]))
                {
                    num = data[i];
                }
            }

            return num;
        }

        public static double Mean(double[] data)
        {
            if (data.Length == 0)
            {
                return double.NaN;
            }

            double num = 0.0;
            ulong num2 = 0uL;
            for (int i = 0; i < data.Length; i++)
            {
                num += (data[i] - num) / ++num2;
            }

            return num;
        }

        public static double Variance(double[] samples)
        {
            if (samples.Length <= 1)
            {
                return double.NaN;
            }

            double num = 0.0;
            double num2 = samples[0];
            for (int i = 1; i < samples.Length; i++)
            {
                num2 += samples[i];
                double num3 = (i + 1) * samples[i] - num2;
                num += num3 * num3 / ((i + 1.0) * i);
            }

            return num / (samples.Length - 1);
        }

        public static double StandardDeviation(double[] samples)
        {
            return Math.Sqrt(Variance(samples));
        }

        public static Tuple<double, double> MeanStandardDeviation(double[] samples)
        {
            return new Tuple<double, double>(Mean(samples), StandardDeviation(samples));
        }
    }
}
