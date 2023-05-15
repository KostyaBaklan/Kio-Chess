using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Application.Helpers;
using Application.Interfaces;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Strategies.Base;
using Engine.Tools;
using Kgb.ChessApp.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Kgb.ChessApp.Views
{
    public class GameViewModel : BindableBase, INavigationAware
    {
        private short _level;
        private bool _disableSelection;
        private Turn _machine;
        private readonly double _blockTimeout;
        private Turn _turn = Turn.White;
        private List<MoveBase> _moves;
        private readonly Stack<TimeSpan> _times;

        private readonly IPosition _position;
        private StrategyBase _strategy;
        private readonly Dictionary<string, CellViewModel> _cellsMap;

        private readonly IMoveFormatter _moveFormatter;
        private readonly IEvaluationService _evaluationService;
        private readonly IMoveHistoryService _moveHistoryService;
        private readonly IStrategyProvider _strategyProvider;

        public GameViewModel(IMoveFormatter moveFormatter, IStrategyProvider strategyProvider)
        {
            _disableSelection = false;
            _times = new Stack<TimeSpan>();
            _blockTimeout = ServiceLocator.Current.GetInstance<IConfigurationProvider>()
                .GeneralConfiguration.BlockTimeout;
            _moveFormatter = moveFormatter;
            _strategyProvider = strategyProvider;

            _cellsMap = new Dictionary<string, CellViewModel>(64);
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

            _position = new Position();

            MoveItems = new ObservableCollection<MoveModel>();

            SelectionCommand = new DelegateCommand<CellViewModel>(SelectionCommandExecute, SelectionCommandCanExecute);
            UndoCommand = new DelegateCommand(UndoCommandExecute);
            SaveHistoryCommand = new DelegateCommand(SaveHistoryCommandExecute);
            _evaluationService = ServiceLocator.Current.GetInstance<IEvaluationService>();
            _moveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
            _strategyProvider = strategyProvider;
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

        private bool _useMachine;

        public bool UseMachine
        {
            get => _useMachine;
            set => SetProperty(ref _useMachine, value);
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

        private string _title;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private TimeSpan _maximum;

        public TimeSpan Maximum
        {
            get => _maximum;
            set => SetProperty(ref _maximum, value);
        }

        private TimeSpan _minimum;

        public TimeSpan Minimum
        {
            get => _minimum;
            set => SetProperty(ref _minimum, value);
        }

        private TimeSpan _average;

        public TimeSpan Average
        {
            get => _average;
            set => SetProperty(ref _average, value);
        }

        private TimeSpan _std;

        public TimeSpan Std
        {
            get => _std;
            set => SetProperty(ref _std, value);
        }

        public ObservableCollection<MoveModel> MoveItems { get; }

        public ICommand SelectionCommand { get; }

        public ICommand UndoCommand { get; }

        public ICommand SaveHistoryCommand { get; }

        #region Implementation of INavigationAware

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var numbers = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var labels = new[] { "A", "B", "C", "D", "E", "F", "G", "H" };
            List<CellViewModel> models = new List<CellViewModel>(64);
            var color = navigationContext.Parameters.GetValue<string>("Color");

            var level = navigationContext.Parameters.GetValue<short>("Level");
            _strategy = _strategyProvider.GetStrategy(level, _position);
            _level = level;
            Title = $"Strategy={_strategy}, Level={level}";

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

                MakeMachineMove();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        #endregion

        private void SaveHistoryCommandExecute()
        {
            _position.SaveHistory();
        }

        private void UndoCommandExecute()
        {
            if (_disableSelection) return;
            if (!MoveItems.Any()) return;

            Zero();

            _position.UnMake();
            //_strategy.Back();

            var moveModel = MoveItems.Last();
            if (string.IsNullOrEmpty(moveModel.Black))
            {
                MoveItems.Remove(moveModel);
            }
            else
            {
                moveModel.Black = null;
            }

            UpdateView();

            _turn = _position.GetTurn();

            if (_useMachine && _machine == _turn)
            {
                _times.Pop();
                UpdateTime();
            }
        }

        private bool SelectionCommandCanExecute(CellViewModel arg)
        {
            if (_disableSelection) return false;
            if (arg.State == State.MoveTo || arg.State == State.MoveFrom) return true;

            return arg.Figure != null && (_turn == Turn.White ? arg.Figure.Value.IsWhite() : arg.Figure.Value.IsBlack());
        }

        private void SelectionCommandExecute(CellViewModel cellViewModel)
        {
            switch (cellViewModel.State)
            {
                case State.Idle:
                    Zero();

                    IEnumerable<MoveBase> possibleMoves = GetAllMoves(cellViewModel.Cell, cellViewModel.Figure.Value);
                    _moves = possibleMoves.ToList();
                    if (_moves.Any())
                    {
                        foreach (var m in _moves)
                        {
                            _cellsMap[m.To.AsString()].State = State.MoveTo;
                        }

                        cellViewModel.State = State.MoveFrom;
                    }
                    break;
                case State.MoveFrom:

                    Zero();
                    break;
                case State.MoveTo:

                    Zero();

                    var move = _moves.FirstOrDefault(m => m.To.Equals(cellViewModel.Cell));
                    if (move == null) return;

                    MakeMove(move);

                    MakeMachineMove();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void MakeMachineMove()
        {
            if (!_useMachine) return;

            _disableSelection = true;

            _machine = _position.GetTurn();

            Task.Delay(10)
                .ContinueWith(t =>
                {
                    var timer = new Stopwatch();
                    timer.Start();

                    while (_strategy.IsBlocked())
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(_blockTimeout));
                    }

                    var q = _strategy.GetResult();

                    //MoveGenerationPerformance.Save();

                    _strategy.ExecuteAsyncAction();
                    timer.Stop();
                    return new Tuple<IResult, TimeSpan>(q, timer.Elapsed);
                })
                .ContinueWith(t =>
                {
                    Tuple<IResult, TimeSpan> tResult = null;
                    try
                    {
                        tResult = t.Result;
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show($"Error = {exception} !");
                    }
                    if (tResult != null)
                    {
                        _times.Push(tResult.Item2);
                        UpdateTime();

                        MessageBox.Show($"Elapsed = {tResult.Item2} !");
                        switch (t.Result.Item1.GameResult)
                        {
                            case GameResult.Continue:
                                MakeMove(tResult.Item1.Move, tResult.Item2);
                                break;
                            case GameResult.Pat:
                                MessageBox.Show("Pat !!!");
                                break;
                            case GameResult.Draw:
                                MessageBox.Show("Draw !!!");
                                break;
                            case GameResult.ThreefoldRepetition:
                                MessageBox.Show("Threefold Repetition !!!");
                                break;
                            case GameResult.FiftyMoves:
                                MessageBox.Show("50 Moves !!!");
                                break;
                            case GameResult.Mate:
                                MessageBox.Show("Mate !!!");
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                    {
                        MessageBox.Show($"No Moves");
                    }

                    _disableSelection = false;
                },
                    CancellationToken.None, TaskContinuationOptions.LongRunning,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UpdateTime()
        {
            if (_times.Any())
            {
                Maximum = _times.Max();
                Minimum = _times.Min();

                var enumerable = _times.Select(t => t.TotalMilliseconds).ToList();
                Average = TimeSpan.FromMilliseconds(enumerable.Average());
                var standardDeviation = enumerable.StandardDeviation();
                if (!double.IsNaN(standardDeviation))
                {
                    Std = TimeSpan.FromMilliseconds(standardDeviation);
                }
            }
            else
            {
                Maximum = Minimum = Average = Std = TimeSpan.Zero;
            }
        }

        private IEnumerable<MoveBase> GetAllMoves(byte cell, byte piece)
        {
            return _position.GetAllMoves(cell, piece);
        }

        private void MakeMove(MoveBase move, TimeSpan? time = null)
        {
            if(_moveHistoryService.GetPly()< 0)
            {
                _position.MakeFirst(move);
            }
            else
            {
                _position.Make(move);
            }

            var lastModel = MoveItems.LastOrDefault();
            MoveModel mm = lastModel;
            if (lastModel == null)
            {
                var model = new MoveModel
                {
                    Number = 1,
                    White = $" {_moveFormatter.Format(move)} ",
                    WhiteValue = $" S={-_position.GetStaticValue()} V={-_position.GetValue()}"
                };
                MoveItems.Add(model);
                mm = model;
            }
            else
            {
                if (_turn == Turn.White)
                {
                    var model = new MoveModel
                    {
                        Number = lastModel.Number + 1,
                        White = $" {_moveFormatter.Format(move)} ",
                        WhiteValue = $" S={-_position.GetStaticValue()} V={-_position.GetValue()}"
                    };
                    MoveItems.Add(model);
                    mm = model;
                }
                else
                {
                    lastModel.Black = $" {_moveFormatter.Format(move)} ";
                    lastModel.BlackValue = $" S={-_position.GetStaticValue()} V={-_position.GetValue()}";
                    var process = Process.GetCurrentProcess();
                    lastModel.Memory = $" {process.WorkingSet64 / 1024 / 1024} MB";
                    lastModel.Table = Math.Round(_strategy.Size/1024.0,2);
                }
            }

            if (time != null)
            {
                mm.Time = time.Value.ToString("mm\\:ss\\.fff");
            }

            UpdateView();

            _turn = _position.GetTurn();

            Thread.Sleep(100);
        }

        private void UpdateView()
        {
            foreach (var cell in _cells)
            {
                _position.GetPiece(cell.Cell, out var piece);
                _cellsMap[cell.Cell.AsString()].Figure = piece;
            }
        }

        private void Zero()
        {
            foreach (var viewModel in _cellsMap.Values)
            {
                viewModel.State = State.Idle;
            }
        }
    }
}