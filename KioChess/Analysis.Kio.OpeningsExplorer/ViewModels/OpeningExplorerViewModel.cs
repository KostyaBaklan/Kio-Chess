using Analysis.Core.Interfaces;
using Analysis.Core.Services;
using Analysis.DataAccess.Interfaces;
using Analysis.DataAccess.Services;
using Analysis.UI.Common.Models;
using Analysis.UI.Common.ViewModels;
using DataAccess.Entities;
using Engine.Models.Boards;
using System.Collections.ObjectModel;

namespace Analysis.Kio.OpeningsExplorer.ViewModels;

public class OpeningExplorerViewModel : BindableBase
{
    private readonly IOpeningExplorerService _explorerService;
    private readonly OpeningNavigator _navigator;
    private readonly Position _position;
    private readonly ISettingsService _settingsService;
    private string _lastKnownOpeningName = "Starting Position";
    private string _lastKnownECOCode = string.Empty;
    private readonly List<string> _moveHistoryUCI = new();
    private readonly List<string> _moveHistorySAN = new();

    public OpeningExplorerViewModel(
        IOpeningExplorerService explorerService,
        Position position,
        ISettingsService settingsService)
    {
        _explorerService = explorerService;
        _position = position;
        _settingsService = settingsService;
        _navigator = new OpeningNavigator(_explorerService);

        Board = new BoardViewModel { BoardState = BoardState.ReadOnly };
        PossibleMoves = [];
        SearchResults = [];
        SearchSuggestions = [];

        PlayMoveCommand = new DelegateCommand<OpeningMoveViewModel>(OnPlayMove, CanPlayMove);
        PreviousMoveCommand = new DelegateCommand(OnPreviousMove, CanPreviousMove);
        ResetCommand = new DelegateCommand(OnReset);
        SearchCommand = new DelegateCommand(OnSearch);
        //ImportDataCommand = new DelegateCommand(OnImportData);
        JumpToOpeningCommand = new DelegateCommand<OpeningSearchResultViewModel>(OnJumpToOpening);
        SelectSuggestionCommand = new DelegateCommand<string>(OnSelectSuggestion);

        _ = InitializeAsync();
    }

    public BoardViewModel Board { get; }
    public ObservableCollection<OpeningMoveViewModel> PossibleMoves { get; }
    public ObservableCollection<OpeningSearchResultViewModel> SearchResults { get; }
    public ObservableCollection<string> SearchSuggestions { get; }

    public DelegateCommand<OpeningMoveViewModel> PlayMoveCommand { get; }
    public DelegateCommand PreviousMoveCommand { get; }
    public DelegateCommand ResetCommand { get; }
    public DelegateCommand SearchCommand { get; }
    public DelegateCommand ImportDataCommand { get; }
    public DelegateCommand<OpeningSearchResultViewModel> JumpToOpeningCommand { get; }
    public DelegateCommand<string> SelectSuggestionCommand { get; }

    private string _currentOpeningName = "Starting Position";
    public string CurrentOpeningName
    {
        get => _currentOpeningName;
        set => SetProperty(ref _currentOpeningName, value);
    }

    private string _ecoCode = string.Empty;
    public string ECOCode
    {
        get => _ecoCode;
        set => SetProperty(ref _ecoCode, value);
    }

    private string _moveHistory = string.Empty;
    public string MoveHistory
    {
        get => _moveHistory;
        set => SetProperty(ref _moveHistory, value);
    }

    private int _depth;
    public int Depth
    {
        get => _depth;
        set => SetProperty(ref _depth, value);
    }

    private bool _isInBook = true;
    public bool IsInBook
    {
        get => _isInBook;
        set => SetProperty(ref _isInBook, value);
    }

