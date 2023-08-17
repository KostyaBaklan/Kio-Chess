using Engine.Tools;

public class SortingItem
{
    public SortingItem(string key, PerformanceItem performanceItem)
    {
        var split = key.Split('_');
        Name= split[0];
        BeforeKiller = int.Parse(split[1]);
        AfterKiller= int.Parse(split[2]);

        PerformanceItem = performanceItem;
    }
    public string Name { get;  }
    public int BeforeKiller { get;  }
    public int AfterKiller { get; }
    public PerformanceItem PerformanceItem { get; }

    public override string ToString()
    { 
        return $"B={BeforeKiller} A={AfterKiller}";
    }
}
