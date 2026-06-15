# StockfishBenchmark — WPF Application Implementation Plan

**Status legend:** `- [ ]` pending · `- [x]` completed · `- [~]` in progress

---

## Overview

WPF application that benchmarks Kio-Chess engine search performance by playing
it against Stockfish for a fixed number of moves, recording per-move timings,
transposition table (TT) size, process memory, and evaluation metrics, then
saving the session to a JSON file.

On subsequent runs (after a code change) the same move sequence is replayed
without Stockfish using the saved settings. The comparison view visualises
improvement or regression move-by-move.

---

## Application Modes

### Mode A — New Run (with Stockfish)
1. User chooses: Strategy (lmr / lmrd / id / asp), Depth (8–11),
   Engine Color, Move Count, Stockfish ELO / Depth.
2. App plays Engine vs Stockfish for `MoveCount` engine moves.
3. Per engine move: records duration, TT count, process memory, material/static eval.
4. Saves `BenchmarkResult` JSON to `Benchmarks/` subfolder.
5. Displays run summary statistics.

### Mode B — Replay from Baseline File
1. User loads an existing `BenchmarkResult` JSON file (produced by Mode A).
2. App extracts session parameters (strategy, depth, color, move sequence).
3. Replays stored move sequence: Stockfish moves applied directly from file,
   engine moves are re-computed with fresh timing.
4. Flags any move mismatch as **REGRESSION** (engine chose a different move).
5. Comparison view shows move-by-move delta (duration %, TT count, memory).

---

## Architecture

```
StockFishCore/Stockfish/              ← Stockfish wrapper (shared; added in Phase 1)
├── Exceptions/MaxTriesException.cs
├── Exceptions/NoMoveFoundException.cs
├── Models/Color.cs
├── Models/Evaluation.cs
├── Models/Settings.cs
├── IStockfish.cs
├── StockfishProcess.cs
└── Stockfish.cs

StockfishBenchmark/
├── Models/
│   ├── BenchmarkSession.cs       # Header: strategy, depth, color, date, machine info
│   ├── BenchmarkMoveRecord.cs    # Per-ply data: UCI move, duration, TT, memory, eval
│   ├── BenchmarkSummary.cs       # Aggregated stats: avg/min/max duration, TT, memory
│   ├── BenchmarkResult.cs        # Root: Session + Moves[] + Summary + factory method
│   └── ComparisonRow.cs          # UI row for side-by-side comparison (computed props)
├── Services/
│   ├── IBenchmarkFileService.cs  # Save / load BenchmarkResult as JSON
│   ├── BenchmarkFileService.cs
│   ├── IBenchmarkRunner.cs       # Run Engine vs Stockfish → BenchmarkResult
│   ├── BenchmarkRunner.cs
│   ├── IReplayRunner.cs          # Replay saved sequence → new BenchmarkResult
│   └── ReplayRunner.cs
├── ViewModels/
│   ├── ViewModelBase.cs          # INotifyPropertyChanged + SetProperty helper
│   ├── RelayCommand.cs           # ICommand (non-generic + generic)
│   ├── MainWindowViewModel.cs    # Navigation host: CurrentView property
│   ├── SetupViewModel.cs         # Mode selection + configuration form
│   ├── RunViewModel.cs           # Live progress + move list during run
│   └── CompareViewModel.cs       # Side-by-side comparison table
├── Views/
│   ├── SetupView.xaml(.cs)       # Form: strategy/depth/color/count OR load file
│   ├── RunView.xaml(.cs)         # ProgressBar + live DataGrid + Save/Compare buttons
│   └── CompareView.xaml(.cs)     # Two summary panels + comparison DataGrid + Export
├── Converters/
│   └── DeltaToColorConverter.cs  # String Status → SolidColorBrush (green/red/orange)
├── App.xaml(.cs)                 # extends UiApp (Prism); DbConnect inits DB+cache; RegisterLocalTypes for Phase 4
├── MainWindow.xaml(.cs)          # Shell: ContentControl bound to CurrentView (MainWindowViewModel)
└── BENCHMARK_PLAN.md             # ← this file
```

---

## Output File Format — `BenchmarkResult` JSON

```json
{
  "Version": "1.0",
  "Session": {
	"Strategy": "lmr",
	"Depth": 10,
	"Color": "w",
	"MoveCount": 24,
	"StockfishDepth": 5,
	"StockfishElo": 1500,
	"RunDate": "2025-01-15T10:30:00Z",
	"MachineName": "DESKTOP-XYZ",
	"ProcessorCount": 12,
	"RuntimeVersion": "10.0.0"
  },
  "Moves": [
	{
	  "MoveNumber": 1,
	  "IsEngineMove": false,
	  "UciMove": "e2e4",
	  "MoveNotation": "e4",
	  "DurationMs": 0.0,
	  "ProcessMemoryMB": 85.2,
	  "TtCount": 0,
	  "MaterialValue": 0,
	  "StaticValue": 0
	},
	{
	  "MoveNumber": 2,
	  "IsEngineMove": true,
	  "UciMove": "e7e5",
	  "MoveNotation": "e5",
	  "DurationMs": 423.7,
	  "ProcessMemoryMB": 102.4,
	  "TtCount": 147532,
	  "MaterialValue": 12,
	  "StaticValue": -8
	}
  ],
  "Summary": {
	"TotalEngineMoves": 24,
	"TotalDurationMs": 9850.0,
	"AvgDurationMs": 410.4,
	"MinDurationMs": 187.2,
	"MaxDurationMs": 723.5,
	"StdDevDurationMs": 95.3,
	"MedianDurationMs": 398.1,
	"FinalTtCount": 221000,
	"MaxTtCount": 245000,
	"AvgTtCount": 178000.0,
	"MaxProcessMemoryMB": 158.4,
	"AvgProcessMemoryMB": 130.2
  }
}
```

