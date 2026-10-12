# Null-Move Infrastructure Refactor: Instructions for the Local Agent

## 0. Read this first

**Goal:** make the null-move infrastructure in the C# chess engine correct, faster and stronger without losing Elo.

**Scope:** limited to these files.

| File | Role |
|---|---|
| `Engine/Strategies/Base/StrategyBase.cs` | Null-move logic: `CanUseNull`, `Calculate*Depth`, `GetNullDepth`, `NullWindowSearch*`, `GetMovesForNullSearch`, `Common*Search` |
| `Engine/Strategies/End/IdLmrDeepEndStrategy.cs` | Endgame wrapper strategy (stale `Ply` fix) |
| `Engine/Services/MoveHistoryService.cs` | Null flag, repetition, history arrays |
| `Engine/Models/Boards/Position.cs` | Make/UnMake, side-to-move |
| `Engine/Models/Boards/Board.State.cs` | Material helpers |
| `Engine/Services/DataPoolService.cs` | Null sort context (to delete) |
| `Engine/Interfaces/Config/NullConfiguration.cs` | Null config class |
| `Configuration.json` (`AlgorithmConfiguration.NullConfiguration`) | Null config values |
| **NEW** `Engine/Models/Moves/NullMove.cs` | Null-move placeholder |

**Rules for the agent:**
1. Implement **one phase per commit**, in this order: **0 → 1 → 4 → 2 → 3 → 5 → 6**. Phase 4 must stay a single commit so it can be reverted cleanly.
2. Do **not** change any search, evaluation or ordering behaviour outside the code shown here.
3. After every phase: build, run the bench (section 10) and report the node count, time to depth and best-move differences. Do not continue to the next phase if the build fails or the DEBUG assertions fire.
4. When an anchor (the text quoted as "find") can't be found exactly, **stop and report**. Don't guess.
5. Keep the existing code style: `[MethodImpl(MethodImplOptions.AggressiveInlining)]`, `sbyte` depths and White/Black split methods.
6. New config values must have defaults that **reproduce the current behaviour** unless a step says otherwise.

---

## 1. How the current design works (for context)

The engine uses **null-move reduction (NMR)**, not null-move pruning:

1. `CanUseNull(beta)` allows a null move only if all of these hold:
   - `MoveHistory.CanUseNull()` is true;
   - the side to move is not in check;
   - `beta <= SearchValueMinusOne`;
   - `ply - Ply >= NullDepthThreshold` (config 1, plus 1 hidden in the constructor = 2);
   - `IsLikelyZugzwangPosition()` is false.
2. `CalculateWhiteDepth/BlackDepth` flip only `_turn` (`Position.SetBlackTurn()`) and run a **separate** fixed-depth zero-window search (`NullWindowSearch*`) at depth `NullDepthReduction[depth]`. That table value is the null-search **depth**, not the reduction R.
3. `GetNullDepth`:
   - **fail low:** the node keeps its full depth;
   - **fail high:** it calls `MoveHistory.SetNull()` and the node is searched at `NullDepthExtendedReduction[depth]`. If that is < 1, the node returns qsearch.
4. `NullWindowSearch*` has **no** TT cutoffs or stores, LMR, futility, extensions, mate-distance pruning or recursive null. It uses the TT move only for ordering.

### Decoded configuration (current)

`NullWindow = 1`, `NullDepthThreshold = 1` (effective 2):

| Node depth | Null-search depth | Equivalent classic R | Depth after fail-high | Plies cut on fail-high |
|---|---|---|---|---|
| 1–3 | 0 (qsearch) | – | 0 → returns qsearch | effectively a prune |
| 4 / 5 / 6 | 1 / 2 / 3 | 2 | 1 / 2 / 3 | 3 |
| 7 | 3 | 3 | 4 | 3 |
| 8 | 4 | 3 | 4 | 4 |
| 9–20 | depth−4 | 3 | depth−4 | 4 |

