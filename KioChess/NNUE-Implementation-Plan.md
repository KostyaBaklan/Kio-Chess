# NNUE Implementation Plan (KioChess) - revision 2

Status: design only. No engine code has been changed.
Target hardware: Intel i5-14600K (6P + 8E cores, 20 threads, AVX2, no AVX-512), one home PC, CPU-only training.
Data source: monthly Lichess database dumps (`lichess_db_standard_rated_YYYY-MM.pgn.zst`), human games only (BOT accounts excluded).
Label source (decision): **KioChess's own search score** (fixed-depth search on HCE). Stockfish `%eval` is not used.
Runtime constraint: every job must finish (or checkpoint) within **18 hours**.

---

## 1. Deep analysis

### 1.1 Review of the previous plan (rev. 1)
| Item in rev. 1 | Finding | Change in rev. 2 |
|---|---|---|
| "Phase 0 done: `NnuePath` added" | **Not true in the current branch.** `NnuePath` exists in no `.cs`/`.json` file. | Phase 0 re-designed: dedicated `NnueConfiguration` node and types instead of a field in `GeneralConfiguration`. |
| Label = static HCE `Board.Evaluate()` | Distilling the static HCE only reproduces the HCE (maybe faster, not stronger). | Label = own engine **search** score at fixed depth (HCE + lookahead), mixed with game result. |
| White-relative network, no side to move | Loses tempo; all modern NNUEs are side-to-move relative. `Position` knows the turn. | Side-to-move (STM) perspective ordering. |
| Comments ignored by the PGN parser | `PgnStreamReader` and `PgnReader` skip `{...}`. | Not an issue: `%eval` is not used, the existing `PgnStreamReader` is reused. |
| Hidden H = 128, 100-byte record | 128 is small for 100M+ positions; 100 bytes/record = 10 GB per 100M. | H = 256 (AVX2 friendly), packed 32-byte record. |
| Parallelism: "several processes on separate files" | A monthly dump is one 200+ GB PGN, so file-level parallelism gives 1 worker. | Byte-range sharding of one file across N worker processes. |
| Dense `[B,768]` float input | ~96% zeros; wastes CPU. | Sparse indices + `EmbeddingBag(sum)` in TorchSharp. |
| Search margins | Not mentioned. | Futility/RFP/razoring/delta/aspiration margins are tuned to HCE scale; calibration + retune phase added. |

### 1.2 Evaluation entry points (confirmed)
| Member | File | Notes |
|---|---|---|
| `Position.GetValue()` | `Engine/Models/Boards/Position.cs` | STM-relative: white -> `Board.Evaluate()`, black -> `Board.EvaluateOpposite()`. Used by search/qsearch/futility (`StrategyBase.cs` ~l.1295). |
| `Position.GetStaticValue()` | same | Material only, used by `ComplexSorter`. Not affected by NNUE. |
| `Board.Evaluate()/EvaluateOpposite()` | `Engine/Models/Boards/Evaluation/Board.Evaluation.cs` | Select `_evaluationService` by phase, then `EvaluateCached(phase)`; opposite = negation. |
| `Board.EvaluateCached(phase)` | same | Direct-mapped cache; key = Zobrist `Hash`, meta = `phase | castleRights`; stores a **white-relative** value. |
| `Board.ComputeEvaluation(phase)` | same | `ComputeAttacks()` + `EvaluateOpening/Middle/End()`. NNUE switch point. |

### 1.3 Board mutation (confirmed)
- `Board.Manipulation.cs`: `AddWhite/RemoveWhite/MoveWhite/AddBlack/RemoveBlack/MoveBlack`. Each XORs `Hash`, updates `_boards[piece]`, `Whites/Blacks`, `Occupied`, `Empty`.
- The only other writer of `_boards` is the start-position setup in `Board.Initialization.cs` (~l.866-895). No FEN loader exists.
- All move types (captures, en passant, promotions, castling) Make/UnMake through these six primitives, so **6 hooks cover all accumulator updates**. Integer add/sub is exactly reversible, so no accumulator stack is required in v1.

### 1.4 Phase and side to move
- `MoveHistoryService` (singleton) sets phase by ply: plies < 16 Opening, 16..35 Middle, from ply 36 `Board.IsEndGame() ? End : Middle`. Phase is history dependent; NNUE ignores it. HCE parts kept with NNUE (mop-up, rule-50 scaling) still use it.
- `Board` has no turn; `Position._turn` has it. The eval cache stores white-relative values shared by both colors. With an STM net, `eval(black to move) != -eval(white to move)` for the same placement, so **turn must be part of the cache meta** (or NNUE bypasses the cache).

### 1.5 Global/static state (limits parallelism)
- `MoveBase.Board` is `public static`, `MoveBase._figureHistory` is static, `ContainerLocator.Current` is global, `MoveHistoryService` is a singleton.
- Consequence: **one `Position` per process**. Data generation is parallelized with worker processes, not threads.

### 1.6 Existing PGN pipeline
- `PgnManager` starts **one `PgnTool.exe` process per game**; each start runs `Boot.SetUp()` (container, config, DB). Tens of ms per game x ~90M games/month = unusable for NNUE data.
- Reusable: `PgnConverter.ProcessMove(position, move, isWhite)` (move resolution), `Boot.SetUp()`, `Position`, `PgnGameTextStreamer` ideas (Elo pre-filter).
- Not reusable: process-per-game model, comment-dropping move parser.

