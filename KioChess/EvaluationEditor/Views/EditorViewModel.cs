using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Engine.Interfaces.Config;
using Engine.Models.Config;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using EvaluationEditor.Models;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;

namespace EvaluationEditor.Views
{
    public class EditorViewModel:BindableBase
    {
        public EditorViewModel(IStaticValueProvider valueProvider)
        {
            Pieces = new ObservableCollection<PieceViewModel>();
            foreach (var piece in Enum.GetValues(typeof(Piece)).OfType<Piece>())
            {
                var pieceViewModel = new PieceViewModel(valueProvider, piece);
                Pieces.Add(pieceViewModel);
            }

            SaveCommand = new DelegateCommand(SaveCommandExecute);
            UpdateCommand = new DelegateCommand(UpdateCommandExecute);
        }

        public ObservableCollection<PieceViewModel> Pieces { get; }

        public ICommand SaveCommand { get; }

        public ICommand UpdateCommand { get; }

        private int _index;

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        private void SaveCommandExecute()
        {
            StaticTableCollection tableCollection = new StaticTableCollection();
            for (var i = 0; i < Pieces.Count; i++)
            {
                tableCollection.Values[i] = Pieces[i].ToTable();
            }
            var json = JsonConvert.SerializeObject(tableCollection, Formatting.Indented);
            File.WriteAllText(@"StaticTables.json", json);
        }

        private void UpdateCommandExecute()
        {
            PhaseViewModel phaseViewModel = Pieces[Index].GetSelectedPhase();
            var opponent = Pieces[Index].Piece.GetOpponent();
            var pieceViewModel = Pieces.FirstOrDefault(p => p.Piece == opponent);
            var opponentPhase = pieceViewModel.Phases.FirstOrDefault(p => p.Phase == phaseViewModel.Phase);

            foreach (var squareViewModel in phaseViewModel.Squares)
            {
                var square = opponentPhase.Squares.FirstOrDefault(s => s.Square == squareViewModel.Square.GetOpponent());
                square.Value = squareViewModel.Value;
            }
        }
    }
}