---

## Key Design Decisions

### Determinism invariant
The engine produces the same move sequence each replay when:
- Strategy type, depth, and position are identical.
- The `TranspositionTable` is created fresh per strategy instance, then shared
  across moves within the same game (matching the live run behaviour).
- Stockfish moves come from the saved file and are applied directly.

Any deviation in the replayed move (`UciMove` mismatch) is a **REGRESSION** —
the engine chose differently, meaning subsequent positions will diverge.
The comparison view flags all moves after the first regression as **DIVERGED**.

### Color convention (mirrors StockfishApp)
`Color = "w"` → Stockfish plays White, Engine plays Black.  
`Color = "b"` → Stockfish plays Black, Engine plays White.  
`isStockfishMove = (Color == "w" && turn == White) || (Color == "b" && turn == Black)`

### Move count definition
`MoveCount` is the number of **engine moves** recorded (not total plies).
Total plies ≈ 2 × MoveCount. Default: 24.

### Process memory sampling
```csharp
double memMB = Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0;
```
Sampled immediately after each engine move.

### TT count access
`TranspositionTable.Count` is already public (`WhiteTable.Count + BlackTable.Count`).
`StrategyBase.Table` is `protected`. Add one public accessor to `StrategyBase`:
```csharp
public int TtCount => Table.Count;
```
This is the **only Engine project change** required.

### Stockfish binary path
Default: `@"..\..\..\stockfish\stockfish-windows-x86-64-avx2.exe"` (same as StockfishApp).  
Store in a `StockfishSettings.json` next to the executable for easy override:
```json
{ "StockfishPath": "..\\..\\..\\stockfish\\stockfish-windows-x86-64-avx2.exe" }
```

### WPF navigation (no Prism)
`MainWindowViewModel` owns a `CurrentView` property (type `ViewModelBase`).
`MainWindow.xaml` uses a `ContentControl` bound to `CurrentView` and implicit
`DataTemplate` entries in `App.xaml` to map each ViewModel type to its View.
Child ViewModels receive a `Navigate(ViewModelBase)` delegate from the parent.

### Async game loop
`BenchmarkRunner.RunAsync` and `ReplayRunner.ReplayAsync` run on `Task.Run(...)`.
Progress is reported via `IProgress<BenchmarkMoveRecord>` which marshals to the
UI thread automatically when created on the WPF thread.

---

## NuGet Packages Required

| Package | Version | Purpose | How it arrives |
|---|---|---|---|
| `Newtonsoft.Json` | 13.0.3 | JSON serialisation | transitive via StockFishCore |
| `Prism.Unity` | 9.0.537 | DI container + PrismApplication | transitive via UI.Common |

*Prism is used via `UiApp` inheritance — no direct Prism package reference needed in StockfishBenchmark.csproj.*

## StockFishCore — Stockfish Wrapper (added in Phase 1)

The Stockfish process wrapper lives in `StockFishCore/Stockfish/` so both
`StockfishApp` and `StockfishBenchmark` can share it without a circular dependency:

```
StockFishCore/Stockfish/
├── Exceptions/
│   ├── MaxTriesException.cs       # namespace StockFishCore.Stockfish.Exceptions
│   └── NoMoveFoundException.cs
├── Models/
│   ├── Color.cs                   # namespace StockFishCore.Stockfish.Models
│   ├── Evaluation.cs
│   └── Settings.cs
├── IStockfish.cs                  # namespace StockFishCore.Stockfish
├── StockfishProcess.cs
└── Stockfish.cs
```

*StockfishApp retains its own copies in `StockfishApp.Core` / `StockfishApp.Models` — no changes to StockfishApp are required.*

## Project References in `StockfishBenchmark.csproj` (actual)

| Reference | Provides |
|---|---|
| `UI.Common\UI.Common.csproj` | `UiApp` (PrismApplication base), DI registration, Engine + DataAccess transitively |
| `StockFishCore\StockFishCore.csproj` | `Stockfish` wrapper, engine models, TT service |

---

## Implementation Steps

---

### Phase 1 — Project Configuration