### 1.7 Label design: "my own evaluation"
- The network must learn **KioChess's** judgement, not Stockfish's. Two own-engine options:
  - Static HCE (`Board.Evaluate()`): the net only imitates the HCE. Useful for speed, little or no Elo.
  - **Fixed-depth search score** (chosen): the score of a depth-d search on the HCE. The net learns "what my engine finds after d plies" as a static function. This is how self-trained NNUEs gain strength over their HCE.
- Target mix: `t = ? * sigmoid(search / 400) + (1 - ?) * wdl`, ? = 0.75 (game result corrects systematic HCE errors).
- Later iterations: label with net N (search on NNUE) to train net N+1.

### 1.8 Lichess data and Elo (approximate)
- Monthly dump: ~90-100M games, ~30+ GB `.zst`, ~200+ GB decompressed. Current book filter (Elo >= 2600) gives ~500k games/month.
- With search labels the **bottleneck is labeling time, not games**:
  - ~15-20 usable positions per game (ply >= 16, quiet, not in check, 50% sampling).
  - Search labeling throughput (estimate, measure first): depth 6 ~ 1.5-3M positions/hour, depth 8 ~ 0.7-1.8M positions/hour with 16 processes.
  - One 18-hour run at depth 8 => ~12-30M positions.
- Elo filter therefore controls **position quality/diversity**, not volume:
  - 2600+: 500k games -> 7-10M positions/month. Enough for one labeling run per month, but positions are narrow (mostly balanced, theory-heavy).
  - **2200+ (recommended)**: several million games/month; sample ~1-2M games per labeling run. More varied middlegames/endgames, results still reliable for the WDL part.
  - Below 2000: noisy results and odd positions; not needed since volume is not the limit.
- Bot exclusion: skip a game if `WhiteTitle` or `BlackTitle` is `BOT`.
- `.zst` can be streamed, but cannot seek; decompress once to SSD for sharding.

### 1.9 Search for labeling (confirmed API)
- `IStrategyFactory.GetStrategy(short depth, Position position, string code)` -> `StrategyBase.GetResult()` -> `IResult.Value` / `IResult.Move`.
- To verify before coding: `Result.Value` sign convention (side to move expected), opening book bypass (`GetResult` uses book/popular moves at low ply; labeling must use raw search), TT/history reset between positions, `MoveHistoryService` state after search (must be restored to the game ply).

### 1.10 Search interactions
- `MarginConfiguration` (Futility 200/450, RFP 150..775, Razoring 700, Delta 375, AttackMargin), `AspirationWindow` 125, `TradeMargin` are tuned to HCE scale. NNUE is calibrated to pawn ~ 100 via `SCALE`, but variance differs: retune after the first net.
- `GetMopUpValue()` and rule-50 scaling: small nets are weak at converting KQ/KR vs K and at drawish scaling. Keep these HCE terms on top of NNUE in phase End (hybrid).
- Depth ~10: NNUE changes leaf quality at every node, no depth-specific assumptions. Expected NPS: AVX2 inference with H=256 is likely comparable to `ComputeAttacks()` + full HCE; incremental updates add 2-4 row ops per make/unmake. Measure, do not assume.

---

## 2. Architecture

### 2.1 Overview
```
lichess_db_*.pgn(.zst)
   |  Pass 1  NnueTool filter  (text scan: Elo >= 2200, no BOT, valid result, time control) -> games-YYYY-MM.pgn
   |  Pass 2  NnueTool extract (N processes: replay, position filters, dedupe) -> positions-XX.bin (unlabeled)
   |  Pass 3  NnueTool label   (N processes: fixed-depth own search, resumable, <= 18 h per run) -> labeled-XX.bin
   |  Pass 4  NnueTool merge   (dedupe, shuffle, train/val split)
   v
data/train.bin, data/val.bin
   |  NnueTrainer train  (TorchSharp-cpu, sparse EmbeddingBag)
   v
nets/kio-XXX.nnue  (quantized)  -->  NnueConfiguration.NetworkPath (+ IsEnabled)
   |
   v
Engine: Nnue/NnueNetwork (loader + AVX2) + Board.Nnue (accumulators) + Evaluate switch
```
Projects:
Commands: `filter`, `extract`, `label`, `merge`, `verify`, `stats`.
- `NnueTrainer` (console, net10.0): refs **only** `TorchSharp-cpu`. Engine stays dependency free.

### 2.2 Network v1
- Inputs: 768 = 12 pieces x 64 squares; two perspectives share one feature transformer (FT).
  - White perspective: `idx = piece * 64 + sq`
  - Black perspective: `idx = ((piece + 6) % 12) * 64 + (sq ^ 56)`
- FT: 768 -> 256 (int16). Output: `concat(acc[stm], acc[!stm])` (512) -> SCReLU -> 1.
- Quantization: `QA = 255`, `QB = 64`, `SCALE = 400`; `cp = (? clamp(acc,0,QA)^2 * w / QA + b) * SCALE / (QA * QB)`.
`wdl ? {0, 0.5, 1}`, `score` = own search score, ? = 0.75 (tunable in the trainer without regenerating data).
- Upgrades only after v1 beats HCE: H = 512, king buckets, output buckets by piece count.