What this costs:
- **R is a constant 3.** Deep null searches cost a lot because the separate null search has no TT cutoffs and no LMR.
- **No static-eval check before the null search.** Many null searches are run where eval is far below beta and are almost certain to fail low.
- **At depths 1–3, qsearch runs twice** (the null "search", then the return value).
- **Both tables have 21 entries,** but depth can exceed 20 through check and one-reply extensions, `IsLateMiddleGame` depth++, endgame +1 hops and endgame `Depth+3` models. That can throw `IndexOutOfRangeException`.

---

## 2. Confirmed issues (the reason for each phase)

| ID | Issue | Evidence | Fixed in |
|---|---|---|---|
| **C1** | The mate guard `beta > SearchValueMinusOne` never fires once ply ≥ 2, because mate-distance pruning has already clamped beta to `Mate − ply`. Null runs against mate-score betas, and the reduced re-search or qsearch return can hide mates. | `TryMateDistancePruning` runs before `CommonWhiteSearch` | Phase 1 |
| **C2** | The null move isn't pushed to `MoveHistory`, so `GetPly()` is one lower in the null subtree. Results: wrong mate distances; ply-indexed `SearchContext`/`SortContext` reused; **killers of the wrong colour** (each ply shares one `KillerMoves` across all colour rows); counter, CMH and continuation suggestions built from the null side's *own* last move, so they never match. | `MoveHistoryService`, `DataPoolService`, `SortContext.SetInternal` | Phase 4 |
| **C3** | `NullWindowSearch*` sets `best = -child` instead of taking the max, so the returned value isn't a real bound. | `StrategyBase` | Phase 4 |
| **C4** | On a TT miss, `TranspositionHashSet.GetValue` returns `new TranspositionEntry()`, whose `PvMove` is probably 0. `GetMovesForNullSearch` checks only `pv < 0`, so move key 0 is treated as the hash move on every miss. | `TranspositionHashSet._default` | Phase 1 (interim), Phase 4 (deleted) |
| **C5** | No bounds check on `NullDepthReduction[depth]` / `NullDepthExtendedReduction[depth]`. | Config has 21 entries | Phase 1 |
| **V2** | `IsRepetition` steps back 2 plies and compares only `Board.Hash`, which has **no side-to-move key**. Inside the null subtree the parity is shifted, so it matches positions with the **opposite** side to move. Any match after the root returns draw (0), giving **false draws**. | `MoveHistoryService.IsRepetition`, `Board.InitializeZoobrist` | Phase 4 |
| **V3** | `SetNull()` sets `_nullMoves[_ply] = false`, and `AddWhite/AddBlack` copy the flag to children, so null is **disabled for the whole reduced subtree** after one fail-high. Correct verified null-move pruning lets that subtree keep pruning. | `MoveHistoryService` | Phase 2 |
| **V4** | The zugzwang guard looks at the whole board and `GetTotalNonKingPieces()` **counts pawns**. Example: White K+6P vs Black K+R = 7 pieces, so null is allowed for a side with only pawns (the classic zugzwang failure). | `Board.State.cs` | Phase 1 |
| **S1** | Stale `Ply` in the inner endgame strategy (`IdItemLmrDeepEndStrategy`). Its `Ply` is set only in its own `GetResult()`, which runs only when the **root** is an endgame. During middlegame searches it stays 0 or keeps an older root's value. Effects: `IsRepetition(Ply)` treats a single repetition of a pre-root position as a draw, and the null-move ply gate is effectively off. | `IdLmrDeepEndStrategy`, `IdItemLmrDeepEndStrategy` | Phase 1 |

Checked and **safe** (no change needed):
- **En passant after a null move:** `PawnOverAttack.IsLegal()` requires `history.IsLast(EnPassant.Key)`. Today the last move is the null side's own move; after Phase 4 it's the null placeholder, whose key never matches. In both cases the en-passant right is correctly lost.
- **TT colour separation:** there are separate `WhiteTable`/`BlackTable` keyed by `Board.Hash`.
- **SearchContext reuse at the null root:** the context is cleared and regenerated after the null search.

Known, out of scope: `Board.Hash` has no castling-rights key, which affects TT entries and repetition. **Do not change this here.**

---

## 3. Phase 0: instrumentation (no behaviour change)

Add a compile-time statistics class. Define `NULL_STATS` only in a diagnostic build configuration.