- [ ] **1.1** Update `StockfishBenchmark.csproj`
  - Change `<TargetFramework>` to `net10.0-windows7.0` (matches KioChess.App)
  - Add `<Nullable>disable</Nullable>`
  - Add `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` (required by Engine)
  - Add `<BaseOutputPath>..\StockFishBuild</BaseOutputPath>` + `<BaseIntermediateOutputPath>obj</BaseIntermediateOutputPath>`
  - Add project references: Engine, Tools.Common, StockfishApp, DataAccess, Engine.Communication
  - Add NuGet: `Newtonsoft.Json 13.0.3`

  Final `StockfishBenchmark.csproj`:
  ```xml
  <Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
	  <OutputType>WinExe</OutputType>
	  <TargetFramework>net10.0-windows7.0</TargetFramework>
	  <Nullable>disable</Nullable>
	  <ImplicitUsings>enable</ImplicitUsings>
	  <UseWPF>true</UseWPF>
	  <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
	  <BaseOutputPath>..\StockFishBuild</BaseOutputPath>
	  <BaseIntermediateOutputPath>obj</BaseIntermediateOutputPath>
	</PropertyGroup>
	<ItemGroup>
	  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
	</ItemGroup>
	<ItemGroup>
	  <ProjectReference Include="..\Engine\Engine.csproj" />
	  <ProjectReference Include="..\Tools.Common\Tools.Common.csproj" />
	  <ProjectReference Include="..\StockfishApp\StockfishApp.csproj" />
	  <ProjectReference Include="..\DataAccess\DataAccess.csproj" />
	  <ProjectReference Include="..\Engine.Communication\Engine.Communication.csproj" />
	</ItemGroup>
  </Project>
  ```

- [x] **1.2** Update `App.xaml`
  - Root element changed from `Application` to `common:UiApp` (Prism base)
  - Removed `StartupUri` (Prism handles shell creation via `CreateShell()`)
  - DataTemplate entries for ViewModels will be added in Phase 4 once ViewModels exist

- [x] **1.3** Update `App.xaml.cs`
  - Extends `UiApp` (Prism `PrismApplication` subclass from UI.Common)
  - `CreateShell()` returns `new MainWindow()`
  - `DbConnect()` — connects AppDbService, initialises `MoveHashSequenceHasher`, calls `cacheLoader.LoadAsync()` (non-blocking; BenchmarkRunner calls `WaitToData()` before starting)
  - `DbDisconnect()` — disconnects AppDbService
  - `RegisterLocalTypes()` — empty placeholder for Phase 4 ViewModels

- [x] **1.4** Update `MainWindow.xaml`
  - Replaced empty `Grid` with `ContentControl Content="{Binding CurrentView}"`
  - Title, size (1280×860), and `CenterScreen` set

- [x] **1.5** Add `TtCount` accessor to `Engine\Strategies\Base\StrategyBase.cs`
  - Added `public int TtCount => Table.Count;` immediately after the `protected readonly TranspositionTable Table;` field
  - Only change to the Engine project

---

### Phase 2 — Data Models

All files go in `StockfishBenchmark/Models/`.

- [x] **2.1** Create `Models/BenchmarkSession.cs`
  ```csharp
  namespace StockfishBenchmark.Models;

  public class BenchmarkSession
  {
	  public string Strategy { get; set; }      // "lmr" | "lmrd" | "id" | "asp"
	  public short  Depth    { get; set; }      // 8–11
	  public string Color    { get; set; }      // "w" = engine Black | "b" = engine White
	  public int    MoveCount { get; set; }     // number of engine moves to record
	  public short  StockfishDepth { get; set; }
	  public int    StockfishElo   { get; set; }
	  public DateTime RunDate        { get; set; }
	  public string   MachineName    { get; set; }
	  public int      ProcessorCount { get; set; }
	  public string   RuntimeVersion { get; set; }

	  public string DisplayName =>
		  $"{Strategy.ToUpper()} d{Depth} {(Color == "b" ? "White" : "Black")} " +
		  $"{MoveCount}moves {RunDate:yyyy-MM-dd HH:mm}";
  }
  ```

- [x] **2.2** Create `Models/BenchmarkMoveRecord.cs`
  ```csharp
  namespace StockfishBenchmark.Models;

  public class BenchmarkMoveRecord
  {
	  public int    MoveNumber      { get; set; }  // 1-based sequential ply
	  public bool   IsEngineMove    { get; set; }  // false = Stockfish played this ply
	  public string UciMove         { get; set; }  // e.g. "e2e4"
	  public string MoveNotation    { get; set; }  // e.g. "e4"  (from ToLightString())
	  public double DurationMs      { get; set; }  // elapsed ms; 0 for Stockfish plies
	  public double ProcessMemoryMB { get; set; }  // WorkingSet64 / 1M after the move
	  public int    TtCount         { get; set; }  // strategy.TtCount; 0 for Stockfish plies
	  public int    MaterialValue   { get; set; }  // Position.GetValue()
	  public int    StaticValue     { get; set; }  // Position.GetStaticValue()
  }
  ```

- [x] **2.3** Create `Models/BenchmarkSummary.cs`
  ```csharp
  namespace StockfishBenchmark.Models;

  public class BenchmarkSummary
  {
	  public int    TotalEngineMoves  { get; set; }
	  public double TotalDurationMs   { get; set; }
	  public double AvgDurationMs     { get; set; }
	  public double MinDurationMs     { get; set; }
	  public double MaxDurationMs     { get; set; }
	  public double StdDevDurationMs  { get; set; }
	  public double MedianDurationMs  { get; set; }
	  public int    FinalTtCount      { get; set; }
	  public int    MaxTtCount        { get; set; }
	  public double AvgTtCount        { get; set; }
	  public double MaxProcessMemoryMB { get; set; }
	  public double AvgProcessMemoryMB { get; set; }
  }
  ```