### 2.3 Training record (32 bytes, little endian)
```
ulong    occupancy   8   occupied squares (a1 = bit 0, real board)
byte[16] pieces     16   4-bit piece code per occupied square in bit order (max 32 pieces)
short    score       2   STM-relative cp of own search, clamped to +-3000; short.MinValue = not labeled yet
byte     wdl         1   0 loss / 1 draw / 2 win, STM POV
byte     stm         1   0 white / 1 black
byte     ply         1
byte     depth       1   label search depth (0 = unlabeled)
ushort   moveIndex   2   index of the game ply, used to rebuild the position for labeling (see 2.4)
```
100M records = 3.2 GB; memory-mappable. The trainer applies the perspective mapping, so the file format is independent of the network layout.

### 2.4 Data generation (multi-pass, each pass resumable)
**Pass 1 - filter** (text only, no replay, ~1 h/month, disk bound):
- Both Elo >= 2200 (CLI `--min-elo`), no `BOT` title, result in {1-0, 0-1, 1/2-1/2}, `Termination` Normal/Time forfeit, base time >= 180 s (blitz+; bullet positions are noisy).
- Output: one filtered PGN per month (expected a few GB).

**Pass 2 - extract** (replay, minutes per month):
- Ply >= 16; side to move not in check; next played move is not a capture/promotion; keep with probability p (default 0.5).
- Dedupe with a Bloom filter on `Hash` + turn (openings repeat heavily even after ply 16).
- Output: unlabeled records (score = `short.MinValue`).

**Pass 3 - label** (the expensive pass):
- Problem: the engine has no FEN loader, so a stored record cannot be loaded into `Position` directly. Two options:
  - **A (v1, recommended): label during replay.** Pass 2 and 3 are one process: replay the game, and for each selected position run the search at that point. No board loader needed; the move history (phase, castling, repetition) is exact.
  - B (later): add `Board.SetPosition(record)` to label stored records independently of games. Needs hash, castle rights, phase and accumulator rebuild; more risk.
- Search: `lmrd` strategy, fixed depth 6 for the first net (more positions), depth 8 for later nets; NNUE off; book disabled; TT cleared per game (not per position: neighbours in a game share useful TT entries).
- Skip mate scores and |score| > 3000; also skip if the search best move is a capture (position not quiet at depth 0).
- Workers write output in chunks and a `done-games.txt` offset, so an interrupted run resumes; a run is stopped by `--max-hours 17`.

Parallelism: 16 worker processes (6P + 8E cores; E-cores are slower, so give each worker its own game range and let fast workers take more ranges from a shared queue file).

Budget per 18-hour run (estimate, measure first): depth 6 => ~25-50M positions; depth 8 => ~12-30M positions.

### 2.5 Training
- TorchSharp-cpu; test `torch.set_num_threads` 6/12/20 (P-cores usually win).
- Loader: memory-mapped `train.bin`, block shuffle per epoch, decode to `long` index tensors + offsets for `EmbeddingBag`.
- Loss MSE on sigmoid space; AdamW lr 1e-3 cosine to 1e-5; batch 16384; clip FT and output weights to [-1.98, 1.98] after each step (keeps quantized output |w| <= 127 so `v*w` fits int16).
- First net: 30-50M positions (one or two depth-6 labeling runs) x 15-20 epochs. Time estimate (unmeasured): 1-3 min per 10M positions per epoch => well under 18 h. Benchmark one epoch before planning larger runs.
- Growth: accumulate labeled data over runs/months (100M+ positions for a well-trained 256 net).
- Validation on `val.bin` (1%); checkpoint + quantized export per epoch.

### 2.6 Engine integration
New files: `Engine/Nnue/NnueNetwork.cs`, `Engine/Models/Boards/Board.Nnue.cs`.
`NnueConfiguration` node (`IsEnabled = false` = current HCE, no behavior change). All values in sections 2.2-2.5 are defaults of that node (see Phase 0).
Hot path: no allocations, pre-allocated accumulators, one predictable `_nnue != null` branch per primitive.

### 2.7 Validation
- `NnueTool verify`: after every Make/UnMake compare incremental accumulators with a full refresh.
- Sanity: start position small positive; colour-flipped positions give equal STM scores.
- Strength: NNUE vs HCE at fixed nodes (eval quality) then fixed time (includes NPS), SPRT. No Elo claims without matches.
- Regression FENs (manual, engine has no FEN loader yet):
  - `8/8/8/4k3/8/8/8/4K2R w K - 0 1` - KR vs K must still be converted (mop-up hybrid).
  - `8/8/1p6/8/8/1P6/8/k6K w - - 0 1` - pawn ending, score near 0, stable across depths.
  - `r1bqkbnr/pppp1ppp/2n5/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R w KQkq - 2 3` - small positive.

---

## 3. Step-by-step implementation

### Phase 0 - Configuration (no behavior change)
Rule: **every number and setting used by the engine NNUE code, `NnueTool` and `NnueTrainer` comes from `Config/Configuration.json`, node `NnueConfiguration`.** No magic numbers in code except the feature-set constant `Inputs = 768` (it defines the record/feature format; it is still validated against config and the net file header).

