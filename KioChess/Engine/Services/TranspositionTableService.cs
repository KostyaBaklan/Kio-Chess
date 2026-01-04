using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;

namespace Engine.Services;

public class TranspositionTableService : ITranspositionTableService
{
    public TranspositionTable Create(int depth, Board board)
    {
        int capacity;
        switch (depth)
        {
            case > 12:
                capacity = 512;
                break;
            case 12:
                capacity = 256;
                break;
            case 11:
            case 10:
                capacity = 128;
                break;
            case 9:
            case 8:
                capacity = 64;
                break;
            default:
                capacity = 32;
                break;
        }

        return new TranspositionTable(capacity, board);
    }

    public int GetFactor(int depth, int coef)
    {
        int d = depth - 4;
        double x = 1;
        double step = 0.075;
        double start = 2;
        double min = 1.1;
        double k = 3;
        for (int i = 0; i < d; i++)
        {
            start = start - step;
            x = x * Math.Max(min, start);
            k += 0.0125;
        }

        return (int)(k * x * coef);
    }

    public int GetFactor(int depth)
    {
        int d = depth - 4;
        double x = 1;
        double step = 0.075;
        double start = 2;
        double min = 1.1;
        double k = 3;
        for (int i = 0; i < d; i++)
        {
            start = start - step;
            x = x * Math.Max(min, start);
            k += 0.0125;
        }

        return (int)(k * x * 200000);
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
    public int NextPrime(int number)
    {

        // Base case
        if (number <= 1)
            return 2;

        int prime = number;
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