- [x] **2.4** Create `Models/BenchmarkResult.cs`
  ```csharp
  using Newtonsoft.Json;
  namespace StockfishBenchmark.Models;

  public class BenchmarkResult
  {
	  public string           Version { get; set; } = "1.0";
	  public BenchmarkSession Session { get; set; }
	  public List<BenchmarkMoveRecord> Moves { get; set; } = new();
	  public BenchmarkSummary Summary { get; set; }

	  public static BenchmarkSummary ComputeSummary(List<BenchmarkMoveRecord> moves)
	  {
		  var eng = moves.Where(m => m.IsEngineMove).ToList();
		  if (eng.Count == 0) return new BenchmarkSummary();
		  var durations = eng.Select(m => m.DurationMs).OrderBy(d => d).ToList();
		  double avg = durations.Average();
		  double variance = durations.Sum(d => (d - avg) * (d - avg)) / durations.Count;
		  return new BenchmarkSummary
		  {
			  TotalEngineMoves   = eng.Count,
			  TotalDurationMs    = durations.Sum(),
			  AvgDurationMs      = avg,
			  MinDurationMs      = durations.First(),
			  MaxDurationMs      = durations.Last(),
			  StdDevDurationMs   = Math.Sqrt(variance),
			  MedianDurationMs   = durations[durations.Count / 2],
			  FinalTtCount       = eng.Last().TtCount,
			  MaxTtCount         = eng.Max(m => m.TtCount),
			  AvgTtCount         = eng.Average(m => m.TtCount),
			  MaxProcessMemoryMB = moves.Max(m => m.ProcessMemoryMB),
			  AvgProcessMemoryMB = moves.Average(m => m.ProcessMemoryMB)
		  };
	  }
  }
  ```

- [x] **2.5** Create `Models/ComparisonRow.cs`
  ```csharp
  namespace StockfishBenchmark.Models;

  public class ComparisonRow
  {
	  public int    MoveNumber      { get; set; }
	  public string UciMove         { get; set; }
	  public string MoveNotation    { get; set; }
	  public double BaselineDurationMs { get; set; }
	  public double NewDurationMs      { get; set; }
	  public double DeltaMs   => NewDurationMs - BaselineDurationMs;
	  public double DeltaPercent => BaselineDurationMs > 0
		  ? (NewDurationMs - BaselineDurationMs) / BaselineDurationMs * 100.0
		  : 0.0;
	  public int    BaselineTtCount { get; set; }
	  public int    NewTtCount      { get; set; }
	  public int    DeltaTtCount    => NewTtCount - BaselineTtCount;
	  public double BaselineMemoryMB { get; set; }
	  public double NewMemoryMB      { get; set; }
	  public double DeltaMemoryMB    => NewMemoryMB - BaselineMemoryMB;
	  public bool   IsRegression { get; set; }  // engine chose a different move
	  public bool   IsDiverged   { get; set; }  // position diverged due to earlier regression
	  public string Status => IsDiverged  ? "DIVERGED"
							: IsRegression? "REGRESSION"
							: DeltaPercent < -5.0 ? "FASTER"
							: DeltaPercent >  5.0 ? "SLOWER"
							: "SAME";
  }
  ```

---

### Phase 3 — Services

All files go in `StockfishBenchmark/Services/`.

- [x] **3.1** Create `Services/IBenchmarkFileService.cs`
  ```csharp
  namespace StockfishBenchmark.Services;

  public interface IBenchmarkFileService
  {
	  void Save(BenchmarkResult result, string filePath = null);
	  BenchmarkResult Load(string filePath);
	  string GetDefaultPath(BenchmarkSession session);
	  // Default: Benchmarks\{strategy}_d{depth}_{color}_{yyyyMMdd_HHmmss}.json
  }
  ```

- [x] **3.2** Create `Services/BenchmarkFileService.cs`
  ```csharp
  public class BenchmarkFileService : IBenchmarkFileService
  {
	  private const string Folder = "Benchmarks";

	  public string GetDefaultPath(BenchmarkSession s)
	  {
		  Directory.CreateDirectory(Folder);
		  var name = $"{s.Strategy}_d{s.Depth}_{s.Color}_{s.RunDate:yyyyMMdd_HHmmss}.json";
		  return Path.Combine(Folder, name);
	  }

	  public void Save(BenchmarkResult result, string filePath = null)
	  {
		  filePath ??= GetDefaultPath(result.Session);
		  Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
		  File.WriteAllText(filePath, JsonConvert.SerializeObject(result, Formatting.Indented));
	  }

	  public BenchmarkResult Load(string filePath)
		  => JsonConvert.DeserializeObject<BenchmarkResult>(File.ReadAllText(filePath));
  }
  ```

- [x] **3.3** Create `Services/IBenchmarkRunner.cs`
  ```csharp
  namespace StockfishBenchmark.Services;

  public interface IBenchmarkRunner
  {
	  Task<BenchmarkResult> RunAsync(
		  BenchmarkSession session,
		  IProgress<BenchmarkMoveRecord> progress,
		  CancellationToken cancellationToken = default);
  }
  ```