The existing pattern is followed: JSON node -> `Engine.Models.Config.*` class -> `Engine.Interfaces.Config.I*` interface -> `IConfigurationProvider` property. Deserialization is Newtonsoft (`Boot.SetUp`, `UiApp`), so plain `{ get; set; }` classes are enough.

**0.1 JSON node** (new top-level node in `Config/Configuration.json`, sibling of `EndGameConfiguration`):
```json
"NnueConfiguration": {
  "IsEnabled": false,                 // false = hand-crafted evaluation, no behavior change
  "NetworkPath": "Config\\kio.nnue",
  "Network": {
    "Inputs": 768,
    "Hidden": 256,
    "QA": 255,
    "QB": 64,
    "Scale": 400,
    "MaxEvaluation": 20000,           // clamp of NNUE output, must stay below Mate - max ply
    "UseEvaluationCache": false,
    "UseEndGameAdjustments": true     // keep HCE mop-up + rule-50 scaling on top of NNUE
  },
  "Data": {
    "SourceDirectory": "C:\\Dev\\PGN",
    "OutputDirectory": "C:\\Dev\\NNUE\\Data",
    "Filter": {
      "MinElo": 2200,
      "MinBaseTimeSeconds": 180,
      "ExcludeBots": true,
      "AllowedTerminations": [ "Normal", "Time forfeit" ]
    },
    "Extract": {
      "MinPly": 16,
      "SampleRate": 0.5,
      "SkipInCheck": true,
      "SkipNonQuiet": true,
      "BloomFilterBits": 27
    },
    "Label": {
      "Strategy": "lmrd",
      "Depth": 6,
      "MaxAbsScore": 3000,
      "SkipCaptureBestMove": true,
      "ClearTablePerGame": true,
      "Workers": 16,
      "RangesPerWorker": 8,
      "MaxHours": 17,
      "FlushPositions": 100000        // write + resume checkpoint interval
    },
    "Merge": {
      "ValidationRate": 0.01,
      "ShuffleChunkMB": 256,
      "MaxPoolPositions": 500000000
    }
  },
  "Training": {
    "TrainPath": "C:\\Dev\\NNUE\\Data\\train.bin",
    "ValidationPath": "C:\\Dev\\NNUE\\Data\\val.bin",
    "OutputDirectory": "C:\\Dev\\NNUE\\Nets",
    "Threads": 12,
    "BatchSize": 16384,
    "Epochs": 15,
    "LearningRate": 0.001,
    "FinalLearningRate": 0.00001,
    "WeightDecay": 0.0,
    "Lambda": 0.75,                   // share of search score in the target, rest = game result
    "WeightClip": 1.98,
    "Seed": 42,
    "MaxHours": 17
  }
}
```

