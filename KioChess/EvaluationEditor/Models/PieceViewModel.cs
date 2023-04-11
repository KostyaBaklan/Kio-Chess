using System.Collections.ObjectModel;
using Engine.Interfaces.Config;
using Engine.Models.Config;
using Engine.Models.Enums;
using Prism.Mvvm;

namespace EvaluationEditor.Models
{
    public class PieceViewModel:BindableBase
    {
        public PieceViewModel(IStaticValueProvider valueProvider, Piece piece)
        {
            Piece = piece;
            Name = piece.ToString();
            Phases = new ObservableCollection<PhaseViewModel>();
            foreach (var phase in new byte[] {Phase.Opening,Phase.Middle,Phase.End})
            {
                var pieceViewModel = new PhaseViewModel(valueProvider, piece, phase);
                Phases.Add(pieceViewModel);
            }
        }

        public string Name { get; }

        private int _index;

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public ObservableCollection<PhaseViewModel> Phases { get; }
        public Piece Piece { get; }

        public PieceStaticTable ToTable()
        {
            var pieceStaticTable = new PieceStaticTable(Piece);
            for (var i = 0; i < Phases.Count; i++)
            {
                pieceStaticTable.Values[Phases[i].Phase] = Phases[i].ToTable();
            }
            return pieceStaticTable;
        }

        public PhaseViewModel GetSelectedPhase()
        {
            return Phases[Index];
        }
    }
}