- [x] **3.4** Create `Services/BenchmarkRunner.cs`

  This is the core game loop. Adapt from `StockFishGame.Play()`:

  ```
  Algorithm:
  ──────────
  1. Create Stockfish(@"stockfish path", session.StockfishDepth, session.StockfishElo)
  2. Create Position; apply any opening moves (none for a fresh game)
  3. Create strategy = strategyFactory.GetStrategy(session.Depth, position, session.Strategy)
  4. Determine isStockfishMove from session.Color and Position.GetTurn()
	 isStockfishMove = (session.Color == "w" && turn == White)
					|| (session.Color == "b" && turn == Black)
  5. engineMoveCount = 0, plyNumber = 1
  6. Loop while engineMoveCount < session.MoveCount && !cancelled:
	 a. fen = Stockfish.GetFenPosition()
	 b. If isStockfishMove:
		  bestUci = Stockfish.GetBestMove()
		  move    = Position.GetAllMoves().First(m => m.ToUciString() == bestUci)
		  ApplyMove(position, stockfish, fen, move)
		  record  = new BenchmarkMoveRecord {
			  MoveNumber=plyNumber, IsEngineMove=false,
			  UciMove=bestUci, MoveNotation=move.ToLightString(),
			  DurationMs=0, ProcessMemoryMB=SampleMemory(),
			  TtCount=0, MaterialValue=position.GetValue(), StaticValue=position.GetStaticValue() }
		Else (engine move):
		  sw.Restart(); result = strategy.GetResult(); sw.Stop()
		  if result.Move == null → handle game over, break
		  ApplyMove(position, stockfish, fen, result.Move)
		  engineMoveCount++
		  record = new BenchmarkMoveRecord {
			  MoveNumber=plyNumber, IsEngineMove=true,
			  UciMove=result.Move.ToUciString(), MoveNotation=result.Move.ToLightString(),
			  DurationMs=sw.Elapsed.TotalMilliseconds, ProcessMemoryMB=SampleMemory(),
			  TtCount=strategy.TtCount, MaterialValue=position.GetValue(), StaticValue=position.GetStaticValue() }
	 c. moves.Add(record); progress.Report(record); plyNumber++
	 d. isStockfishMove = !isStockfishMove
  7. benchmarkResult.Moves   = moves
  8. benchmarkResult.Summary = BenchmarkResult.ComputeSummary(moves)
  9. return benchmarkResult

  Helper:
	ApplyMove(position, stockfish, fen, move):
	  if position.GetHistory().Any() → position.Make(move)
	  else → position.MakeFirst(move)
	  stockfish.SetPosition(fen, move.ToUciString())

	SampleMemory():
	  return Process.GetCurrentProcess().WorkingSet64 / 1_048_576.0
  ```

- [x] **3.5** Create `Services/IReplayRunner.cs`
  ```csharp
  namespace StockfishBenchmark.Services;

  public interface IReplayRunner
  {
	  Task<BenchmarkResult> ReplayAsync(
		  BenchmarkResult baseline,
		  IProgress<(BenchmarkMoveRecord Record, bool IsRegression)> progress,
		  CancellationToken cancellationToken = default);
  }
  ```

- [x] **3.6** Create `Services/ReplayRunner.cs`

  ```
  Algorithm:
  ──────────
  1. Extract session from baseline.Session
  2. Create fresh Position
  3. Create strategy = strategyFactory.GetStrategy(session.Depth, position, session.Strategy)
  4. diverged = false
  5. For each record in baseline.Moves (in order):
	 a. Retrieve MoveBase matching record.UciMove from position.GetAllMoves()
		(if not found and game is over → break)
	 b. If !record.IsEngineMove:
		  position.Make(move) [or MakeFirst for first move]
		  newRecord = copy of record with fresh ProcessMemoryMB, TtCount=0
		  progress.Report((newRecord, false))
		Else:
		  if !diverged:
			sw.Restart(); result = strategy.GetResult(); sw.Stop()
			engineUci  = result.Move?.ToUciString() ?? ""
			isRegression = engineUci != record.UciMove
			if isRegression → diverged = true
			// Apply the engine's ACTUAL move (not the saved one) to continue naturally
			actualMove = isRegression ? result.Move : move
			position.Make(actualMove)
			newRecord = new BenchmarkMoveRecord {
				MoveNumber=record.MoveNumber, IsEngineMove=true,
				UciMove=engineUci, MoveNotation=result.Move?.ToLightString() ?? "?",
				DurationMs=sw.Elapsed.TotalMilliseconds, ProcessMemoryMB=SampleMemory(),
				TtCount=strategy.TtCount, MaterialValue=position.GetValue(),
				StaticValue=position.GetStaticValue() }
			progress.Report((newRecord, isRegression))
		  else:
			// Position has diverged — record stub with IsDiverged flag
			// (we must still advance position by applying the saved move if possible)
			// Just emit a null/zero record flagged as diverged
  6. Compute new Summary from new move records
  7. Return new BenchmarkResult with same Session (RunDate = DateTime.UtcNow) + new Moves + new Summary
  ```

---

### Phase 4 — ViewModels

All files go in `StockfishBenchmark/ViewModels/`.

- [x] **4.1** ~~Create `ViewModels/ViewModelBase.cs`~~ — replaced by Prism `BindableBase`
  ```csharp
  using System.ComponentModel;
  using System.Runtime.CompilerServices;

  namespace StockfishBenchmark.ViewModels;

  public abstract class ViewModelBase : INotifyPropertyChanged
  {
	  public event PropertyChangedEventHandler PropertyChanged;

	  protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
	  {
		  if (EqualityComparer<T>.Default.Equals(field, value)) return false;
		  field = value;
		  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		  return true;
	  }

	  protected void OnPropertyChanged([CallerMemberName] string name = "")
		  => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }
  ```