**0.2 Types** (one file per type, in `Engine/Models/Config/` and `Engine/Interfaces/Config/`, matching `BookConfiguration`/`IBookConfiguration`):
```csharp
// Engine/Interfaces/Config/INnueConfiguration.cs
namespace Engine.Interfaces.Config;

public interface INnueConfiguration
{
    bool IsEnabled { get; }
    string NetworkPath { get; }
    NnueNetworkConfiguration Network { get; }
    NnueDataConfiguration Data { get; }
    NnueTrainingConfiguration Training { get; }
}

// Engine/Models/Config/NnueConfiguration.cs
namespace Engine.Models.Config;

public class NnueConfiguration : INnueConfiguration
{
    public bool IsEnabled { get; set; }
    public string NetworkPath { get; set; }
    public NnueNetworkConfiguration Network { get; set; }
    public NnueDataConfiguration Data { get; set; }
    public NnueTrainingConfiguration Training { get; set; }
}

public class NnueNetworkConfiguration
{
    public int Inputs { get; set; }
    public int Hidden { get; set; }
    public int QA { get; set; }
    public int QB { get; set; }
    public int Scale { get; set; }
    public int MaxEvaluation { get; set; }
    public bool UseEvaluationCache { get; set; }
    public bool UseEndGameAdjustments { get; set; }
}

public class NnueDataConfiguration
{
    public string SourceDirectory { get; set; }
    public string OutputDirectory { get; set; }
    public NnueFilterConfiguration Filter { get; set; }
    public NnueExtractConfiguration Extract { get; set; }
    public NnueLabelConfiguration Label { get; set; }
    public NnueMergeConfiguration Merge { get; set; }
}

public class NnueFilterConfiguration
{
    public int MinElo { get; set; }
    public int MinBaseTimeSeconds { get; set; }
    public bool ExcludeBots { get; set; }
    public string[] AllowedTerminations { get; set; }
}

public class NnueExtractConfiguration
{
    public int MinPly { get; set; }
    public double SampleRate { get; set; }
    public bool SkipInCheck { get; set; }
    public bool SkipNonQuiet { get; set; }
    public int BloomFilterBits { get; set; }
}

public class NnueLabelConfiguration
{
    public string Strategy { get; set; }
    public short Depth { get; set; }
    public int MaxAbsScore { get; set; }
    public bool SkipCaptureBestMove { get; set; }
    public bool ClearTablePerGame { get; set; }
    public int Workers { get; set; }
    public int RangesPerWorker { get; set; }
    public double MaxHours { get; set; }
    public int FlushPositions { get; set; }
}

public class NnueMergeConfiguration
{
    public double ValidationRate { get; set; }
    public int ShuffleChunkMB { get; set; }
    public long MaxPoolPositions { get; set; }
}

public class NnueTrainingConfiguration
{
    public string TrainPath { get; set; }
    public string ValidationPath { get; set; }
    public string OutputDirectory { get; set; }
    public int Threads { get; set; }
    public int BatchSize { get; set; }
    public int Epochs { get; set; }
    public double LearningRate { get; set; }
    public double FinalLearningRate { get; set; }
    public double WeightDecay { get; set; }
    public double Lambda { get; set; }
    public double WeightClip { get; set; }
    public int Seed { get; set; }
    public double MaxHours { get; set; }
}
```
(Sub-classes may live in one file `NnueConfiguration.cs` or one file each - follow the folder's convention.)

**0.3 Wiring**
- `Configuration.cs`: add `public NnueConfiguration NnueConfiguration { get; set; }`.
- `IConfigurationProvider`: add `INnueConfiguration NnueConfiguration { get; }`.
- `ConfigurationProvider`: add constructor parameter `INnueConfiguration nnueConfiguration` and property.
- Update **both** construction sites: `Tools.Common/Boot.cs` and `UI.Common/UiApp.cs` (pass `configuration.NnueConfiguration`).
- Every project's copy of `Config/Configuration.json` used at runtime gets the node (missing node => `null` => treat as disabled; guard with `NnueConfiguration?.IsEnabled == true`).

**0.4 Validation at load** (engine, `Board` initialization):
- `IsEnabled == false` or file missing -> HCE, log once.
- Net file header (inputs, hidden, QA, QB, scale) must equal `Network` values, else throw a clear error (prevents running a net with wrong quantization).
- `MaxEvaluation < Mate - DynamicGameDepth`.

**0.5 Consumers**
| Consumer | Uses |
|---|---|
| Engine (`NnueNetwork`, `Board.Nnue`, `Board.Evaluation`) | `IsEnabled`, `NetworkPath`, `Network.*` |
| `NnueTool filter/extract/label/merge` | `Data.*` (CLI options override config values for experiments) |
| `NnueTrainer` | `Training.*`, `Network.Hidden/QA/QB/Scale` (export must match engine) |

`NnueTrainer` references `Engine` only to reuse these configuration types and reads the same `Configuration.json` with Newtonsoft (no `Boot.SetUp`, no container). Engine still has no TorchSharp dependency.

Exit: build green; `IsEnabled = false` -> benchmark node counts identical.

### Phase 1 - Engine inference
**1.1 `NnueNetwork`** - loader and AVX2 evaluation.
```csharp
namespace Engine.Nnue;

public sealed class NnueNetwork
{
    public const int Inputs = 768;          // feature-set constant, validated against config
    private const int Magic = 0x554E4E4B;   // 'KNNU'
    private const int Version = 2;

    public readonly int Hidden;
    public readonly int QA;
    public readonly int QB;
    public readonly int Scale;
    private readonly int _divisor;           // QA * QB, precomputed

    public readonly short[] FtWeights;       // [feature][hidden]
    public readonly short[] FtBias;
    public readonly short[] OutWeights;      // [us | them]
    public int OutBias;

    private NnueNetwork(NnueNetworkConfiguration config)
    {
        if (config.Inputs != Inputs || config.Hidden % Vector256<short>.Count != 0)
            throw new InvalidDataException("NnueConfiguration.Network: Inputs must be 768 and Hidden a multiple of 16");

        Hidden = config.Hidden;
        QA = config.QA;
        QB = config.QB;
        Scale = config.Scale;
        _divisor = QA * QB;

        FtWeights = new short[Inputs * Hidden];
        FtBias = new short[Hidden];
        OutWeights = new short[2 * Hidden];
    }

    public static NnueNetwork Load(string path, NnueNetworkConfiguration config)
    {
        using var reader = new BinaryReader(File.OpenRead(path));
        if (reader.ReadInt32() != Magic || reader.ReadInt32() != Version
            || reader.ReadInt32() != config.Inputs || reader.ReadInt32() != config.Hidden
            || reader.ReadInt32() != config.QA || reader.ReadInt32() != config.QB
            || reader.ReadInt32() != config.Scale)
            throw new InvalidDataException($"NNUE file '{path}' does not match NnueConfiguration.Network");

        var net = new NnueNetwork(config);
        reader.BaseStream.ReadExactly(MemoryMarshal.AsBytes(net.FtWeights.AsSpan()));
        reader.BaseStream.ReadExactly(MemoryMarshal.AsBytes(net.FtBias.AsSpan()));
        reader.BaseStream.ReadExactly(MemoryMarshal.AsBytes(net.OutWeights.AsSpan()));
        net.OutBias = reader.ReadInt32();
        return net;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Evaluate(short[] us, short[] them)
    {
        ref short w = ref MemoryMarshal.GetArrayDataReference(OutWeights);
        var sum = Accumulate(Vector256<int>.Zero, us, ref w);
        sum = Accumulate(sum, them, ref Unsafe.Add(ref w, Hidden));

        int output = Vector256.Sum(sum) / QA + OutBias;
        return output * Scale / _divisor;
    }

    // SCReLU: (v * w) * v with v = clamp(x, 0, QA); v * w fits int16 because |w| <= 127
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector256<int> Accumulate(Vector256<int> sum, short[] acc, ref short w)
    {
        var zero = Vector256<short>.Zero;
        var qa = Vector256.Create((short)QA);
        ref short a = ref MemoryMarshal.GetArrayDataReference(acc);
        nuint hidden = (nuint)Hidden;

        for (nuint i = 0; i < hidden; i += (nuint)Vector256<short>.Count)
        {
            var v = Vector256.Min(Vector256.Max(Vector256.LoadUnsafe(ref a, i), zero), qa);
            var vw = v * Vector256.LoadUnsafe(ref w, i);
            sum += Avx2.MultiplyAddAdjacent(vw, v);
        }
        return sum;
    }
}
```

**1.2 `Board.Nnue.cs`** - accumulators.
```csharp
public partial class Board
{
    private NnueNetwork _nnue;
    private short[] _accWhite;
    private short[] _accBlack;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int WhiteIndex(byte piece, byte square) => piece * 64 + square;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int BlackIndex(byte piece, byte square) => (piece < 6 ? piece + 6 : piece - 6) * 64 + (square ^ 56);

    private int _nnueMaxEvaluation;
    private bool _nnueEndGameAdjustments;

    // called from Board initialization with configurationProvider.NnueConfiguration
    private void InitializeNnue(INnueConfiguration config)
    {
        if (config == null || !config.IsEnabled || !File.Exists(config.NetworkPath)) return;

        _nnue = NnueNetwork.Load(config.NetworkPath, config.Network);
        _nnueMaxEvaluation = config.Network.MaxEvaluation;
        _nnueEndGameAdjustments = config.Network.UseEndGameAdjustments;
        _accWhite = new short[_nnue.Hidden];
        _accBlack = new short[_nnue.Hidden];
        NnueRefresh();
    }

    public void NnueRefresh()
    {
        _nnue.FtBias.CopyTo(_accWhite, 0);
        _nnue.FtBias.CopyTo(_accBlack, 0);
        for (byte piece = 0; piece < 12; piece++)
            foreach (var square in _boards[piece].BitScan())
                NnueAdd(piece, square);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void NnueAdd(byte piece, byte square)
    {
        AddRow(_accWhite, WhiteIndex(piece, square));
        AddRow(_accBlack, BlackIndex(piece, square));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void NnueRemove(byte piece, byte square)
    {
        SubRow(_accWhite, WhiteIndex(piece, square));
        SubRow(_accBlack, BlackIndex(piece, square));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void NnueMove(byte piece, byte from, byte to)
    {
        SubAddRow(_accWhite, WhiteIndex(piece, from), WhiteIndex(piece, to));
        SubAddRow(_accBlack, BlackIndex(piece, from), BlackIndex(piece, to));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddRow(short[] acc, int feature)
    {
        ref short a = ref MemoryMarshal.GetArrayDataReference(acc);
        ref short w = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_nnue.FtWeights), feature * _nnue.Hidden);
        for (nuint i = 0; i < (nuint)_nnue.Hidden; i += (nuint)Vector256<short>.Count)
            (Vector256.LoadUnsafe(ref a, i) + Vector256.LoadUnsafe(ref w, i)).StoreUnsafe(ref a, i);
    }

    // SubRow analogous; SubAddRow does acc = acc - w[from] + w[to] in one pass.
}
```

**1.3 Hooks** - one line per primitive in `Board.Manipulation.cs`:
```csharp
public void MoveWhite(byte piece, byte from, byte to)
{
    Hash = Hash ^ _hashTable[from * 12 + piece] ^ _hashTable[to * 12 + piece];
    if (_nnue != null) NnueMove(piece, from, to);
    // ... unchanged
}
```

**1.4 STM-aware evaluation** in `Board.Evaluation.cs`:
```csharp
public int Evaluate()          // white to move
{
    var phase = _moveHistory.GetPhase();
    _evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);
    return _nnue == null ? EvaluateCached(phase) : EvaluateNnue(phase, true);
}

public int EvaluateOpposite()  // black to move, STM-relative
{
    var phase = _moveHistory.GetPhase();
    _evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);
    return _nnue == null ? -EvaluateCached(phase) : EvaluateNnue(phase, false);
}

private int EvaluateNnue(byte phase, bool isWhite)
{
    int value = isWhite ? _nnue.Evaluate(_accWhite, _accBlack) : _nnue.Evaluate(_accBlack, _accWhite);
    if (_nnueEndGameAdjustments && phase == Phase.End)
        value = ApplyEndGameAdjustments(value, isWhite); // existing mop-up + rule-50 scaling, STM sign
    return Math.Clamp
}
```
v1 bypasses the eval cache for NNUE (`Network.UseEvaluationCache = false`). If profiling shows a gain, set it to `true` and use `meta = phase | castle | turnBit` with STM-relative values.
Check: every evaluation call site must go through `Position.GetValue()` (or pass the turn); a direct `_board.Evaluate()` for black would be wrong with an STM net.

**1.5** Verify: NNUE off -> identical node counts; NNUE on with a dummy net -> 0 accumulator mismatches.

### Phase 2 - Data generator (`NnueTool`)
copy `Config` like `PgnTool`; `NnueConfiguration.IsEnabled = false` for HCE labeling; all thresholds from `NnueConfiguration.Data`).

