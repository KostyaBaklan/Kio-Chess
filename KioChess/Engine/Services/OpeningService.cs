using System.Text;
using Engine.Interfaces;

namespace Engine.Services;

public class OpeningService: IOpeningService
{
    private readonly Dictionary<string, ICollection<string>> _sequences;
    private IMoveProvider _moveProvider;

    public OpeningService(IMoveProvider moveProvider)
    {
        _moveProvider = moveProvider;
        Dictionary<string, ICollection<string>> sequences = new Dictionary<string, ICollection<string>>();

        List<string> keyContainer = new List<string>();

        foreach (var keys in File.ReadLines(Path.Combine("Moves", "temp.txt"))
            .Select(line => line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)))
        {
            for (var i = 0; i < keys.Length - 1; i++)
            {
                keyContainer.Add(keys[i]);
                string key = CreateKey(keyContainer);

                string sequence = keys[i + 1];

                if (sequences.TryGetValue(key, out var list))
                {
                    list.Add(sequence);
                }
                else
                {
                    sequences[key] = new HashSet<string> { sequence };
                }
            }
            keyContainer.Clear();
        }

        _sequences = sequences;
    }

    private string CreateKey(ICollection<string> keys)
    {
        StringBuilder builder = new StringBuilder();

        foreach (var s in keys.Take(keys.Count - 1))
        {
            builder.Append($"{s},");
        }
        builder.Append($"{keys.Last()}");

        return builder.ToString();
    }

    #region Implementation of IOpeningService

    public IDictionary<string, ICollection<string>> GetSequences()
    {
        return _sequences;
    }

    public IEnumerable<ICollection<short>> GetMoveKeys()
    {
        foreach (var key in _sequences.Keys)
        {
            yield return key.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(short.Parse).ToList();
        }
    }

    #endregion
}