- [x] **4.2** ~~Create `ViewModels/RelayCommand.cs`~~ — replaced by Prism `DelegateCommand`
  ```csharp
  using System.Windows.Input;

  namespace StockfishBenchmark.ViewModels;

  public class RelayCommand : ICommand
  {
	  private readonly Action<object> _execute;
	  private readonly Func<object, bool> _canExecute;

	  public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
	  {
		  _execute    = execute;
		  _canExecute = canExecute;
	  }

	  public event EventHandler CanExecuteChanged
	  {
		  add    => CommandManager.RequerySuggested += value;
		  remove => CommandManager.RequerySuggested -= value;
	  }

	  public bool CanExecute(object p) => _canExecute == null || _canExecute(p);
	  public void Execute(object p)    => _execute(p);

	  public void Refresh() => CommandManager.InvalidateRequerySuggested();
  }
  ```

- [x] **4.3** Create `ViewModels/MainWindowViewModel.cs`
  ```csharp
  namespace StockfishBenchmark.ViewModels;

  public class MainWindowViewModel : ViewModelBase
  {
	  private ViewModelBase _currentView;
	  public ViewModelBase CurrentView
	  {
		  get => _currentView;
		  set => SetProperty(ref _currentView, value);
	  }

	  public MainWindowViewModel()
	  {
		  Navigate(new SetupViewModel(Navigate));
	  }

	  public void Navigate(ViewModelBase vm) => CurrentView = vm;
  }
  ```

- [x] **4.4** Create `ViewModels/SetupViewModel.cs`

  Properties to expose:
  - `string[] Strategies` = `{ "lmr", "lmrd", "id", "asp" }`
  - `string SelectedStrategy` (default `"lmr"`)
  - `int[] Depths` = `{ 8, 9, 10, 11 }`
  - `int SelectedDepth` (default `10`)
  - `string[] Colors` = `{ "Engine plays White  (Color=b)", "Engine plays Black  (Color=w)" }`
  - `string SelectedColorLabel` → maps to `Color` = `"b"` or `"w"`
  - `int MoveCount` (default `24`, validate 1–100)
  - `int StockfishDepth` (default `5`, validate 2–15)
  - `int StockfishElo` (default `1500`, validate 1320–3190)
  - `bool IsNewRun` / `bool IsLoadMode` (radio binding)
  - `string LoadedFilePath` (read-only TextBox)
  - `string ValidationMessage`

  Commands:
  - `StartCommand` → validate → build `BenchmarkSession` → navigate to `RunViewModel(session, navigate)`
  - `LoadFileCommand` → `OpenFileDialog` filter `*.json` → load `BenchmarkResult`
	→ navigate to `RunViewModel(baseline, navigate)` in Replay mode

- [x] **4.5** Create `ViewModels/RunViewModel.cs`

  Constructor variants:
  - `RunViewModel(BenchmarkSession session, Action<ViewModelBase> navigate)` → Mode A (new run)
  - `RunViewModel(BenchmarkResult baseline, Action<ViewModelBase> navigate)` → Mode B (replay)

  Properties:
  - `ObservableCollection<BenchmarkMoveRecord> LiveMoves`
  - `string StatusText` ("Ready", "Running…", "Complete", "Cancelled", "Error: …")
  - `double ProgressPercent` (0–100, based on engineMoveCount / session.MoveCount)
  - `string ElapsedText` — updated by a `DispatcherTimer` while running
  - `string AvgDurationText` / `string MinDurationText` / `string MaxDurationText`
	(updated after each engine move from running stats)
  - `BenchmarkResult CurrentResult` (null until complete)
  - `bool CanSave` / `bool CanCompare`

  Commands:
  - `StartCommand` (auto-invoked on VM load)
  - `CancelCommand` → `_cts.Cancel()`
  - `SaveCommand` (enabled when CanSave) → `BenchmarkFileService.Save(CurrentResult)`
	→ shows saved path in StatusText
  - `CompareCommand` (enabled when CanCompare) → navigate to `CompareViewModel`

  Lifecycle: `RunViewModel` calls `Task.Run(RunAsync)` on construction (or via StartCommand).
  `IProgress<BenchmarkMoveRecord>` implementation updates `LiveMoves` via
  `Application.Current.Dispatcher.InvokeAsync`.

- [x] **4.6** Create `ViewModels/CompareViewModel.cs`

  Constructor: `CompareViewModel(BenchmarkResult baseline, BenchmarkResult newRun, Action<ViewModelBase> navigate)`
  Also supports loading either file independently via commands.

  Properties:
  - `BenchmarkResult Baseline` / `BenchmarkResult NewRun`
  - `ObservableCollection<ComparisonRow> Rows`
  - `string BaselineInfo` → `baseline.Session.DisplayName`
  - `string NewRunInfo`   → `newRun.Session.DisplayName`
  - `string AvgDeltaText` → `$"{avgDelta:+0.0;-0.0}%"`
  - `string TotalTimeDeltaText` → formatted ms delta
  - `int RegressionCount`
  - `bool HasRegressions`
  - Summary stats: `BaselineAvg`, `NewAvg`, `BaselineMax`, `NewMax`, `BaselineTtAvg`, `NewTtAvg`

  Commands:
  - `LoadBaselineCommand` → OpenFileDialog → reload `Baseline` + rebuild rows
  - `LoadNewRunCommand`   → OpenFileDialog → reload `NewRun` + rebuild rows
  - `BackCommand`         → navigate back to `SetupViewModel`
  - `ExportCsvCommand`    → SaveFileDialog `.csv` → write header + all rows

  `BuildRows()` method:
  ```
  Zip engine-only moves from Baseline and NewRun by index.
  For each pair (baseRec, newRec):
	  row.MoveNumber      = baseRec.MoveNumber
	  row.UciMove         = baseRec.UciMove
	  row.MoveNotation    = baseRec.MoveNotation
	  row.BaselineDurationMs = baseRec.DurationMs
	  row.NewDurationMs      = newRec.DurationMs
	  row.IsRegression       = newRec.UciMove != baseRec.UciMove
	  row.IsDiverged         = diverged (set true once IsRegression seen)
	  ... fill TT + memory fields
  ```