**2.2** Pass 1 `filter` - tag-only text filter (no move parsing):
```csharp
// reads raw game text blocks; writes kept games unchanged
static bool Keep(IReadOnlyDictionary<string, string> tags, int minElo, int minBaseSeconds)
{
    if (tags.TryGetValue("WhiteTitle", out var wt) && wt == "BOT") return false;
    if (tags.TryGetValue("BlackTitle", out var bt) && bt == "BOT") return false;
    if (!int.TryParse(tags.GetValueOrDefault("WhiteElo"), out var we) || we < minElo) return false;
    if (!int.TryParse(tags.GetValueOrDefault("BlackElo"), out var be) || be < minElo) return false;

    var result = tags.GetValueOrDefault("Result");
    if (result is not ("1-0" or "0-1" or "1/2-1/2")) return false;

    var tc = tags.GetValueOrDefault("TimeControl");    // "180+2"
    int plus = tc?.IndexOf('+') ?? -1;
    return plus > 0 && int.TryParse(tc.AsSpan(0, plus), out var baseSeconds) && baseSeconds >= minBaseSeconds;
}
```

**2.3** Pass 2+3 `label` - replay, select, search (option A):
```csharp
var factory = Boot.GetService<IStrategyFactory>();
var data = Boot.GetService<IConfigurationProvider>().NnueConfiguration.Data;   // CLI options may override
var extract = data.Extract;
var label = data.Label;
var maxDuration = TimeSpan.FromHours(label.MaxHours);

foreach (var game in reader.ReadGames())             // existing PgnStreamReader, comments not needed
{
    var position = new Position();                     // or reset; check MoveHistoryService/TT reset requirements
    bool isWhite = true;
    int ply = 0;

    foreach (var pgnMove in game.Moves)
    {
        var move = resolver.Resolve(position, pgnMove, isWhite);   // PgnConverter logic
        if (move == null) { failedGames++; break; }

        if (ply >= extract.MinPly
            && (!extract.SkipNonQuiet || (!move.IsAttack && !move.IsPromotion))
            && (!extract.SkipInCheck || !IsInCheck(position, isWhite))
            && rng.NextDouble() < extract.SampleRate
            && bloom.TryAdd(position.GetBoard().Hash ^ (isWhite ? 0UL : TurnKey)))
        {
            var strategy = factory.GetStrategy(label.Depth, position, label.Strategy);
            var result = strategy.GetResult();          // must bypass book; restores MoveHistory after search
            int score = result.Value;                   // verify: side-to-move relative

            if (Math.Abs(score) <= label.MaxAbsScore
                && (!label.SkipCaptureBestMove || result.Move is { IsAttack: false }))
                writer.Write(position.GetBoard(), isWhite, (short)score, Wdl(game.Result, isWhite), (byte)ply, (byte)label.Depth);
        }

        Make(position, move, isWhite);                  // same path PgnConverter uses
        isWhite = !isWhite;
        ply++;
    }

    progress.MarkDone(reader.Position);                 // resume point (flushed every label.FlushPositions)
    if (stopwatch.Elapsed > maxDuration) break;
}
```
Note: creating a strategy per position may allocate a TT; create one strategy per worker and reuse it if the API allows (check `StrategyBase` constructor: it resolves a TT via `ITranspositionTableService.Create`).
Packed writer:
```csharp
internal void Write(Board board, bool whiteToMove, short score, byte wdl, byte ply, byte depth)
{
    Span<byte> rec = stackalloc byte[32];
    rec.Clear();
    ulong occ = board.GetOccupied();
    BinaryPrimitives.WriteUInt64LittleEndian(rec, occ);

    int n = 0;
    for (ulong b = occ; b != 0; b &= b - 1, n++)
    {
        byte sq = (byte)BitOperations.TrailingZeroCount(b);
        board.GetPiece(sq, out var piece);
        rec[8 + (n >> 1)] |= (byte)(piece.Value << ((n & 1) * 4));
    }

    BinaryPrimitives.WriteInt16LittleEndian(rec[24..], score);
    rec[26] = wdl;
    rec[27] = whiteToMove ? (byte)0 : (byte)1;
    rec[28] = ply;
    rec[29] = depth;
    _stream.Write(rec);
}
```

