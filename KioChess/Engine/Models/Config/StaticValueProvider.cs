using Engine.Interfaces.Config;
using Engine.Models.Helpers;

namespace Engine.Models.Config;

public class StaticValueProvider: IStaticValueProvider
{
    private readonly StaticTableCollection _collection;

    public StaticValueProvider(StaticTableCollection collection)
    {
        _collection = collection;
    }

    #region Implementation of IStaticValueProvider

    public int GetValue(byte piece, byte phase, byte square) => GetValue(piece, phase, square.AsString());

    private int GetValue(byte piece, byte phase, string square) => _collection.Values[piece].Values[phase].Values[square];

    #endregion

    #region Overrides of Object

    public override string ToString() => _collection.ToString();

    #endregion
}