**New file `Engine/Strategies/Base/NullStats.cs`:**
```csharp
#if NULL_STATS
namespace Engine.Strategies.Base;

public static class NullStats
{
    public static long Nodes;            // all SearchWhite/SearchBlack entries
    public static long NullNodes;        // nodes visited while inside a null search
    public static long Tries;
    public static long FailHighs;
    public static readonly long[] TriesByDepth = new long[128];
    public static readonly long[] FailHighsByDepth = new long[128];
    public static int Nesting;           // > 0 while inside a null search

    public static void Reset()
    {
        Nodes = NullNodes = Tries = FailHighs = 0; Nesting = 0;
        Array.Clear(TriesByDepth); Array.Clear(FailHighsByDepth);
    }

    public static string Report()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Nodes={Nodes} NullNodes={NullNodes} ({(Nodes == 0 ? 0 : 100.0 * NullNodes / Nodes):F1}%) Tries={Tries} FailHighs={FailHighs} ({(Tries == 0 ? 0 : 100.0 * FailHighs / Tries):F1}%)");
        for (int d = 0; d < TriesByDepth.Length; d++)
            if (TriesByDepth[d] > 0)
                sb.AppendLine($"  depth {d}: tries={TriesByDepth[d]} fh={FailHighsByDepth[d]} ({100.0 * FailHighsByDepth[d] / TriesByDepth[d]:F1}%)");
        return sb.ToString();
    }
}
#endif
```

**Hooks in `StrategyBase.cs`** (all wrapped in `#if NULL_STATS ... #endif`):
- First line of `SearchWhite` and `SearchBlack`: `NullStats.Nodes++; if (NullStats.Nesting > 0) NullStats.NullNodes++;`
- First line of `NullWindowSearchWhite` and `NullWindowSearchBlack`: the same two statements.
- In `CalculateWhiteDepth` / `CalculateBlackDepth`, around the `NullWindowSearch*` call:
  ```csharp
  NullStats.Tries++; NullStats.TriesByDepth[depth]++; NullStats.Nesting++;
  // existing null search call
  NullStats.Nesting--;
  if (nullValue >= beta) { NullStats.FailHighs++; NullStats.FailHighsByDepth[depth]++; }
  ```

Print `NullStats.Report()` at the end of the bench and call `Reset()` before each bench position. Keep these hooks in later phases and move them with the code. In Phase 4 they go around `NullValueWhite/Black`.

**Acceptance:** with `NULL_STATS` undefined, the bench node count is identical to the baseline.

---

## 4. Phase 1: cheap safety and speed fixes

### 1a. `Board.State.cs`: side-to-move material helper (fixes V4)
Insert directly **after** the `IsZugzwangRisk()` method:
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public bool HasNonPawnMaterial(Turn turn)
{
    ref var b = ref _boards[0];
    return turn == Turn.White
        ? (Unsafe.Add(ref b, Pieces.WhiteKnight) | Unsafe.Add(ref b, Pieces.WhiteBishop) | Unsafe.Add(ref b, Pieces.WhiteRook) | Unsafe.Add(ref b, Pieces.WhiteQueen)).Any()
        : (Unsafe.Add(ref b, Pieces.BlackKnight) | Unsafe.Add(ref b, Pieces.BlackBishop) | Unsafe.Add(ref b, Pieces.BlackRook) | Unsafe.Add(ref b, Pieces.BlackQueen)).Any();
}
```

### 1b. Config: add `NullEvalMargin`
`NullConfiguration.cs` (add property):
```csharp
public int NullEvalMargin { get; set; }
```
`Configuration.json` → `NullConfiguration` (add):
```json
"NullEvalMargin": 0,
```

### 1c. `StrategyBase.cs`: fields and constructor (fixes C5)
Find the field `protected readonly int NullDepthThreshold;` and add after it:
```csharp
protected readonly int NullEvalMargin;
```
In the constructor, find:
```csharp
NullDepthReduction = nullConfiguration.NullDepthReduction;
NullDepthExtendedReduction = nullConfiguration.NullDepthExtendedReduction;
NullDepthThreshold = nullConfiguration.NullDepthThreshold + 1;
```
Replace with:
```csharp
NullDepthReduction = ExtendTable(nullConfiguration.NullDepthReduction, 128);
NullDepthExtendedReduction = ExtendTable(nullConfiguration.NullDepthExtendedReduction, 128);
NullDepthThreshold = nullConfiguration.NullDepthThreshold + 1;
NullEvalMargin = nullConfiguration.NullEvalMargin;
```
Add this private static method at the top of `#region Null Search`:
```csharp
private static sbyte[] ExtendTable(sbyte[] src, int size)
{
    var dst = new sbyte[size];
    int last = src.Length - 1;
    for (int i = 0; i < size; i++)
        dst[i] = i <= last ? src[i] : (sbyte)Math.Min(127, src[last] + (i - last));
    return dst;
}
```
For indexes ≤ 20 the values are unchanged; past the end they keep growing by 1 per depth.