---

### Phase 5 — Views

All XAML files go in `StockfishBenchmark/Views/`.

- [x] **5.1** Create `Views/SetupView.xaml` + `SetupView.xaml.cs`

  Layout — vertical `StackPanel` inside `ScrollViewer`, 600px wide, centred:
  ```
  ┌─ Mode ──────────────────────────────────────────┐
  │  ● New Run vs Stockfish                          │
  │  ○ Load Baseline File                            │
  └──────────────────────────────────────────────────┘
  ┌─ Configuration (visible: New Run) ──────────────┐
  │  Strategy:       [ComboBox: lmr/lmrd/id/asp]    │
  │  Depth:          [ComboBox: 8/9/10/11]           │
  │  Engine Color:   [ComboBox]                      │
  │  Move Count:     [TextBox 24]                    │
  │  Stockfish Depth:[TextBox 5]                     │
  │  Stockfish ELO:  [TextBox 1500]                  │
  └──────────────────────────────────────────────────┘
  ┌─ Load Baseline (visible: Load Mode) ────────────┐
  │  File: [TextBox readonly]  [Browse...]            │
  └──────────────────────────────────────────────────┘
  [Validation message in Red]
							  [Start / Load & Replay]
  ```

  Use `GroupBox`, `Label`/`TextBox`, `ComboBox`. Bind all to `SetupViewModel`.
  Visibility of the two GroupBoxes controlled by `BooleanToVisibilityConverter`
  on `IsNewRun` / `IsLoadMode`.

- [x] **5.2** Create `Views/RunView.xaml` + `RunView.xaml.cs`

  Layout — `DockPanel`:
  ```
  [Top]    ProgressBar + StatusText + ElapsedText
  [Top]    Live stats strip: Avg | Min | Max (updates per engine move)
  [Center] DataGrid (AutoGenerateColumns=False):
			 # | Move | Type | Duration ms | TT Count | Memory MB | Material | Static
  [Bottom] [Cancel]  [Save Result]  [Open Comparison →]
  ```

  DataGrid row colour: engine moves in `AliceBlue`, Stockfish moves in default.
  `ItemsSource="{Binding LiveMoves}"` — `ObservableCollection` ensures live scroll.

- [x] **5.3** Create `Views/CompareView.xaml` + `CompareView.xaml.cs`

  Layout — `Grid` (2 rows + 1 bottom strip):

  **Row 0** — Two `GroupBox` panels side-by-side (Baseline | New Run):
  ```
  ┌─ Baseline ──────────────────────┬─ New Run ──────────────────────┐
  │ lmr  d10  Black  24 moves       │ lmr  d10  Black  24 moves      │
  │ Avg: 410 ms  Min: 187 Max: 724  │ Avg: 385 ms  Min: 175 Max: 690 │
  │ Avg TT: 178k  MaxMem: 158 MB    │ Avg TT: 178k  MaxMem: 152 MB   │
  └──────────────────────────────────┴────────────────────────────────┘
  ```

  **Row 1** — DataGrid (scrollable, fills space):
  ```
  #  | Move  | Status    | Baseline ms | New ms | Δms   | Δ%    | ΔTT   | ΔMem MB
  1  | e5    | SAME      | 410.2       | 398.1  | -12.1 | -2.9% | +1200 | +0.1
  2  | Nf6   | FASTER    | 723.5       | 615.0  | -108  | -14.9%| -500  | -2.1
  3  | d4    | REGRESSION| 300.0       | 290.0  | -10.0 | -3.3% | 0     | 0
  ```
  Row background from `DeltaToColorConverter` on the `Status` column.

  **Bottom strip**:
  ```
  [← Back]  Regressions: 0   Avg Δ: -3.2%   Total saved: 240 ms
  [Load Baseline]  [Load New Run]  [Export CSV]
  ```

- [x] **5.4** Create `Converters/DeltaToColorConverter.cs` + `Converters/MiscConverters.cs`
  ```csharp
  [ValueConversion(typeof(string), typeof(SolidColorBrush))]
  public class DeltaToColorConverter : IValueConverter
  {
	  public object Convert(object value, ...) => value?.ToString() switch
	  {
		  "FASTER"     => new SolidColorBrush(Colors.LightGreen),
		  "SLOWER"     => new SolidColorBrush(Colors.LightCoral),
		  "REGRESSION" => new SolidColorBrush(Colors.Orange),
		  "DIVERGED"   => new SolidColorBrush(Color.FromRgb(200, 200, 200)),
		  _            => Brushes.Transparent
	  };
	  public object ConvertBack(...) => throw new NotSupportedException();
  }
  ```

