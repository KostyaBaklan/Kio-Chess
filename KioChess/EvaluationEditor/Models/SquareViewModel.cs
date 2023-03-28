using Engine.Models.Boards;
using Engine.Models.Helpers;
using Prism.Mvvm;

namespace EvaluationEditor.Models
{
    public class SquareViewModel:BindableBase
    {
        public SquareViewModel(Square square, short value, CellType cellType)
        {
            Name = square.AsString();
            Square = square;
            Value = value;
            CellType = cellType;
        }

        private CellType _cellType;

        public CellType CellType
        {
            get => _cellType;
            set => SetProperty(ref _cellType, value);
        }

        public Square Square { get; }

        private short _value;

        public short Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string Name { get; }
    }
}