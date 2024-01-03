using CommonServiceLocator;
using Engine.Interfaces.Config;

namespace Engine.Sorting;

public static class Sort
{
    public static readonly byte[] SortAttackMinimum;
    public static readonly byte[] SortMinimum;

    static Sort()
    {
        SortingConfiguration sortConfiguration;
        try
        {
            sortConfiguration = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                    .AlgorithmConfiguration.SortingConfiguration;
        }
        catch (Exception)
        {
            sortConfiguration = new SortingConfiguration { SortMinimum = 10, SortMoveIndex = 41, SortHalfIndex = 11 };
        }

        SortMinimum = new byte[128];
        SortAttackMinimum = new byte[128];
        for (var i = 0; i < sortConfiguration.SortHalfIndex; i++)
        {
            SortMinimum[i] = (byte)Math.Min(GetSortCount(i), sortConfiguration.SortMinimum);
            SortAttackMinimum[i] = (byte)Math.Min(GetSortCount(i), sortConfiguration.SortMinimum);
        }
        for (var i = sortConfiguration.SortHalfIndex; i < sortConfiguration.SortMoveIndex; i++)
        {
            SortMinimum[i] = (byte)Math.Min(GetSortCount(i), sortConfiguration.SortMinimum);
            SortAttackMinimum[i] = (byte)Math.Min(GetSortCount(i), sortConfiguration.SortMinimum);
        }
        for (var i = sortConfiguration.SortMoveIndex; i < SortMinimum.Length; i++)
        {
            SortMinimum[i] = (byte)Math.Min(GetSortCount(i), sortConfiguration.SortMinimum + 1);
            SortAttackMinimum[i] = (byte)Math.Min(GetSortCount(i), sortConfiguration.SortMinimum + 1);
        }
    }

    private static int GetSortCount(int i, int factor = 0, int offset = 0)
    {
        if(i < 2)return 0;
        return i < 4 ? 1 : (i + factor) / 3 + offset;
    }
}