---

### Phase 6 — Navigation Wiring & Integration

- [ ] **6.1** Register `DeltaToColorConverter` in `App.xaml` resources
  ```xml
  <converters:DeltaToColorConverter x:Key="DeltaToColor"/>
  ```

- [ ] **6.2** Wire navigation delegates
  - `SetupViewModel` receives `Action<ViewModelBase> navigate` in constructor.
  - `StartCommand` creates `new RunViewModel(session, navigate)` and calls `navigate(vm)`.
  - `RunViewModel.CompareCommand` creates `new CompareViewModel(baseline, newRun, navigate)` and calls `navigate(vm)`.
  - `CompareViewModel.BackCommand` calls `navigate(new SetupViewModel(navigate))`.

- [ ] **6.3** Add `Benchmarks/` folder auto-creation
  - `BenchmarkFileService.Save()` calls `Directory.CreateDirectory(Folder)` before writing.
  - Default output path is relative to `AppDomain.CurrentDomain.BaseDirectory`.

- [ ] **6.4** Add `StockfishSettings.json` support
  - Create `StockfishSettings.json` in `StockfishBenchmark/` project root.
  - Mark as `Content` / `CopyToOutputDirectory=Always`.
  - Content: `{ "StockfishPath": "..\\..\\..\\stockfish\\stockfish-windows-x86-64-avx2.exe" }`
  - `BenchmarkRunner` reads this at construction via `JsonConvert.DeserializeObject`.

- [ ] **6.5** Handle game-over mid-sequence
  - If `strategy.GetResult().Move == null` or `result.GameResult != Continue`, stop the loop.
  - Mark `StatusText` as `"Complete (game over at move {n})"`.
  - Save partial result with however many moves were recorded.

---

### Phase 7 — Build & Testing

- [ ] **7.1** Build solution — ensure zero errors after all Phase 1–6 changes
  - Run `dotnet build KioChess.sln` — fix any namespace or reference issues.

- [ ] **7.2** Test Mode A — New Run
  - Launch app → Setup → New Run → Strategy=lmr, Depth=10, Engine=Black, Moves=24.
  - Verify `Benchmarks/lmr_d10_w_<date>.json` is created with correct structure.
  - Verify `Summary.TotalEngineMoves == 24`.
  - Verify `Moves` array alternates `IsEngineMove` correctly.

- [ ] **7.3** Test JSON roundtrip
  - Load saved JSON file via `BenchmarkFileService.Load()`.
  - Verify all fields deserialise without loss.
  - Verify `Session.Strategy`, `Depth`, `Color` match what was configured.

- [ ] **7.4** Test Mode B — Replay (no code changes)
  - Load the JSON from 7.2 → Replay.
  - Expected: `RegressionCount == 0`, all rows `Status == "SAME"`.
  - Expected: New avg duration within ±20% of baseline (system jitter only).
  - Expected: TT counts identical to baseline for every engine move.

- [ ] **7.5** Test regression detection
  - Temporarily change one evaluation constant in the Engine project.
  - Re-run replay from same baseline file.
  - Expected: First affected move shows `Status == "REGRESSION"`.
  - Expected: All subsequent moves show `Status == "DIVERGED"`.
  - Revert the evaluation change.

- [ ] **7.6** Test all strategies
  - Repeat 7.2–7.4 for `lmrd`, `id`, `asp`.
  - Repeat with Depth 8, 9, 11.

- [ ] **7.7** Test cancellation
  - Start a run → Cancel after ~5 moves.
  - Verify partial `BenchmarkResult` with however many moves are available.
  - Verify UI shows "Cancelled" status cleanly.

- [ ] **7.8** Test Export CSV
  - Run comparison view → Export CSV.
  - Open in Excel → verify all columns present, values match UI.

---

## Open Questions & Risks

| # | Topic | Decision / Note |
|---|---|---|
| 1 | Stockfish binary location | Configurable via `StockfishSettings.json`; falls back to hardcoded relative path |
| 2 | DB requirement in Boot | `Boot.SetUp()` requires `IAppDbService` + `GetAllMoveHashValues()`; ensure DB is available or make optional with empty hash |
| 3 | TT between moves | TT persists across moves in the same game — intentional for determinism between live run and replay |
| 4 | First regression → diverged | After first regression all subsequent positions are different; these rows are marked DIVERGED and timing comparisons are meaningless |
| 5 | Config files | `Config\Configuration.json` and `Config\StaticTables.json` must be in output dir — they copy via Engine project automatically |
| 6 | Partial save on cancel | Currently planned: save whatever moves were recorded (partial result) |
| 7 | Opening pre-moves | For future: support loading a sequence of pre-played opening moves before the benchmark starts; stored in `Session.OpeningMoves[]` |
| 8 | `StrategyBase.Table` access | Single line change in Engine: `public int TtCount => Table.Count;` on `StrategyBase` |

---

## Quick-Reference: Strategy Codes

| Code | Class | Notes |
|---|---|---|
| `lmr` | `LmrStrategyBase` | Late Move Reduction |
| `lmrd` | `LmrDeepStrategy` | LMR Deep variant |
| `id` | `IteretiveDeepingStrategy` | Iterative Deepening |
| `asp` | `AspirationStrategy` | Aspiration Windows |

Registered in `StrategyFactory._strategyFactories`.

---

*Last updated: initial draft*
