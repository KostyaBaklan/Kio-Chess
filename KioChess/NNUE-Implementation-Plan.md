# NNUE Implementation Plan (KioChess) - revision 2

Status: design only. No engine code has been changed.
Target hardware: Intel i5-14600K (6P + 8E cores, 20 threads, AVX2, no AVX-512), one home PC, CPU-only training.
Data source: monthly Lichess database dumps (`lichess_db_standard_rated_YYYY-MM.pgn.zst`).

---

## 1. Deep analysis

### 1.1 Review of the previous plan (rev. 1)
| Item in rev. 1 | Finding | Change in rev. 2 |
|---|---|---|
| "Phase 0 done: `NnuePath` added" | **Not true in the current branch.** `NnuePath` exists in no `.cs`/`.json` file. | Phase 0 is re-opened. |
| Label = static HCE `Board.Evaluate()` | Distilling the static HCE caps the net at roughly HCE quality (plus result signal). Static eval of non-quiet positions is noisy. | Primary label = Lichess `[%eval]` (Stockfish) where present; fallback = own engine shallow search score. Game result always stored. |
| White-relative network, no side to move | Loses tempo; all modern NNUEs are side-to-move relative. `Position` knows the turn. | Side-to-move (STM) perspective ordering. |
| Comments ignored by the PGN parser | `PgnStreamReader` and `PgnReader` skip `{...}`; `[%eval]` is lost. | Dedicated fast reader in the generator that keeps per-move `[%eval]`. |
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

### 1.7 Lichess data facts (approximate, verify on your dump)
- Monthly standard dump: ~90-100M games, ~30+ GB `.zst`, ~200+ GB decompressed.
- About 6-10% of games carry `[%eval x.xx]` / `[%eval #n]` after every move (Stockfish, white POV, pawns). Several million games x ~70 plies = **hundreds of millions of labeled positions per month**, more than this PC can train on. Use them as the primary source.
- `.zst` can be streamed (`zstd -d --stdout` or `ZstdSharp.Port`), but compressed streams cannot seek; decompress once to SSD for sharding.

### 1.8 Search interactions
- `MarginConfiguration` (Futility 200/450, RFP 150..775, Razoring 700, Delta 375, AttackMargin), `AspirationWindow` 125, `TradeMargin` are tuned to HCE scale. NNUE is calibrated to pawn ~ 100 via `SCALE`, but variance differs: retune after the first net.
- `GetMopUpValue()` and rule-50 scaling: small nets are weak at converting KQ/KR vs K and at drawish scaling. Keep these HCE terms on top of NNUE in phase End (hybrid).
- Depth ~10: NNUE changes leaf quality at every node, no depth-specific assumptions. Expected NPS: AVX2 inference with H=256 is likely comparable to `ComputeAttacks()` + full HCE; incremental updates add 2-4 row ops per make/unmake. Measure, do not assume.

---

## 2. Architecture

### 2.1 Overview
```
lichess_db_*.pgn(.zst)
   |  NnueTool run    -> split into N game-aligned byte ranges
   |  NnueTool gen    (N worker processes; each: Boot.SetUp, one Position, NNUE off)
   v
data/part-XX.bin   (32-byte packed records)
   |  NnueTool merge  (dedupe, shuffle, train/val split)
   v
data/train.bin, data/val.bin
   |  NnueTrainer train  (TorchSharp-cpu, sparse EmbeddingBag)
   v
nets/kio-XXX.nnue  (quantized)  -->  GeneralConfiguration.NnuePath
   |
   v
Engine: Nnue/NnueNetwork (loader + AVX2) + Board.Nnue (accumulators) + Evaluate switch
```
Projects:
- `NnueTool` (console, net10.0): refs `Engine`, `Engine.Pgn`, `Tools.Common`. Commands: `run`, `gen`, `merge`, `verify`, `stats`.
- `NnueTrainer` (console, net10.0): refs **only** `TorchSharp-cpu`. Engine stays dependency free.

### 2.2 Network v1
- Inputs: 768 = 12 pieces x 64 squares; two perspectives share one feature transformer (FT).
  - White perspective: `idx = piece * 64 + sq`
  - Black perspective: `idx = ((piece + 6) % 12) * 64 + (sq ^ 56)`