### 1d. `StrategyBase.cs`: replace `CanUseNull` (fixes C1, V4; adds the static-eval check)
Cheapest checks come first:
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool CanUseNull(int beta)
{
    if (!MoveHistory.CanUseNull() || MoveHistory.IsLastMoveWasCheck()) return false;
    int mateThreshold = GetMateThreshold();
    if (beta >= mateThreshold || beta <= -mateThreshold) return false;          // C1
    if (MoveHistory.GetPly() - Ply < NullDepthThreshold) return false;
    if (!_board.HasNonPawnMaterial(Position.GetTurn())) return false;          // V4
    if (Position.GetValue() + NullEvalMargin < beta) return false;              // static-eval check
    return !IsLikelyZugzwangPosition();                                          // keep old guard for now
}
```
Notes:
- `Position.GetValue()` is from the side to move's point of view (`Evaluate()` / `EvaluateOpposite()`). It runs the full eval, which is much cheaper than a null search. If profiling shows it's hot, try `Position.GetStaticValue()` and test with SPRT.
- **This is an intended behaviour change:** null is no longer tried when static eval < beta.

### 1e. `StrategyBase.cs`: interim fix for C4
In `NullWindowSearchWhite`, find `GetMovesForNullSearch(depth, Table.GetWhite().PvMove)` and replace it with:
```csharp
var ttEntry = Table.GetWhite();
ref MoveHistoryList moves = ref GetMovesForNullSearch(depth, ttEntry.Depth > 0 ? ttEntry.PvMove : (short)-1);
```
(Delete the original `ref MoveHistoryList moves = ...` line.) Do the same in `NullWindowSearchBlack` with `Table.GetBlack()`. Phase 4 deletes this code anyway; this step only keeps Phase 1 measurements clean.

### 1f. Stale `Ply` in the endgame strategy (fixes S1)
**`StrategyBase.cs`**: add next to `SetExtensionThresholds`:
```csharp
internal virtual void SetRootPly(int ply) => Ply = ply;
```
Change `SetExtensionThresholds` to:
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
protected void SetExtensionThresholds(int ply)
{
    //MaxRecuptureExtensionPly = ply + RecuptureExtensionOffest;
    Ply = ply;
    MaxExtensionPly = ply + ExtensionOffest;
    MaxOneReplyPly = ply + OneReplyDepthDifference;
    _endGameStrategy?.SetRootPly(ply);
}
```
**`IdLmrDeepEndStrategy.cs`**: add inside the class:
```csharp
internal override void SetRootPly(int ply)
{
    base.SetRootPly(ply);
    _strategy.SetRootPly(ply);
}
```
This syncs only `Ply`. Do **not** touch the static `MaxExtensionPly` here.

**Acceptance for Phase 1:** builds; bench node count goes down (expected from the static-eval check); no crash at high depths; zugzwang suite not worse; SPRT ≥ non-regression.

---

## 5. Phase 4: real null move through the main search (single commit)

This phase fixes **C2, C3, C4 and V2** and gives the null subtree TT cutoffs and stores, LMR, futility, extensions and mate-distance pruning. Do it right after Phase 1, before any tuning.

