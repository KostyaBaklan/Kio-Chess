using Engine.DataStructures.Hash;
using Engine.Interfaces;

namespace Engine.Services
{
    public class TranspositionTableService : ITranspositionTableService
    {
        public TranspositionTable Create(short depth)
        {
            int factor = GetFactor(depth);
            int capacity = nextPrime(factor);

            return new TranspositionTable(capacity, depth);
        }

        private int GetFactor(short depth)
        {
            int d = depth - 5;
            double x = 1;
            double step = 0.125;
            double start = 2.25;
            double min = 1.06125;
            int k = 0;
            for (int i = 0; i < d; i++)
            {
                k += 500000;
                if(i > 10)
                {
                    k += 500000;
                }
                start = start - step;
                x = x * Math.Max(min,start);
                if(i == 9)
                {
                    step = step / 25;
                }
            }

            return (int)(x * 1000000)+k;
        }

        // Function that returns true if n
        // is prime else returns false
        static bool isPrime(int n)
        {
            // Corner cases
            if (n <= 1) return false;
            if (n <= 3) return true;

            // This is checked so that we can skip
            // middle five numbers in below loop
            if (n % 2 == 0 || n % 3 == 0)
                return false;

            for (int i = 5; i * i <= n; i = i + 6)
                if (n % i == 0 ||
                    n % (i + 2) == 0)
                    return false;

            return true;
        }

        // Function to return the smallest
        // prime number greater than N
        static int nextPrime(int N)
        {

            // Base case
            if (N <= 1)
                return 2;

            int prime = N;
            bool found = false;

            // Loop continuously until isPrime
            // returns true for a number
            // greater than n
            while (!found)
            {
                prime++;

                if (isPrime(prime))
                    found = true;
            }
            return prime;
        }
    }
}