- FT: 768 -> 256 (int16). Output: `concat(acc[stm], acc[!stm])` (512) -> SCReLU -> 1.
- Quantization: `QA = 255`, `QB = 64`, `SCALE = 400`; `cp = (? clamp(acc,0,QA)^2 * w / QA + b) * SCALE / (QA * QB)`.
- Target (STM POV): `t = ? * sigmoid(score / 400) + (1 - ?) * wdl`, `wdl ? {0, 0.5, 1}`, ? ? 0.75 for eval-labeled data.
- Upgrades only after v1 beats HCE: H = 512, king buckets, output buckets by piece count.

### 2.3 Training record (32 bytes, little endian)
```
ulong    occupancy   8   occupied squares (a1 = bit 0, real board)
byte[16] pieces     16   4-bit piece code per occupied square in bit order (max 32 pieces)
short    score       2   STM-relative cp, clamped to +-3000
byte     wdl         1   0 loss / 1 draw / 2 win, STM POV
byte     stm         1   0 white / 1 black
byte     ply         1
byte     source      1   0 = lichess %eval, 1 = own search
ushort   reserved    2
```
100M records = 3.2 GB; memory-mappable. The trainer applies the perspective mapping, so the file format is independent of the network layout.

### 2.4 Data generation
Game filter (CLI defaults): movetext contains `%eval` (cheap string check before parsing); result in {1-0, 0-1, 1/2-1/2}; `Termination` = Normal/Time forfeit; both Elo >= 1800; base time >= 60 s.

Position filter: ply >= 16; side to move not in check; next played move is not a capture/promotion; label is not a mate (`#n`); |score| <= 3000; keep with probability p (default 0.5); per-worker Bloom filter on `Hash` + turn to reduce duplicates.

Label: `%eval` after move k-1 is the eval of the position before move k (white POV, pawns) -> `score_stm = (white to move ? 1 : -1) * round(eval * 100)`.
Fallback (`--label search`): own engine fixed depth 4-6 with qsearch, NNUE off; slow (hundreds of positions/s/process).

Parallelism: N = 14-16 worker processes on the 14600K, each on a game-aligned byte range. Throughput estimate (unmeasured): 50-150k replayed positions/s per process; one month of eval games in hours.

### 2.5 Training
- TorchSharp-cpu; test `torch.set_num_threads` 6/12/20 (P-cores usually win).
- Loader: memory-mapped `train.bin`, block shuffle per epoch, decode to `long` index tensors + offsets for `EmbeddingBag`.
- Loss MSE on sigmoid space; AdamW lr 1e-3 cosine to 1e-5; batch 16384; clip FT and output weights to [-1.98, 1.98] after each step (keeps quantized output |w| <= 127 so `v*w` fits int16).
- First net: 100M positions x 15 epochs. Time estimate (unmeasured): 1-3 min per 10M positions. Benchmark one epoch before planning larger runs.
- Validation on `val.bin` (1%); checkpoint + quantized export per epoch.

### 2.6 Engine integration
New files: `Engine/Nnue/NnueNetwork.cs`, `Engine/Models/Boards/Board.Nnue.cs`.
Changes: hooks in `Board.Manipulation.cs`; init in `Board.Initialization.cs`; STM-aware NNUE branch in `Board.Evaluation.cs`; `NnuePath` config (empty = current HCE, no behavior change).
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
**0.1** Add `NnuePath` to `IGeneralConfiguration`, `GeneralConfiguration`, `Config/Configuration.json`.
```csharp
// IGeneralConfiguration
string NnuePath { get; }
// GeneralConfiguration
public string NnuePath { get; set; }
```
```json
"NnuePath": "", // empty = hand-crafted evaluation
```
Exit: build green, benchmark node counts identical.