### 4a. New file `Engine/Models/Moves/NullMove.cs`
```csharp
using System.Runtime.CompilerServices;
namespace Engine.Models.Moves;

/// <summary>Placeholder pushed to MoveHistory for a null move. Make/UnMake are no-ops.</summary>
public sealed class NullMove : MoveBase
{
    public NullMove(short key, byte piece) { Key = key; Piece = piece; }
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public override bool IsLegal() => false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public override void Make() { }
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public override void UnMake() { }
    public override string ToUciString() => "0000";
}
```
There is one placeholder per colour, so counter-move entries keyed by the null move ("the threat after a pass") don't mix colours. All `MoveBase` flags default to `false`; `IsIrreversible = false` is intentional.

### 4b. `MoveHistoryService.cs`
**Fields**: after `private bool[] _checks;` add:
```csharp
private short[] _lastNullPly;
private readonly MoveBase _whiteNull;
private readonly MoveBase _blackNull;
```
**Constructor**: after `_checks = new bool[historyDepth];` add:
```csharp
_lastNullPly = new short[historyDepth];
```
Find `SetCounterMoves(history.MovesCount);` and replace with:
```csharp
_whiteNull = new NullMove((short)history.MovesCount, Pieces.WhiteKing);
_blackNull = new NullMove((short)(history.MovesCount + 1), Pieces.BlackKing);
SetCounterMoves(history.MovesCount + 2);
```
**`AddFirst`**: add at the end: `_lastNullPly[_ply] = -1;`

