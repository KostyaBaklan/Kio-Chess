using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Models.Config
{
    public class StaticValueProvider: IStaticValueProvider
    {
        private readonly StaticTableCollection _collection;

        public StaticValueProvider(StaticTableCollection collection)
        {
            _collection = collection;
        }

        #region Implementation of IStaticValueProvider

        public int GetValue(Piece piece, Phase phase, Square square)
        {
            return GetValue(piece.AsByte(), phase, square.AsString());
        }

        public int GetValue(byte piece, byte phase, byte square)
        {
            return GetValue(piece, (Phase)phase, new Square(square).AsString());
        }

        private int GetValue(byte piece, Phase phase, string square)
        {
            return _collection.Values[piece].Values[phase].Values[square];
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return _collection.ToString();
        }

        #endregion
    }
}