### Phase 1 - Engine inference
**1.1 `NnueNetwork`** - loader and AVX2 evaluation.
```csharp
namespace Engine.Nnue;

public sealed class NnueNetwork
{
    public const int Inputs = 768;
    public const int Hidden = 256;
    public const int QA = 255;
    public const int QB = 64;
    public const int Scale = 400;

    public readonly short[] FtWeights = new short[Inputs * Hidden]; // [feature][hidden]
    public readonly short[] FtBias = new short[Hidden];
    public readonly short[] OutWeights = new short[2 * Hidden];     // [us | them]
    public int OutBias;

    public static NnueNetwork Load(string path)
    {
        using var reader = new BinaryReader(File.OpenRead(path));
        if (reader.ReadInt32() != 0x554E4E4B || reader.ReadInt32() != 2
            || reader.ReadInt32() != Inputs || reader.ReadInt32() != Hidden)
            throw new InvalidDataException($"Invalid NNUE file '{path}'");

        var net = new NnueNetwork();
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
        return output * Scale / (QA * QB);
    }

    // SCReLU: (v * w) * v with v = clamp(x, 0, QA); v * w fits int16 because |w| <= 127
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<int> Accumulate(Vector256<int> sum, short[] acc, ref short w)
    {
        var zero = Vector256<short>.Zero;
        var qa = Vector256.Create((short)QA);
        ref short a = ref MemoryMarshal.GetArrayDataReference(acc);

        for (nuint i = 0; i < Hidden; i += (nuint)Vector256<short>.Count)
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

    private void InitializeNnue(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
        _nnue = NnueNetwork.Load(path);
        _accWhite = new short[NnueNetwork.Hidden];
        _accBlack = new short[NnueNetwork.Hidden];
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
        ref short w = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_nnue.FtWeights), feature * NnueNetwork.Hidden);
        for (nuint i = 0; i < NnueNetwork.Hidden; i += (nuint)Vector256<short>.Count)
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
    if (phase == Phase.End)
        value = ApplyEndGameAdjustments(value, isWhite); // existing mop-up + rule-50 scaling, STM sign
    return Math.Clamp(value, -MaxNnueEval, MaxNnueEval); // well below Mate - max ply
}
```
v1 bypasses the eval cache for NNUE. If profiling shows a gain, re-enable with `meta = phase | castle | turnBit` and STM-relative values.
Check: every evaluation call site must go through `Position.GetValue()` (or pass the turn); a direct `_board.Evaluate()` for black would be wrong with an STM net.

**1.5** Verify: NNUE off -> identical node counts; NNUE on with a dummy net -> 0 accumulator mismatches.

### Phase 2 - Data generator (`NnueTool`)
**2.1** Create the project (refs `Engine`, `Engine.Pgn`, `Tools.Common`; copy `Config` like `PgnTool`; `NnuePath` empty).

**2.2** Fast movetext parser that keeps `%eval`:
```csharp
// san[k] = k-th move; evals[k] = eval after move k (short.MinValue = none or mate)
internal static void ParseMoves(ReadOnlySpan<char> text, List<string> san, List<short> evals)
{
    int i = 0;
    while (i < text.Length)
    {
        char c = text[i];
        if (c == '{')
        {
            int end = i + text[i..].IndexOf('}');
            var comment = text[(i + 1)..end];
            int e = comment.IndexOf("%eval ");
            if (e >= 0 && evals.Count > 0)
            {
                var token = comment[(e + 6)..];
                int stop = token.IndexOfAny(' ', ']');
                token = stop >= 0 ? token[..stop] : token;
                evals[^1] = token[0] == '#' ? short.MinValue
                    : (short)Math.Clamp(Math.Round(double.Parse(token, CultureInfo.InvariantCulture) * 100), -3000, 3000);
            }
            i = end + 1;
        }
        else if (char.IsWhiteSpace(c)) i++;
        else
        {
            int start = i;
            while (i < text.Length && !char.IsWhiteSpace(text[i]) && text[i] != '{') i++;
            var token = text[start..i];
            if (char.IsDigit(token[0]) || token[0] == '$' || token[0] == '*') continue; // move numbers, NAGs, results
            san.Add(token.TrimEnd("!?+#").ToString());
            evals.Add(short.MinValue);
        }
    }
}
```

