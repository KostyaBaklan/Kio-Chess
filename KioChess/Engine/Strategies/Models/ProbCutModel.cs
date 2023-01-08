namespace Engine.Strategies.Models
{
    public class ProbCutModel
    {
        public ProbCutModel(bool canReduce, double percentile, double sigma, double a, double b, int depth)
        {
            CanReduce = canReduce;
            Percentile = percentile;
            Sigma = sigma;
            A = a;
            B = b;
            Depth = depth;
        }

        public bool CanReduce;
        public double Percentile;
        public double Sigma;
        public double A;
        public double B;
        public int Depth;
    }
}