using CommonServiceLocator;
using Data.Common;
using DataViewer.Models;
using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace DataViewer.Views
{
    public class DataViewModel : BindableBase
    {
        private string _outputSequenceFile;
        private readonly string _outputSequenceDirectory = "Sequences";

        private readonly short _searchDepth;
        private readonly IPosition _position;
        private List<MoveSequence> _sequences;
        private readonly Dictionary<string, CellViewModel> _cellsMap;

        private readonly IMoveFormatter _moveFormatter;
        private readonly IMoveHistoryService _moveHistoryService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IDataKeyService _dataKeyService;
        private readonly IMoveProvider _moveProvider;

        public DataViewModel(IMoveFormatter moveFormatter, IDataAccessService dataAccessService, IDataKeyService dataKeyService, IConfigurationProvider configurationProvider)
        {
            _sequenceNumber = -1;

            _searchDepth = configurationProvider.BookConfiguration.SaveDepth;

            _dataAccessService = dataAccessService;
            _dataKeyService = dataKeyService;

            _cellsMap = new Dictionary<string, CellViewModel>(64);

            InitializeCells();

            InitializeBoard();

            InitializeSequenceOutput();

            _position = new Position();

            _moveFormatter = moveFormatter;
            _moveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
            _moveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

            MoveItems = new ObservableCollection<MoveModel>();
            DataItems = new ObservableCollection<DataModel>();

            InitializeCommands();

            InitializeMoves();
        }

        #region Properties

        private string _opening;
        public string Opening
        {
            get => _opening;
            set => SetProperty(ref _opening, value);
        }

        private int _sequenceNumber;
        public int SequenceNumber
        {
            get { return _sequenceNumber; }
            set
            {
                if(SetProperty(ref _sequenceNumber, value))
                {
                    if(_sequenceNumber > -1)
                    {
                        while (UndoCommandCanExecute())
                        {
                            UndoCommandExecute();
                        }

                        var sequence = _sequences[_sequenceNumber];

                        for(int i = 0; i< sequence.Keys.Count; i++)
                        {
                            var item = DataItems.FirstOrDefault(d => d.Key == sequence.Keys[i]);
                            SelectionCommandExecute(item);
                        }
                    }

                    PreviouseSequenceCommand.RaiseCanExecuteChanged();
                    NextSequenceCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private IEnumerable<int> _numbers;

        public IEnumerable<int> Numbers
        {
            get => _numbers;
            set => SetProperty(ref _numbers, value);
        }

        private IEnumerable<string> _labels;

        public IEnumerable<string> Labels
        {
            get => _labels;
            set => SetProperty(ref _labels, value);
        }

        private IEnumerable<CellViewModel> _cells;

        public IEnumerable<CellViewModel> Cells
        {
            get => _cells;
            set => SetProperty(ref _cells, value);
        }

        public ObservableCollection<MoveModel> MoveItems { get; }
        public ObservableCollection<DataModel> DataItems { get; }

        #endregion

        #region Commands

        public ICommand SelectionCommand { get; private set; }
        public DelegateCommand UndoCommand { get; private set; }
        public DelegateCommand SequenceCommand { get; private set; }

        public ICommand LoadSequenceCommand { get; private set; }
        public DelegateCommand PreviouseSequenceCommand { get; private set; }
        public DelegateCommand NextSequenceCommand { get; private set; }

        private void InitializeCommands()
        {
            SelectionCommand = new DelegateCommand<DataModel>(SelectionCommandExecute);
            UndoCommand = new DelegateCommand(UndoCommandExecute, UndoCommandCanExecute);
            SequenceCommand = new DelegateCommand(SequenceCommandExecute, SequenceCommandCanExecute);
            LoadSequenceCommand = new DelegateCommand(LoadSequenceCommandExecute);
            PreviouseSequenceCommand = new DelegateCommand(PreviouseSequenceCommandExecute, PreviouseSequenceCommandCanExecute);
            NextSequenceCommand = new DelegateCommand(NextSequenceCommandExecute, NextSequenceCommandCanExecute);
        }

        private bool NextSequenceCommandCanExecute()
        {
            return _sequences != null && _sequenceNumber < _sequences.Count - 1;
        }

        private void NextSequenceCommandExecute()
        {
            SequenceNumber = _sequenceNumber + 1;
        }

        private bool PreviouseSequenceCommandCanExecute()
        {
            return _sequences != null && _sequenceNumber > 0;
        }

        private void PreviouseSequenceCommandExecute()
        {
            SequenceNumber = _sequenceNumber - 1;
        }

        private void LoadSequenceCommandExecute()
        {
            // Configure open file dialog box
            var dialog = new OpenFileDialog
            {
                FileName = "Sequence", 
                DefaultExt = ".txt", //
                Filter = "Text documents (.txt)|*.txt", 
                InitialDirectory = new DirectoryInfo("Config").FullName
            };

            if (dialog.ShowDialog() == true)
            {
                string filename = dialog.FileName;

                _sequences = File.ReadLines(filename)
                .Select(JsonConvert.DeserializeObject<MoveSequence>)
                .ToList();

                SequenceNumber = 0;
            }
        }

        private bool SequenceCommandCanExecute()
        {
            return MoveItems.Count > 0;
        }

        private void SequenceCommandExecute()
        {
           MoveSequence moveSequence = new MoveSequence();

            foreach (var item in _position.GetHistory())
            {
                moveSequence.Add(item);
            }

            using (var stream = new StreamWriter(_outputSequenceFile, true))
            {
                stream.WriteLine(JsonConvert.SerializeObject(moveSequence));
            }
        }

        private bool UndoCommandCanExecute()
        {
            return MoveItems.Count > 0;
        }

        private void UndoCommandExecute()
        {
            MoveItems.RemoveAt(MoveItems.Count - 1);

            _position.UnMake();

            InitializeMoves();

            UpdateView();
        }

        private void SelectionCommandExecute(DataModel obj)
        {
            if (obj == null)
            {
                return;
            }

            var move = _moveProvider.Get(obj.Key);
            if (_moveHistoryService.Any())
            {
                _position.Make(move);

                MoveItems.Add(new MoveModel { Number = MoveItems.Last().Number + 1, Move = move.ToString() });
            }
            else
            {
                _position.MakeFirst(move);
                MoveItems.Add(new MoveModel { Number = 1, Move = move.ToString() });
            }

            InitializeMoves();

            UpdateView();
        }

        #endregion

        #region Private

        private void InitializeSequenceOutput()
        {
            _outputSequenceFile = Path.Combine(_outputSequenceDirectory, "Sequence.txt");
            if (!Directory.Exists(_outputSequenceDirectory))
            {
                Directory.CreateDirectory(_outputSequenceDirectory);
            }
            else if (File.Exists(_outputSequenceFile))
            {
                File.Move(_outputSequenceFile, Path.Combine(_outputSequenceDirectory, $"Sequence_{DateTime.Now.ToFileName()}.txt"));
            }
        }

        private void UpdateView()
        {
            foreach (var cell in _cells)
            {
                _position.GetPiece(cell.Cell, out var piece);
                _cellsMap[cell.Cell.AsString()].Figure = piece;
            }

            UndoCommand.RaiseCanExecuteChanged();
            SequenceCommand.RaiseCanExecuteChanged();
        }

        private void InitializeMoves()
        {
            var moves = _position.GetAllMoves();

            MoveKeyList keys = stackalloc short[_searchDepth];

            _moveHistoryService.GetSequence(ref keys);

            var key = _dataKeyService.Get(ref keys);

            HistoryValue history = _dataAccessService.Get(key);

            List<DataModel> models= new List<DataModel>();

            foreach (var move in moves)
            {
                var book = history.GetBookValue(move.Key);

                int total = book.GetTotal();

                models.Add(new DataModel
                {
                    Key = move.Key,
                    Move = _moveFormatter.Format(move),
                    Total = total,
                    WhiteCount = book.White,
                    DrawCount = book.Draw,
                    BlackCount = book.Black,
                    WhitePercentage = book.GetWhitePercentage(total),
                    DrawPercentage = book.GetDrawPercentage(total),
                    BlackPercentage = book.GetBlackPercentage(total),
                    Difference = _position.GetTurn() == Turn.White? book.GetWhite() : book.GetBlack(),
                }) ;
            }

            models = models.OrderByDescending(m => m.Total).ToList();

            DataItems.Clear();

            for (int i = 0; i < models.Count; i++)
            {
                models[i].Number = i + 1;

                DataItems.Add(models[i]);
            }

            var opening = _dataAccessService.GetOpening(key);

            if (!string.IsNullOrWhiteSpace(opening) || string.IsNullOrWhiteSpace(key))
            {
                Opening = opening; 
            }
        }

        private void InitializeBoard()
        {
            var numbers = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var labels = new[] { "A", "B", "C", "D", "E", "F", "G", "H" };
            List<CellViewModel> models = new List<CellViewModel>(64);

            var color = "White";

            if (color == "White")
            {
                var array = numbers.Reverse().ToArray();
                Numbers = array;
                Labels = labels;

                foreach (var n in array)
                {
                    foreach (var l in labels)
                    {
                        var model = _cellsMap[$"{l}{n}"];
                        models.Add(model);
                    }
                }

                Cells = models;
            }
            else
            {
                var array = labels.Reverse().ToArray();
                Numbers = numbers;
                Labels = array;

                foreach (var n in numbers)
                {
                    foreach (var l in array)
                    {
                        var model = _cellsMap[$"{l}{n}"];
                        models.Add(model);
                    }
                }

                Cells = models;

            }
        }

        private void InitializeCells()
        {
            for (byte i = 0; i < 64; i++)
            {
                var x = i / 8;
                var y = i % 2;
                CellType cellType;

                if (x % 2 == 0)
                {
                    cellType = y == 0 ? CellType.Black : CellType.White;
                }
                else
                {
                    cellType = y == 1 ? CellType.Black : CellType.White;
                }

                CellViewModel cell = new CellViewModel { Cell = i, CellType = cellType };
                _cellsMap[i.AsString()] = cell;
            }

            FillCells();
        }

        private void FillCells()
        {
            _cellsMap["A1"].Figure = Pieces.WhiteRook;
            _cellsMap["B1"].Figure = Pieces.WhiteKnight;
            _cellsMap["C1"].Figure = Pieces.WhiteBishop;
            _cellsMap["D1"].Figure = Pieces.WhiteQueen;
            _cellsMap["E1"].Figure = Pieces.WhiteKing;
            _cellsMap["F1"].Figure = Pieces.WhiteBishop;
            _cellsMap["G1"].Figure = Pieces.WhiteKnight;
            _cellsMap["H1"].Figure = Pieces.WhiteRook;

            _cellsMap["A2"].Figure = Pieces.WhitePawn;
            _cellsMap["B2"].Figure = Pieces.WhitePawn;
            _cellsMap["C2"].Figure = Pieces.WhitePawn;
            _cellsMap["D2"].Figure = Pieces.WhitePawn;
            _cellsMap["E2"].Figure = Pieces.WhitePawn;
            _cellsMap["F2"].Figure = Pieces.WhitePawn;
            _cellsMap["G2"].Figure = Pieces.WhitePawn;
            _cellsMap["H2"].Figure = Pieces.WhitePawn;

            _cellsMap["A7"].Figure = Pieces.BlackPawn;
            _cellsMap["B7"].Figure = Pieces.BlackPawn;
            _cellsMap["C7"].Figure = Pieces.BlackPawn;
            _cellsMap["D7"].Figure = Pieces.BlackPawn;
            _cellsMap["E7"].Figure = Pieces.BlackPawn;
            _cellsMap["F7"].Figure = Pieces.BlackPawn;
            _cellsMap["G7"].Figure = Pieces.BlackPawn;
            _cellsMap["H7"].Figure = Pieces.BlackPawn;

            _cellsMap["A8"].Figure = Pieces.BlackRook;
            _cellsMap["B8"].Figure = Pieces.BlackKnight;
            _cellsMap["C8"].Figure = Pieces.BlackBishop;
            _cellsMap["D8"].Figure = Pieces.BlackQueen;
            _cellsMap["E8"].Figure = Pieces.BlackKing;
            _cellsMap["F8"].Figure = Pieces.BlackBishop;
            _cellsMap["G8"].Figure = Pieces.BlackKnight;
            _cellsMap["H8"].Figure = Pieces.BlackRook;
        }

        #endregion
    }
}
