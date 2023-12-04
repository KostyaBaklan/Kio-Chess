using Newtonsoft.Json;

namespace Engine.Tools;

public static class MoveGenerationPerformance
{
    private static Dictionary<string, PerformanceItem> _items = new Dictionary<string, PerformanceItem>();

    public static void Add(string name, TimeSpan time)
    {
        if (_items.TryGetValue(name, out PerformanceItem item))
        {
            item.Time += time;
            item.Count++;
        }
        else
        {
            _items.Add(name, new PerformanceItem { Count = 1, Time = time });
        }
    }

    public static void Save()
    {
        foreach (var item in _items.Values)
        {
            item.Calculate();
        }

        var json = JsonConvert.SerializeObject(_items, Formatting.Indented);

        File.WriteAllText($"{nameof(MoveGenerationPerformance)}.json", json);
    }
}