**`AddWhite` and `AddBlack`**: directly after `_nullMoves[_ply] = _nullMoves[ply];` add:
```csharp
_lastNullPly[_ply] = _lastNullPly[ply];
```
**New methods**: add after `SetNull()`:
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
internal void AddNull(bool white)
{
    var ply = _ply;
    _history[++_ply] = white ? _whiteNull : _blackNull;
    _nullMoves[_ply] = _nullMoves[ply];
    _lastNullPly[_ply] = _ply;
    if (_ply >= 36) _phases[_ply] = _phases[ply];   // NEVER overwrite the preset opening/middle table (< 36)
    _reversibleMovesHistory[_ply] = (short)(_reversibleMovesHistory[ply] + _one);
    _castleHistory[_ply] = _castleHistory[ply];
    _checks[_ply] = false;
    _boardHistory[_ply] = _board.Hash;
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
internal bool IsLastMoveNull() => _lastNullPly[_ply] == _ply;
```
> **Trap:** `_phases` is filled in advance for plies < 36 in the constructor, and `SetPhase()` only writes from ply 36 onwards. Copying the parent phase unconditionally would permanently overwrite the preset values. Keep the `if (_ply >= 36)` guard.

**`IsRepetition(int rootPly)`** (fixes V2): find
```csharp
int offset = _ply - _reversibleMovesHistory[_ply];
```
inside `IsRepetition` (**not** `IsThreefoldRepetition`) and replace with:
```csharp
int offset = Math.Max(_ply - _reversibleMovesHistory[_ply], _lastNullPly[_ply]);
```
The backward scan then stops at the last null move, so positions with the wrong side to move are never compared.

**`Resize(int offset)`**: add next to the other resizes:
```csharp
EnumerableExtensions.Resize(ref _lastNullPly, offset);
```
`Remove()` needs no change: it calls the placeholder's no-op `UnMake()`.

### 4c. `Position.cs`
Add after `UnMakeBlack()`:
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public void MakeNullWhite()   { _moveHistoryService.AddNull(true);  _turn = Turn.Black; }
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public void UnMakeNullWhite() { _moveHistoryService.Remove();       _turn = Turn.White; }
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public void MakeNullBlack()   { _moveHistoryService.AddNull(false); _turn = Turn.White; }
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public void UnMakeNullBlack() { _moveHistoryService.Remove();       _turn = Turn.Black; }
```

### 4d. `StrategyBase.cs`
**Delete**: `CalculateWhiteDepth`, `CalculateBlackDepth`, `GetNullDepth`, `NullWindowSearchWhite`, `NullWindowSearchBlack`, `GetMovesForNullSearch`.

**Add** (in `#region Null Search`):
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private int NullValueWhite(int beta, sbyte depth)
{
#if DEBUG
    ulong h = _board.Hash; int p = MoveHistory.GetPly();
#endif
    Position.MakeNullWhite();
    int v = -SearchBlack(-beta, NullWindow - beta, NullDepthReduction[depth]);
    Position.UnMakeNullWhite();
#if DEBUG
    System.Diagnostics.Debug.Assert(h == _board.Hash && p == MoveHistory.GetPly() && Position.GetTurn() == Turn.White);
#endif
    return v;
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
private int NullValueBlack(int beta, sbyte depth)
{
#if DEBUG
    ulong h = _board.Hash; int p = MoveHistory.GetPly();
#endif
    Position.MakeNullBlack();
    int v = -SearchWhite(-beta, NullWindow - beta, NullDepthReduction[depth]);
    Position.UnMakeNullBlack();
#if DEBUG
    System.Diagnostics.Debug.Assert(h == _board.Hash && p == MoveHistory.GetPly() && Position.GetTurn() == Turn.Black);
#endif
    return v;
}
```
The null-search depth is unchanged (`NullDepthReduction[depth]`), so node-count changes come only from the fixes and the main-search features.

**Update `CanUseNull`**: add the double-null check to the first condition:
```csharp
if (!MoveHistory.CanUseNull()
    || MoveHistory.IsLastMoveWasCheck()
    || MoveHistory.IsLastMoveNull()) return false;
```

**Null block in all four methods** (`CommonWhitePvSearch`, `CommonWhiteNonPvSearch`, `CommonBlackPvSearch`, `CommonBlackNonPvSearch`). Replace:
```csharp
if (CanUseNull(beta))
{
    depth = CalculateWhiteDepth(beta, depth);
    if (depth < 1)
        return EvaluateWhite(alpha, beta);
}
```
with (White shown; for Black use `NullValueBlack` and `EvaluateBlack`):
```csharp
if (CanUseNull(beta))
{
    int nullValue = NullValueWhite(beta, depth);
    if (nullValue >= beta)
    {
        MoveHistory.SetNull();
        depth = NullDepthExtendedReduction[depth];
        if (depth < 1) return EvaluateWhite(alpha, beta);
    }
}
```
Move the Phase 0 `NullStats` hooks around `NullValueWhite/Black`.

### 4e. `DataPoolService.cs`
Delete `GetCurrentNullSortContext()`, which no longer has callers.

**Acceptance for Phase 4:**
- DEBUG build: the asserts never fire over the full bench and a short self-play run.
- Bench: fewer nodes expected (TT cutoffs and LMR inside the null subtree). Best moves may change slightly, because killers, mate distances and repetition are now correct.
- Zugzwang/endgame suite plus SPRT against the Phase 1 build. If SPRT fails, revert the whole commit.

---

## 6. Phase 2: verified null-move pruning (prune shallow, verify deep; fixes V3)

Reuse `_nullMoves` as the **"verification active"** flag:
- **Flag on:** a fail-high triggers the reduced verification search (current behaviour).
- **Flag off** (inside a verified subtree): a fail-high **prunes** instead of disabling null.

### 2a. Config
`NullConfiguration.cs`: `public int NullVerificationDepth { get; set; }`
`Configuration.json`: `"NullVerificationDepth": 0,`
`StrategyBase.cs`: field `protected readonly int NullVerificationDepth;` and in the constructor `NullVerificationDepth = nullConfiguration.NullVerificationDepth;`

### 2b. `CanUseNull`
**Remove** `!MoveHistory.CanUseNull() ||` from the first condition. Keep the check and double-null tests.

### 2c. Fail-high branch in the four `Common*Search` methods
```csharp
if (nullValue >= beta)
{
    if (!MoveHistory.CanUseNull() || depth < NullVerificationDepth)
        return nullValue >= GetMateThreshold() ? beta : nullValue;   // prune; never return an unproven mate
    MoveHistory.SetNull();                                            // verify once; subtree prunes freely
    depth = NullDepthExtendedReduction[depth];
    if (depth < 1) return EvaluateWhite(alpha, beta);                 // EvaluateBlack in Black methods
}
```
**Tuning order:**
1. `NullVerificationDepth = 0`. The only change is that verified subtrees now prune instead of skipping null. Run SPRT.
2. Then try `NullVerificationDepth` = 8, 10, 12 (pure pruning below that depth; this removes the double qsearch at depths 1–3 and the re-search at middle depths). Run SPRT for each.
3. Optional: a separate, more conservative value for the endgame strategy, where zugzwang is most likely.

---

## 7. Phase 3: adaptive reduction R

Replace the fixed `NullDepthReduction[depth]` lookup in `NullValueWhite/Black` with `GetNullSearchDepth(beta, depth)`.

### 3a. Config (`NullConfiguration.cs` + `Configuration.json`)
| Property | Start value | Meaning |
|---|---|---|
| `NullFormulaMinDepth` | 128 | Below this depth use the old table (128 = formula off) |
| `NullBaseReduction` | 4 | Plies removed, including the null ply (matches today's `depth−4`) |
| `NullDepthDivisor` | 1000 | Extra reduction `depth / divisor` (1000 = off) |
| `NullEvalDivisor` | 200 | Extra reduction `(eval − beta) / divisor` |
| `NullEvalMaxReduction` | 0 | Cap for the eval term (0 = off) |

### 3b. `StrategyBase.cs`
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private sbyte GetNullSearchDepth(int beta, sbyte depth)
{
    if (depth < NullFormulaMinDepth) return NullDepthReduction[depth];
    int evalTerm = Math.Clamp((Position.GetValue() - beta) / NullEvalDivisor, 0, NullEvalMaxReduction);
    int d = depth - NullBaseReduction - depth / NullDepthDivisor - evalTerm;
    return (sbyte)(d < 0 ? 0 : d);
}
```
Call it **before** `MakeNull*`, because eval is from the side to move's point of view.

**Tuning order** (one change per SPRT):
1. `NullFormulaMinDepth = 8` with the other defaults. This is identical to today for depth ≥ 8, a sanity check.
2. `NullDepthDivisor = 6`, then 4.
3. `NullEvalMaxReduction = 2`, then 3, with `NullEvalDivisor` ≈ 200.

---

## 8. Phase 5: simplify the zugzwang guard

Now that `HasNonPawnMaterial(side to move)` and verification are in place, test whether `IsLikelyZugzwangPosition()` can be removed from `CanUseNull`, or reduced to `_board.IsZugzwangRisk()` only:
- variant A: remove the call entirely;
- variant B: keep it only in `Phase.End`.

Accept only with SPRT non-regression **and** no regressions on the zugzwang/endgame suite.

---

## 9. Phase 6: config cleanup

1. Rename `NullDepthThreshold` → `NullMinPlyFromRoot` and remove the hidden `+ 1` (set JSON value to 2).
2. If the Phase 3 formula wins, delete `NullDepthReduction` (keep it only if `NullFormulaMinDepth > 1`) and document the meaning of every remaining value in a comment in `NullConfiguration.cs`.
3. Turn `NullWindow` into a constant `1` (a non-1 value has no use with a zero-window search).
4. Run a final non-regression SPRT.

---

## 10. Validation for every phase

1. **Build** Release and DEBUG. The DEBUG asserts must stay silent.
2. **Bench:** fixed depth over a set of about 50 positions. Report total nodes, time to depth, NPS, best-move changes and the `NullStats` report.
3. **Tactics and zugzwang suites:** pawn endgames, K+P vs K+minor, mutual-zugzwang positions. No new failures.
4. **SPRT self-play** (cutechess or fastchess) against the previous phase's build. Use gain bounds for Phases 1–4 and non-regression bounds for Phases 5–6. Keep long runs within the stable power limits configured for the test PC.
5. If a phase fails SPRT, **revert that phase** and report the numbers. Don't try to fix it by changing unrelated parameters.

## 11. Agent report format (after each phase)
```
Phase: <n>
Files changed: <list>
Build: OK / FAIL (<errors>)
DEBUG asserts: silent / fired (<where>)
Bench: nodes <before> -> <after>, time <before> -> <after>, best-move diffs <count>
NullStats: tries, fail-high %, null-node %
Suites: <pass/fail counts>
SPRT: <result / not run>
Open questions: <anything ambiguous; anchors not found>
```
