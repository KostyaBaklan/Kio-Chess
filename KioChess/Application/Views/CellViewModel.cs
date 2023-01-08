using Engine.Models.Boards;
using Engine.Models.Enums;
using Kgb.ChessApp.Models;
using Prism.Mvvm;

namespace Kgb.ChessApp.Views
{
    public class CellViewModel : BindableBase
    {
        public CellViewModel()
        {
            _state = State.Idle;
        }

        private State _state;

        public State State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        private CellType _cellType;

        public CellType CellType
        {
            get => _cellType;
            set => SetProperty(ref _cellType, value);
        }

        private Piece? _figure;

        public Piece? Figure
        {
            get => _figure;
            set => SetProperty(ref _figure, value);
        }

        public Square Cell { get; set; }
    }
}