**2.3** Replay and emit records:
```csharp
var position = new Position();            // per game, or UnMake all moves (check MoveHistoryService reset)
bool isWhite = true;

for (int k = 0; k < san.Count; k++)
{
    var move = resolver.Resolve(position, san[k], isWhite); // PgnConverter logic
    if (move == null) { failedGames++; break; }

    if (k >= 16 && evals[k - 1] != short.MinValue
        && !move.IsAttack && !move.IsPromotion
        && !IsInCheck(position, isWhite)
        && rng.NextDouble() < sampleRate)
    {
        short score = isWhite ? evals[k - 1] : (short)-evals[k - 1];
        writer.Write(position.GetBoard(), isWhite, score, Wdl(result, isWhite), (byte)k);
    }

    Make(position, move, isWhite);        // same path PgnConverter uses
    isWhite = !isWhite;
}
```
Packed writer:
```csharp
internal void Write(Board board, bool whiteToMove, short score, byte wdl, byte ply)
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
    _stream.Write(rec);                   // BufferedStream, 1 MB buffer
}
```

**2.4** `run` orchestration (replaces process-per-game):
```csharp
var ranges = PgnSplitter.Split(input, workers);      // offsets aligned to "\n[Event "
var processes = ranges.Select((r, i) => Process.Start("NnueTool.exe",
    $"gen --input \"{input}\" --from {r.From} --to {r.To} --out data\\part-{i:D2}.bin")).ToArray();
foreach (var p in processes) p.WaitForExit();
```
**2.5** `merge`: dedupe by 64-bit hash of the first 28 bytes, block shuffle (chunks + in-chunk Fisher-Yates), write `train.bin`/`val.bin`.
**2.6** `verify`: replay games with NNUE on, call `NnueVerify()` after each Make/UnMake.
**2.7** `stats`: distribution by ply, material, score buckets.
Exit: >99% of games replay, 0 mismatches, 50-100M positions.

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

// training step (SCALE / 400 == 1)
var pred = sigmoid(net.call(stm, stmOff, nstm, nstmOff).squeeze(1));
var target = lambda * sigmoid(score / 400.0) + (1 - lambda) * wdl;
var loss = (pred - target).pow(2).mean();
optimizer.zero_grad();
loss.backward();
optimizer.step();
using (no_grad()) { /* clamp_ ft.weight and output.weight to [-1.98, 1.98] */ }
```
**3.4** Export (format v2; EmbeddingBag weight is already `[768,256]`, no transpose):
```
int32 'KNNU' | int32 2 | int32 768 | int32 256
short[768*256] FT weights  round(w * QA)
short[256]     FT bias     round(b * QA)
short[512]     out weights round(w * QB)
int32          out bias    round(b * QA * QB)
```
**3.5** Smoke run: 10M positions x 3 epochs; validation loss decreases; net loads; start position sane.

### Phase 4 - Integration and tuning
**4.1** Calibrate NNUE vs HCE scale on ~10k quiet positions.
**4.2** Retune `FutilityMargin`, `ReverseFutilityMargin`, `RazoringMargin`, `DeltaPruningMargin`, `AspirationWindow` one at a time (SPRT).
**4.3** Matches NNUE vs HCE: fixed nodes, then fixed time.
**4.4** Iterate: more months of data, H = 512, then buckets; optionally relabel with own search using net N to train net N+1.

### Phase 5 - Monthly pipeline
Download -> `zstd -d` -> `NnueTool run` -> `NnueTool merge` (append to pool, cap size) -> `NnueTrainer train --resume` -> export -> SPRT vs previous net -> promote.

---

## 4. Risks
- **STM vs cache**: turn missing from cache meta gives silently wrong evals. v1 bypasses the cache for NNUE.
- **Accumulator desync**: any future board write outside the six primitives (FEN loader) breaks the net. Mitigation: `verify` and `Debug.Assert(NnueVerify())` in debug builds.
- **Label noise**: Lichess `%eval` depth varies; WDL mixing (?) dampens it.
- **Endgames**: small nets misjudge bare-king and fortress positions; hybrid HCE terms remain.
- **Margins**: HCE-tuned pruning can cost more than the net gains until retuned.
- **Estimates** (throughput, training time, NPS) are unmeasured; benchmark before scaling up.