    private string _statusMessage = "Loading opening database...";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                _ = UpdateSearchSuggestionsWithDebounceAsync();
            }
        }
    }

    private bool _isSearchExpanded;
    public bool IsSearchExpanded
    {
        get => _isSearchExpanded;
        set => SetProperty(ref _isSearchExpanded, value);
    }

    private bool _showSuggestions;
    public bool ShowSuggestions
    {
        get => _showSuggestions;
        set => SetProperty(ref _showSuggestions, value);
    }

    private int _totalOpeningsCount;
    public string TotalOpeningsDisplay => $"{_totalOpeningsCount:N0} openings loaded";

    private bool _isDatabaseInitialized;
    public bool IsDatabaseInitialized
    {
        get => _isDatabaseInitialized;
        set => SetProperty(ref _isDatabaseInitialized, value);
    }

    private OpeningMoveViewModel _selectedMove;
    public OpeningMoveViewModel SelectedMove
    {
        get => _selectedMove;
        set
        {
            if (SetProperty(ref _selectedMove, value) && value != null)
            {
                OnPlayMove(value);
            }
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            IsDatabaseInitialized = true;
            _totalOpeningsCount = _explorerService.GetTotalOpeningsCount();
            StatusMessage = $"Ready - {TotalOpeningsDisplay}";
            RaisePropertyChanged(nameof(TotalOpeningsDisplay));

            await LoadPossibleMovesAsync();

            Board.LoadPosition(_position);
            Board.SyncFromPosition();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error initializing: {ex.Message}";
            IsDatabaseInitialized = false;
        }
    }

    private bool CanPlayMove(OpeningMoveViewModel move) => move != null;

    private async void OnPlayMove(OpeningMoveViewModel moveVm)
    {
        if (moveVm == null) return;

        try
        {
            var legalMoves = _position.GetAllMoves();
            var move = legalMoves.FirstOrDefault(m =>
                UciMoveConverter.ToUci(m) == moveVm.MoveUCI);

            if (move != null)
            {
                // Try to advance in the opening book
                var success = await _navigator.MoveNextAsync(move.Key);

                // Make the move on the board regardless of whether it's in the book
                if (_navigator.Depth == 1)
                    _position.MakeFirst(move);
                else
                    _position.Make(move);

                Board.SyncFromPosition();

                // Track move history
                _moveHistoryUCI.Add(moveVm.MoveUCI);
                _moveHistorySAN.Add(moveVm.MoveSAN);

                // Update last known opening if still in book
                if (success && _navigator.IsInBook)
                {
                    _lastKnownOpeningName = _navigator.CurrentOpeningName;
                    _lastKnownECOCode = _navigator.ECOCode;
                }
            }

            await UpdateNavigationStateAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error playing move: {ex.Message}";
        }
    }

    private bool CanPreviousMove() => _moveHistoryUCI.Count > 0;

    private async void OnPreviousMove()
    {
        try
        {
            if (_moveHistoryUCI.Count == 0) return;

            // Remove last move from history
            _moveHistoryUCI.RemoveAt(_moveHistoryUCI.Count - 1);
            _moveHistorySAN.RemoveAt(_moveHistorySAN.Count - 1);

            // Move back in navigator
            await _navigator.MovePreviousAsync();

            // Unmake the move on the board
            _position.UnMake();
            Board.SyncFromPosition();

            // Update last known opening
            if (_navigator.IsInBook)
            {
                _lastKnownOpeningName = _navigator.CurrentOpeningName;
                _lastKnownECOCode = _navigator.ECOCode;
            }

            await UpdateNavigationStateAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async void OnReset()
    {
        try
        {
            _navigator.Reset();
            _position.Clear();
            Board.LoadPosition(_position);
            Board.SyncFromPosition();

            // Reset move history and last known opening
            _moveHistoryUCI.Clear();
            _moveHistorySAN.Clear();
            _lastKnownOpeningName = "Starting Position";
            _lastKnownECOCode = string.Empty;

            await UpdateNavigationStateAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async void OnSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery) || SearchQuery.Length < 2)
        {
            IsSearchExpanded = false;
            ShowSuggestions = false;
            return;
        }

        ShowSuggestions = false;
        await PerformSearchAsync();
        IsSearchExpanded = SearchResults.Count > 0;
    }

    private CancellationTokenSource _searchCts;

    private async Task UpdateSearchSuggestionsWithDebounceAsync()
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        try
        {
            await Task.Delay(300, token);
            await UpdateSearchSuggestionsAsync();
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async Task UpdateSearchSuggestionsAsync()
    {
        SearchSuggestions.Clear();
        ShowSuggestions = false;

        if (string.IsNullOrWhiteSpace(SearchQuery))
            return;

        try
        {
            // Get more results for better prioritization (100 instead of 50)
            var results = await _explorerService.SearchByNameAsync(SearchQuery, 100);

            if (results.Count == 0)
                return;

            // Score, sort, and take top 15
            var scoredResults = results
                .Select(r => new
                {
                    Opening = r,
                    Score = CalculateSearchScore(r, SearchQuery)
                })
                .OrderByDescending(x => x.Score) // ? Sorting happens HERE, not in DB query
                .Take(15);

            foreach (var item in scoredResults)
            {
                var result = item.Opening;
                var suggestion = string.IsNullOrEmpty(result.ECO)
                    ? result.FullName
                    : $"[{result.ECO}] {result.FullName}";
                SearchSuggestions.Add(suggestion);
            }

            ShowSuggestions = SearchSuggestions.Count > 0;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search error: {ex.Message}";
        }
    }

    private int CalculateSearchScore(OpeningEntry opening, string query)
    {
        var queryLower = query.ToLowerInvariant();
        int score = 0;

        // PRIORITY 1: ECO Code (highest priority)
        if (!string.IsNullOrEmpty(opening.ECO))
        {
            var ecoLower = opening.ECO.ToLowerInvariant();

            // Exact ECO match (e.g., "B20" searches for "B20")
            if (ecoLower == queryLower)
                score += 10000; // Very high priority
            // ECO starts with query (e.g., "B" matches "B20", "B56", etc.)
            else if (ecoLower.StartsWith(queryLower))
                score += 5000;
            // ECO contains query
            else if (ecoLower.Contains(queryLower))
                score += 2000;
        }

        // PRIORITY 2: Opening Name (second priority)
        if (!string.IsNullOrEmpty(opening.Name))
        {
            var nameLower = opening.Name.ToLowerInvariant();

            // Exact name match
            if (nameLower == queryLower)
                score += 1000;
            // Name starts with query (e.g., "Sicil" ? "Sicilian Defense")
            else if (nameLower.StartsWith(queryLower))
                score += 500;
            // Name contains query at word boundary (e.g., "Def" ? "Sicilian Defense")
            else if (nameLower.Contains(" " + queryLower))
                score += 300;
            // Name contains query anywhere
            else if (nameLower.Contains(queryLower))
                score += 200;
        }

        // Also check FullName (includes variations) with lower priority than base name
        if (!string.IsNullOrEmpty(opening.FullName))
        {
            var fullNameLower = opening.FullName.ToLowerInvariant();

            // Only add if not already matched in Name
            if (!opening.Name?.ToLowerInvariant().Contains(queryLower) ?? false)
            {
                // Full name starts with query
                if (fullNameLower.StartsWith(queryLower))
                    score += 250;
                // Full name contains query at word boundary
                else if (fullNameLower.Contains(" " + queryLower))
                    score += 150;
                // Full name contains query anywhere
                else if (fullNameLower.Contains(queryLower))
                    score += 100;
            }
        }

        // PRIORITY 3: Variation (third priority)
        if (!string.IsNullOrEmpty(opening.Variation))
        {
            var variationLower = opening.Variation.ToLowerInvariant();

            // Variation exact match
            if (variationLower == queryLower)
                score += 150;
            // Variation starts with query
            else if (variationLower.StartsWith(queryLower))
                score += 100;
            // Variation contains query
            else if (variationLower.Contains(queryLower))
                score += 50;
        }

        // Minor bonuses
        score += opening.Popularity / 10; // Popularity (small influence)

        // Prefer shorter move counts (more fundamental openings)
        if (opening.MoveCount <= 3)
            score += 20;
        else if (opening.MoveCount <= 6)
            score += 10;

        return score;
    }

    private async Task PerformSearchAsync()
    {
        SearchResults.Clear();

        if (string.IsNullOrWhiteSpace(SearchQuery) || SearchQuery.Length < 2)
            return;

        try
        {
            var results = await _explorerService.SearchByNameAsync(SearchQuery, 100);

            // Score and sort by relevance (not popularity!)
            var sortedResults = results
                .Select(r => new
                {
                    Opening = r,
                    Score = CalculateSearchScore(r, SearchQuery)
                })
                .OrderByDescending(x => x.Score)
                .Take(50);

            foreach (var item in sortedResults)
            {
                var result = item.Opening;
                SearchResults.Add(new OpeningSearchResultViewModel
                {
                    Name = result.FullName,
                    ECO = result.ECO,
                    Moves = result.MovesSAN,
                    Popularity = result.Popularity,
                    OpeningId = result.Id,
                    MovesUCI = result.MovesUCI
                });
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search error: {ex.Message}";
        }
    }

    private async void OnJumpToOpening(OpeningSearchResultViewModel result)
    {
        if (result == null) return;

        try
        {
            _navigator.Reset();
            _position.Clear();
            Board.LoadPosition(_position);

            // Reset move history
            _moveHistoryUCI.Clear();
            _moveHistorySAN.Clear();

            var movesUCI = result.MovesUCI.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var movesSAN = result.Moves.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(m => !m.Contains('.') && !int.TryParse(m, out _))
                .ToArray();

            bool isFirstMove = true;
            for (int i = 0; i < movesUCI.Length; i++)
            {
                var uciMove = movesUCI[i];
                var legalMoves = _position.GetAllMoves();
                var move = legalMoves.FirstOrDefault(m => UciMoveConverter.ToUci(m) == uciMove);

                if (move != null)
                {
                    await _navigator.MoveNextAsync(move.Key);
                    if (isFirstMove)
                    {
                        _position.MakeFirst(move);
                        isFirstMove = false;
                    }
                    else
                    {
                        _position.Make(move);
                    }

                    // Track move history
                    _moveHistoryUCI.Add(uciMove);
                    if (i < movesSAN.Length)
                        _moveHistorySAN.Add(movesSAN[i]);
                }
            }

            // Update last known opening
            if (_navigator.IsInBook)
            {
                _lastKnownOpeningName = _navigator.CurrentOpeningName;
                _lastKnownECOCode = _navigator.ECOCode;
            }

            Board.SyncFromPosition();
            await UpdateNavigationStateAsync();

            IsSearchExpanded = false;
            ShowSuggestions = false;
            SearchQuery = string.Empty;

            StatusMessage = $"Jumped to {result.Name}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async void OnSelectSuggestion(string suggestion)
    {
        if (string.IsNullOrEmpty(suggestion)) return;

        try
        {
            ShowSuggestions = false;

            var query = suggestion.StartsWith("[")
                ? suggestion.Substring(suggestion.IndexOf(']') + 2).Trim()
                : suggestion;

            var results = await _explorerService.SearchByNameAsync(query, 1);
            if (results.Count > 0)
            {
                var opening = results[0];
                var searchResult = new OpeningSearchResultViewModel
                {
                    Name = opening.FullName,
                    ECO = opening.ECO,
                    Moves = opening.MovesSAN,
                    MovesUCI = opening.MovesUCI,
                    Popularity = opening.Popularity,
                    OpeningId = opening.Id
                };

                OnJumpToOpening(searchResult);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task UpdateNavigationStateAsync()
    {
        // Show current opening if in book, otherwise show last known opening
        if (_navigator.IsInBook)
        {
            CurrentOpeningName = _navigator.CurrentOpeningName;
            ECOCode = _navigator.ECOCode;
        }
        else if (_moveHistoryUCI.Count > 0)
        {
            // Out of book - show last known opening with indicator
            CurrentOpeningName = $"{_lastKnownOpeningName} (out of book)";
            ECOCode = _lastKnownECOCode;
        }
        else
        {
            CurrentOpeningName = "Starting Position";
            ECOCode = string.Empty;
        }

        // Show full move history (even if out of book)
        MoveHistory = _moveHistorySAN.Count > 0
            ? string.Join(" ", _moveHistorySAN.Select((move, i) =>
                i % 2 == 0 ? $"{i / 2 + 1}.{move}" : move))
            : string.Empty;

        Depth = _moveHistoryUCI.Count;
        IsInBook = _navigator.IsInBook;

        await LoadPossibleMovesAsync();

        PreviousMoveCommand.RaiseCanExecuteChanged();
    }

    private async Task LoadPossibleMovesAsync()
    {
        PossibleMoves.Clear();

        try
        {
            // Get all legal moves from the current position
            var allLegalMoves = _position.GetAllMoves();

            // Get moves from opening database
            var openingMoves = await _navigator.GetPossibleMovesAsync();

            // Create a dictionary of opening moves by UCI for quick lookup
            var openingMovesByUci = openingMoves
                .GroupBy(m => m.MoveUCI)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(m => !string.IsNullOrEmpty(m.ECO))
                          .ThenByDescending(m => m.IsMainLine)
                          .ThenByDescending(m => m.Popularity)
                          .ToList()
                );

            var allMoves = new List<OpeningMoveViewModel>();

            // Process all legal moves
            foreach (var legalMove in allLegalMoves)
            {
                var uci = UciMoveConverter.ToUci(legalMove);

                if (openingMovesByUci.TryGetValue(uci, out var dbMoves))
                {
                    // Add all variations from the database for this move
                    foreach (var dbMove in dbMoves)
                    {
                        allMoves.Add(new OpeningMoveViewModel
                        {
                            MoveUCI = dbMove.MoveUCI,
                            MoveSAN = dbMove.MoveSAN,
                            OpeningName = dbMove.OpeningName,
                            ECO = dbMove.ECO,
                            Popularity = dbMove.Popularity,
                            IsMainLine = dbMove.IsMainLine,
                            PlayMoveCommand = PlayMoveCommand
                        });
                    }
                }
                else
                {
                    // Move not in database - show as legal move without opening info
                    allMoves.Add(new OpeningMoveViewModel
                    {
                        MoveUCI = uci,
                        MoveSAN = legalMove.ToSAN(), // Use the virtual ToSAN() method
                        OpeningName = string.Empty,
                        ECO = string.Empty,
                        Popularity = 0,
                        IsMainLine = false,
                        PlayMoveCommand = PlayMoveCommand
                    });
                }
            }

            // Sort: Known openings first, then by main line, then popularity, then unknown moves
            var sortedMoves = allMoves
                .OrderByDescending(m => !string.IsNullOrEmpty(m.ECO))  // Known openings first
                .ThenByDescending(m => m.IsMainLine)                    // Main lines first
                .ThenByDescending(m => m.Popularity);                   // Then by popularity

            foreach (var move in sortedMoves)
            {
                PossibleMoves.Add(move);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading moves: {ex.Message}";
        }
    }
}