**2.4** Orchestration (replaces PgnManager's process-per-game model):
```csharp
var ranges = PgnSplitter.Split(input, workers * 8);  // many small game-aligned ranges -> queue (P/E core balance)
var processes = Enumerable.Range(0, workers).Select(i => Process.Start("NnueTool.exe",
    $"label --input \"{input}\" --queue queue.txt --depth 6 --max-hours 17 --out data\\labeled-{i:D2}.bin")).ToArray();
foreach (var p in processes) p.WaitForExit();
```
**2.5** `merge`: dedupe by 64-bit hash of the first 28 bytes, block shuffle (chunks + in-chunk Fisher-Yates), write `train.bin`/`val.bin`.
**2.6** `verify`: replay games with NNUE on, call `NnueVerify()` after each Make/UnMake.
**2.7** `stats`: distribution by ply, material, score buckets.
Exit: >99% of games replay, 0 mismatches, measured labeling throughput at depth 6/8, first 30-50M labeled positions.

### Phase 3 - Trainer (`NnueTrainer`)
**3.1** Project with `TorchSharp-cpu`.
**3.2** Record decoding into sparse indices:
```csharp
static void Decode(ReadOnlySpan<byte> rec, List<long> stmIdx, List<long> nstmIdx)
{
    ulong occ = BinaryPrimitives.ReadUInt64LittleEndian(rec);
    bool black = rec[27] == 1;
    int n = 0;
    for (ulong b = occ; b != 0; b &= b - 1, n++)
    {
        int sq = BitOperations.TrailingZeroCount(b);
        int p = (rec[8 + (n >> 1)] >> ((n & 1) * 4)) & 0xF;
        long w = p * 64 + sq;
        long bl = (p < 6 ? p + 6 : p - 6) * 64 + (sq ^ 56);
        stmIdx.Add(black ? bl : w);
        nstmIdx.Add(black ? w : bl);
    }
}
```
**3.3** Model and step:
```csharp
sealed class KioNet : Module<Tensor, Tensor, Tensor, Tensor, Tensor>
{
    private readonly EmbeddingBag ft = EmbeddingBag(768, 256, mode: EmbeddingBagMode.Sum);
    private readonly Parameter ftBias = Parameter(zeros(256));
    private readonly Linear output = Linear(512, 1);

    public KioNet() : base("kio") => RegisterComponents();

    public override Tensor forward(Tensor stm, Tensor stmOffsets, Tensor nstm, Tensor nstmOffsets)
    {
        var a = ft.call(stm, stmOffsets) + ftBias;
        var b = ft.call(nstm, nstmOffsets) + ftBias;
        var h = cat([a, b], 1).clamp(0, 1).square();   // SCReLU
        return output.call(h);
    }
}

// configuration (Newtonsoft, same Configuration.json, no container)
var root = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(@"Config\Configuration.json"));
var training = root.NnueConfiguration.Training;
var network = root.NnueConfiguration.Network;
torch.set_num_threads(training.Threads);
torch.manual_seed(training.Seed);

// training step (score scale = network.Scale; sigmoid temperature fixed at 400 cp)
var pred = sigmoid(net.call(stm, stmOff, nstm, nstmOff).squeeze(1) * (network.Scale / 400.0));
var target = training.Lambda * sigmoid(score / 400.0) + (1 - training.Lambda) * wdl;
var loss = (pred - target).pow(2).mean();
optimizer.zero_grad();
loss.backward();
optimizer.step();
using (no_grad()) { /* clamp_ ft.weight and output.weight to [-training.WeightClip, training.WeightClip] */ }
```
**3.4** Export (format v2; EmbeddingBag weight is already `[Inputs, Hidden]`, no transpose; all values from `NnueConfiguration.Network`):
```
int32 'KNNU' | int32 2 | int32 Inputs | int32 Hidden | int32 QA | int32 QB | int32 Scale
short[Inputs*Hidden] FT weights  round(w * QA)
short[Hidden]        FT bias     round(b * QA)
short[2*Hidden]      out weights round(w * QB)
int32                out bias    round(b * QA * QB)
```
The engine loader rejects a file whose header differs from its `NnueConfiguration.Network`.
**3.5** Smoke run: 10M positions x 3 epochs; validation loss decreases; net loads; start position sane.

### Phase 4 - Integration and tuning
**4.1** Calibrate NNUE vs HCE scale on ~10k quiet positions.
**4.2** Retune `FutilityMargin`, `ReverseFutilityMargin`, `RazoringMargin`, `DeltaPruningMargin`, `AspirationWindow` one at a time (SPRT).
**4.3** Matches NNUE vs HCE: fixed nodes, then fixed time.
**4.4** Iterate: label new months with search on net N (NNUE on) to train net N+1; increase depth to 8; H = 512, then buckets.

### Phase 5 - Monthly pipeline
Each step fits in one <= 18 h session:
1. Download + `zstd -d` + `NnueTool filter` (~2 h).
2. `NnueTool label --max-hours 17` (one or more sessions, resumable).
3. `NnueTool merge` (append to pool, cap size) (~0.5 h).
4. `NnueTrainer train --resume` (3-8 h, checkpoint per epoch).
5. Export -> SPRT vs previous net -> promote.

---

## 4. Risks
- **STM vs cache**: turn missing from cache meta gives silently wrong evals. v1 bypasses the cache for NNUE.
- **Accumulator desync**: any future board write outside the six primitives (FEN loader) breaks the net. Mitigation: `verify` and `Debug.Assert(NnueVerify())` in debug builds.
- **Label quality**: depth-6/8 HCE search inherits HCE blind spots; WDL mixing (?) and later NNUE-search relabeling reduce it.
- **Labeling throughput** is the main bottleneck; measure positions/hour at depth 6 and 8 before committing to a depth.
- **Search state leakage**: book usage, TT/history and `MoveHistoryService` state must be reset/restored correctly between labeled positions; verify on a few games by comparing with a fresh-process search.
- **Endgames**: small nets misjudge bare-king and fortress positions; hybrid HCE terms remain.
- **Margins**: HCE-tuned pruning can cost more than the net gains until retuned.
- **Estimates** (throughput, training time, NPS) are unmeasured; benchmark before scaling up.
