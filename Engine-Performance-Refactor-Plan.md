# Kio-Chess Engine — Performance Refactor Plan

> **Scope:** `KioChess/Engine/Engine.csproj` — the search engine (`Strategies/*`) and the
> data structures (`DataStructures/*`, `Models/Boards/*`, `Services/*`) on the hot path.
> **Goal:** Make the engine faster while producing **identical search results**
> (same best move, same score, same node counts for behaviour-preserving items).
> **Status of code:** No code has been changed yet. This document is the plan.

---

## How to use this document

- Each task has a checkbox. When you finish a task, change `- [ ]` to `- [x]` and fill in the
  **Measured result** line so the file stays an accurate, living log.
- Tasks are grouped into **Tiers** ordered by *(impact ÷ effort)*. Do them top-to-bottom.
- Every task is tagged with a **Result impact** label:
  - **`IDENTICAL`** — bit-for-bit same search output (safe; verify with the node-count bench).
  - **`ORDERING-ONLY`** — same legal results, but move-ordering heuristics may differ, so node
	counts can change slightly. Strength must be re-validated, not just node counts.
  - **`CONFIG`** — build/runtime configuration only; no source behaviour change.

---

## Executive summary

The engine is already heavily hand-optimised (bitboards + magic bitboards, `Span`/`Unsafe`,
`[InlineArray]` buffers, `[SkipLocalsInit]`, pooled per-ply context objects, an AOS cache-line
aligned transposition table, and colour-specialised search to avoid per-node branches). The
remaining wins are therefore **algorithmic and memory-locality** changes rather than
micro-tuning. The five biggest opportunities are:

| # | Opportunity | Where | Expected* | Result impact |
|---|-------------|-------|-----------|----------------|
| 1 | **Static-evaluation cache** (eval keyed by Zobrist hash) | `Board.Evaluation`, `StrategyBase` | 5–15% | IDENTICAL |
| 2 | **Lazy / staged move generation** (don't sort 21 bins when move #1 cuts off) | `MoveCollection`, `SortContext` | 10–25% | IDENTICAL |
| 3 | **Devirtualise `move.Make()/UnMake()`** + enable Dynamic PGO | `MoveBase`, `Position`, `.csproj` | 5–15% | IDENTICAL / CONFIG |
| 4 | **Replace per-node `Dictionary` heuristic look-ups** with open-addressed arrays | `MoveHistoryService` | 3–8% | ORDERING-ONLY |
| 5 | **Shrink/repack `MoveBase`** (pack 11 `bool`s, reorder hot fields) | `MoveBase` | 3–8% | IDENTICAL |

\* *Estimates only.* All numbers **must be confirmed** with the measurement protocol below;
gains overlap and are not additive.

---

## Measurement & validation protocol (read before changing anything)

A refactor is only "the same result" if you can prove it. Establish a baseline **first**.

1. **Build Release.** The perf-critical MSBuild flags only apply to `Release|AnyCPU`
   (`Engine.csproj` lines 13–37).
2. **Determinism check (node-count bench).** Run a fixed set of positions at a **fixed depth**
   with the transposition table cleared between positions, and record for each position:
   `bestMove`, `score`, and `nodes`. This triple is your golden file.
   - For **`IDENTICAL`** tasks the golden triple must be **unchanged** after the refactor.
   - For **`ORDERING-ONLY`** tasks `nodes` may move; `bestMove`/`score` at fixed depth should
	 normally match, and overall strength must be checked by self-play.
3. **Speed metric.** Record **NPS** (nodes/sec) and **time-to-depth** on the same suite. This is
   what should improve.
4. **Move-gen correctness.** For anything touching generation, run **perft** to known values.
5. **Existing harnesses to reuse:**
   - `KioChess/BenchmarkTool/BenchmarkTool.csproj` — driver for timed runs.
   - `KioChess/Engine/Tools/MoveGenerationPerformance.cs` and `LmrParityPerformance.cs` — timing buckets.
   - `KioChess/StockFishComparer/*` — strength/parity comparison vs reference.
6. **Profile, don't guess.** Confirm each hypothesis with a sampling profiler (Visual Studio
   Performance Profiler / `dotnet-trace`). Optimise the function that the profiler says is hot.

> Suggested baseline table to fill in once and keep:
>
> | Suite | Depth | Total nodes | NPS (base) | NPS (now) | Notes |
> |-------|------:|------------:|-----------:|----------:|-------|
> | (fill) |     |             |            |           |       |

---

## Baseline architecture notes (why the hot path is what it is)

- **Per interior node** the engine: probes the TT (`TranspositionTable.GetWhite/GetBlack`),
  optionally runs a null-move search, **generates and fully sorts all moves**
  (`SortContext.GetAllMoves` → `Position.ProcessRegularMoves` → `MoveCollection.Build*`), then
  loops moves calling `MoveProvider.Get(key)` → `Position.MakeWhite/Black` → recurse →
  `UnMake`.
- **Per leaf / quiescence node** it calls `Board.Evaluate()` /`EvaluateOpposite()`, which calls
  `ComputeAttacks()` to **recompute every sliding-piece attack set from scratch**, then the
  phase-specific evaluator.
- **Moves are shared singleton objects.** `MoveProvider.Get(short key) => _all[key]`
  (`MoveProvider.Core.cs:350`) returns one `MoveBase` instance per move key. The search keeps
  lightweight `MoveHistory` structs (`Key`,`History`) in an `[InlineArray(128)]` buffer and
  re-fetches the heavy object by key inside every loop.

These three facts drive Tiers 1–3 below.

---

# Tier 0 — Quick wins (CONFIG, minutes, zero logic change)

### - [x] T0.1 Enable Dynamic PGO + full optimisation for hot search methods  `CONFIG`
**Problem.** `move.Make()`/`UnMake()` are `abstract`/`virtual` (`MoveBase.cs`), called through a
`MoveBase` reference in `Position.MakeWhite/MakeBlack` (`Position.cs:682,697`). The concrete
classes (`WhiteMove`, `BlackMove`, …) are already `sealed`, but the call site sees only the base
type, so the JIT cannot statically devirtualise. **Dynamic PGO** adds *guarded devirtualisation*
(it inlines the hot concrete type behind a cheap type check) and improves block layout.

**Change (`Engine.csproj`, Release PropertyGroup):**
```xml
<!-- add -->
<TieredPGO>true</TieredPGO>
```
Keep `TieredCompilation=true` and `TieredCompilationQuickJit=false` (already set). Optionally mark
the recursive search entry points so they skip tiering and are born fully optimised:
```csharp
// StrategyBase.SearchWhite / SearchBlack / CommonWhiteSearch / CommonBlackSearch / EvaluateWhite / EvaluateBlack
[MethodImpl(MethodImplOptions.AggressiveOptimization)]
```
**Why faster.** Guarded devirtualisation removes the vtable indirection on the single
most-executed call (`move.Make`) and lets it inline into `MakeWhite`. PGO-driven hot/cold
splitting shrinks the I-cache footprint of the giant `StrategyBase`.
**Validation.** `IDENTICAL` golden triple; compare NPS.
**Measured result:** Applied. `TieredPGO=true` added to `Engine.csproj` Release PropertyGroup. `AggressiveOptimization` added to `SearchWhite`, `SearchBlack`, `CommonWhiteSearch`, `CommonBlackSearch`, `EvaluateWhite`, `EvaluateBlack` in `StrategyBase.cs`; `SearchInternalWhite`/`SearchInternalBlack` in `LmrStrategyBase.cs`; `SearchWhite`/`SearchBlack` in `IdItemLmrDeepEndStrategy.cs` and `IdLmrDeepEndStrategy.cs`. Build: ✓. NPS delta: _(measure in Release)_### - [x] T0.2 Pin GC mode for the search host  `CONFIG`
**Problem.** Background/concurrent GC threads can preempt the single hot search thread.
**Change (host app `.csproj`, e.g. `KioChess.App`/`GsServer`/`BenchmarkTool`):**
```xml
<ServerGarbageCollection>false</ServerGarbageCollection>
<ConcurrentGarbageCollection>false</ConcurrentGarbageCollection>
<TieredPGO>true</TieredPGO>
```
**Why faster.** The search is allocation-light (pooled contexts), so workstation **non-concurrent**
GC avoids background GC CPU steal and gives more deterministic timings. Measure both ways —
on some boxes Server GC wins.
**Validation.** `IDENTICAL`; compare NPS variance across 3 runs.
**Measured result:** Applied. `GsServer.csproj` Release PropertyGroup: `ServerGarbageCollection=false`, `ConcurrentGarbageCollection=false`, `TieredPGO=true`. `KioChess.App.csproj`: `TieredPGO=true` only (GC disable omitted — WPF UI thread would see longer pauses). `BenchmarkTool.csproj`: `TieredPGO=true` only (BenchmarkDotNet manages GC per-job). NPS variance delta: _(measure in Release)_

### - [ ] T1.1 Static-evaluation cache (eval hash)  `IDENTICAL`
**Problem.** `Board.Evaluate()` / `EvaluateOpposite()` (`Board.Evaluation.cs:12,23`) call
`ComputeAttacks()` (`Board.Attacks.cs:35`) which rebuilds **all** bishop/rook/queen attack sets
with magic-bitboard look-ups, then runs the full phase evaluator. The same position is evaluated
repeatedly — as the quiescence stand-pat (`EvaluateWhite/Black`), as the futility/razoring static
value (`GetSearchResultType` → `Position.GetValue()` → `Evaluate()`, `StrategyBase.cs`), and again
whenever it is reached via a transposition. None of these results are cached.

**Change.** Add a small direct-mapped eval cache keyed by `_board.Hash` (the Zobrist key is already
maintained incrementally in `Board.Manipulation.cs`). Sketch:
```csharp
// new: EvaluationCache.cs
[StructLayout(LayoutKind.Sequential)]
private struct EvalEntry { public ulong Key; public int Value; } // 12–16 bytes

private readonly EvalEntry[] _evalCache = new EvalEntry[1 << 16]; // power of two
private readonly ulong _evalMask = (1 << 16) - 1;

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public int Evaluate()
{
	ulong h = Hash;
	ref EvalEntry e = ref _evalCache[h & _evalMask];
	if (e.Key == h) return e.Value;          // hit: skip ComputeAttacks + full eval

	ComputeAttacks();
	var phase = _moveHistory.GetPhase();
	_evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);
	int v = phase == Phase.Middle ? EvaluateMiddle()
		  : phase == Phase.End    ? EvaluateEnd()
		  :                         EvaluateOpening();
	e.Key = h; e.Value = v;                   // store
	return v;
}
```
**Caveats that keep it `IDENTICAL`:**
- Evaluation must be a **pure function of the position** at the cache key. The current evaluator
  reads `_moveHistory.GetPhase()`, and phase is a function of ply, not only of the board — so the
  cache must key on `(Hash, phase)` *or* be reset across phase boundaries. Easiest correct version:
  fold `phase` into the stored key (`Key == h` → also compare a stored `byte Phase`), or clear the
  cache when phase changes. Verify with the golden triple.
- `Evaluate()` and `EvaluateOpposite()` return *negatives of the same quantity* for the side to
  move; cache the **white-relative** score once and negate, or use two keys. Do **not** share one
  slot between the two sign conventions.

**Why faster.** `ComputeAttacks()` is the single most expensive per-eval step (loops every slider,
two magic look-ups each). Eliminating it on a cache hit — typical hit rates in qsearch/futility are
high because the same nodes recur — directly cuts leaf cost. A **pawn-structure hash** (cache only
the pawn terms keyed by a pawn Zobrist key) is an even safer incremental follow-up.
**Validation.** Golden triple **unchanged**; NPS up; check `_evalCache` hit-rate counter in a debug
build.
**Measured result:** _(fill in)_

### - [ ] T1.2 Lazy / staged move generation with early cut-off  `IDENTICAL`
**Problem.** Every interior node generates **and sorts all moves up front**. `MoveCollection`
holds **21** separate `MoveHistoryList` bins (`MoveCollection.cs:9–28`) and
`BuildComplexInternal` runs ~16 `SortCopyClear`/`CopyClear` passes (each an insertion sort plus a
memcpy) *before the first move is tried* (`MoveCollection.cs:284`). In a well-ordered alpha-beta
tree the **first move causes a beta cut-off ~85–95% of the time**, so the work spent sorting the
remaining 20 bins is thrown away.

**Change.** Convert generation into **stages** consumed on demand by the move loop:
1. **Hash move** (TT `PvMove`) — emit immediately, search it.
2. **Winning captures / promotions** (already SEE-classified) — generate+sort, search.
3. **Killers / counter / CMH** quiet refutations.
4. **Remaining quiets** — generate+sort only if we get here.
5. **Losing captures / "bad"** last.

Drive it from the search loop, e.g. replace the single `context.Moves` walk in
`SearchInternalWhite/Black` with a `while (stage.MoveNext(out move))` that lazily fills the next
bin only when the previous bins are exhausted without a cut-off. The **order must be byte-for-byte
the same** as today's concatenation order in `BuildComplexInternal` so results stay `IDENTICAL`.

**Why faster.** On a cut-off after move 1–3 you never categorise/sort stages 4–5. You also skip the
final big `MoveHistoryList` copies. This is the highest-value classical optimisation for an engine
that currently eager-sorts everything.
**Risk/effort.** Medium-high (touches generation + search loops). Do it **incrementally**: first
split out the hash move only (search TT move before generating anything), measure, then peel off
captures, etc. Each increment is independently `IDENTICAL`-verifiable.
**Validation.** Golden triple **unchanged** at every increment; perft unaffected; NPS up.
**Measured result:** _(fill in)_

### - [ ] T1.3 Devirtualise `Make`/`UnMake` on the move loop  `IDENTICAL`
**Problem.** Even with PGO (T0.1), the cleanest win is to remove the virtual dispatch entirely.
`Position.MakeWhite/MakeBlack` call `move.Make()` virtually for every move of every node.

**Change (pick one):**
- **(a) Type-tag + switch.** Add a `byte Kind` to `MoveBase` set at construction
  (Simple/Capture/Promotion/PromotionAttack/EnPassant/Castle/PawnOver). Replace `move.Make()` with
  a `switch (move.Kind)` that calls **sealed, inlinable** concrete helpers. The JIT inlines each arm.
- **(b) Function pointer.** Store `delegate*<MoveBase,void>` in the move; call through it. Removes
  the vtable walk; slightly less inlinable than (a).
- Combine with **T1.1**: `MakeWhite` already does `move.IsCheck = _board.IsCheckToBlack();` — fine
  to keep.

**Why faster.** `Position.MakeWhite/MakeBlack` is executed once per move per node (millions/sec).
The board mutation itself (`Board.MoveWhite`) is tiny, so the virtual call + non-inlining is a real
fraction of make/unmake cost. Devirtualising lets the bitboard update inline.
**Validation.** `IDENTICAL`; perft unchanged; NPS up.
**Measured result:** _(fill in)_

---

# Tier 2 — Move-ordering cost reduction

### - [ ] T2.1 Replace per-node `Dictionary` heuristic look-ups with open-addressed arrays  `ORDERING-ONLY`
**Problem.** `GetHeuristicMoves()` (`MoveHistoryService.cs:371`) runs **two `Dictionary`
look-ups per node** — `_countermoveHistory` (`Dictionary<int,short>`) and `_continiousMoveHistory`
(`Dictionary<long,short>`) — and `SetCountermoveHistory` writes them on every cut-off. `Dictionary`
hashing + bucket chasing is far slower and more cache-hostile than array indexing, and this runs at
every sort-context setup.

**Change.** Use fixed-size **power-of-two open-addressed tables** with masking (a "history hash"):
```csharp
// size = 1<<N; index = (key * 0x9E3779B1u) >> (32-N); store (key, move) and compare key on read.
```
Collisions only cause a heuristic *miss* (slightly different ordering), never a correctness bug —
this is why the label is `ORDERING-ONLY`. The flat `_counterMoves` array already in this class
(`MoveHistoryService.cs`) is the model to follow.
**Why faster.** Removes managed `Dictionary` overhead and indirections from the per-node path;
predictable, cache-friendly memory access.
**Validation.** `bestMove`/`score` at fixed depth should still match on the suite; run **self-play
(several hundred games)** to confirm no strength regression, since node counts may shift.
**Measured result:** _(fill in)_

### - [ ] T2.2 Integer relative-history; trim redundant ordering work  `ORDERING-ONLY`
**Problem.** `MoveBase.SetRelativeHistory()` uses **floating-point** math per move:
`(int)((History * _inverseHistoryFactor) / Butterfly)` (`MoveBase.cs`). `ComplexSorter.SetValues()`
also computes full board mobility (`CountTotalWhite/BlackMobility`) **every node** for non-end
phases, even when the first move cuts off (compounds with T1.2).
**Change.** Use integer/fixed-point relative history (e.g. `History * K / Butterfly` with integer
`K`); defer the mobility computation until the stage that actually needs it (after T1.2 lands).
**Why faster.** Removes FP→int conversions from the inner sort comparisons; avoids computing
mobility for nodes that cut off early.
**Validation.** `ORDERING-ONLY`; self-play strength check + NPS.
**Measured result:** _(fill in)_

---

# Tier 3 — Data-structure & memory-layout

### - [ ] T3.1 Repack `MoveBase` (pack flags, order hot fields first)  `IDENTICAL`
**Problem.** `MoveBase` (`MoveBase.cs`) carries **11 separate `bool` fields**
(`IsCheck`,`IsAttack`,`IsCastle`,`IsPromotion`,`IsEnPassant`,`CanReduce`,`CanNotReduceNext`,
`IsIrreversible`,`IsFutile`,`IsQuiet`,`IsPromotionExtension`) plus several `int`s and a `BitBoard`.
Because moves are shared singletons fetched by key inside the inner loop, every
`move.IsQuiet`/`IsCheck`/`CanReduce` read is a pointer-chase into a large, sparsely-used object →
extra cache lines.
**Change.** Pack the booleans into a single `int Flags` bitset (expose the old names as
`[MethodImpl(AggressiveInlining)]` properties doing `(Flags & Mask) != 0`), and **reorder fields**
so the search-hot members (`Key`,`Flags`,`History`,`Butterfly`,`RelativeHistory`) sit first, within
one cache line. Keep public API identical so call sites don't change.
**Why faster.** Shrinks the object and clusters the fields the search actually touches, cutting
cache-line loads per move. Bit tests are register-cheap.
**Risk.** Mechanical but wide (many flag write sites in `MoveProvider.*`/sorters). Change
field→property and update the writers; the compiler finds every site.
**Validation.** `IDENTICAL` golden triple; NPS up.
**Measured result:** _(fill in)_

### - [ ] T3.2 Transposition-table prefetch + keep the 64-byte bucket  `IDENTICAL`
**Problem.** `TranspositionHashSet` is a nice AOS, exactly **64-byte cache-line** `Bucket` with 5
entries (`TranspositionHashSet.cs:30`). The probe in `CommonWhiteSearch/BlackSearch` happens right
after entering the node, but the bucket address is known earlier (it's `Hash & _mask`).
**Change.** Issue a software prefetch for `_buckets[key & _mask]` as soon as the node's hash is
known (e.g. right after `Make`), so the line is resident by the time `GetWhite/GetBlack` reads it:
```csharp
System.Runtime.Intrinsics.X86.Sse.Prefetch0(ref Unsafe.As<Bucket,byte>(ref _buckets[key & _mask]));
```
**Do not** add a static-eval field to `TranspositionEntry`: it would push `BucketEntry` past 12
bytes and break the 5-per-64-byte layout. Use the separate eval cache from **T1.1** instead.
**Why faster.** Overlaps TT memory latency with useful work; TT probes are classic cache-miss
sites.
**Validation.** `IDENTICAL`; NPS up (CPU-dependent — measure; revert if neutral).
**Measured result:** _(fill in)_

### - [ ] T3.3 Re-examine the TT cut-off guard `entry.Depth < CutoffDepth`  `ORDERING-ONLY`
**Problem.** The TT cut-off requires `entry.Depth >= depth && entry.Depth < CutoffDepth`
(`StrategyBase.CommonWhiteSearch/BlackSearch`). The upper-bound guard suppresses otherwise-valid
TT cut-offs near the root, forcing re-search. This may be intentional (PV stability), but it costs
nodes.
**Change.** A/B test removing or raising the `< CutoffDepth` clause; measure node reduction vs any
best-move instability.
**Why faster.** More TT cut-offs = fewer nodes.
**Validation.** `ORDERING-ONLY`; compare nodes + self-play strength; keep only if neutral-or-better.
**Measured result:** _(fill in)_

---

# Tier 4 — Micro-optimisations (do last, only if the profiler flags them)

### - [ ] T4.1 Keep LINQ off any search-reachable path  `IDENTICAL`
`Position.GetAllMoves(byte,byte)` and `GetAllMovesForColor` use `List`/`.Where`/`.Take`
(`Position.cs:64–164`) and `MoveHistoryService.GetHistory()` uses `_history.Take(...)`
(`MoveHistoryService.cs:310`). These are the **UI/perft/debug** paths, not the search loop — keep
them out of search and they cost nothing. Add a comment/guard so they don't creep onto the hot
path. **Measured result:** _(fill in)_

### - [ ] T4.2 Branch-lean phase/turn row selection  `IDENTICAL`
`DataPoolService.EnsureRows()` recomputes `(GetPhase()<<1 | turn)` and compares to a cached row
each call. Already cheap; only revisit if profiling shows it. **Measured result:** _(fill in)_

---

## Suggested execution order (dependency-aware)

1. **T0.1, T0.2** — config, instant, establish new baseline.
2. **T1.1 eval cache** — biggest single leaf-cost cut, fully `IDENTICAL`.
3. **T1.2 lazy movegen (incremental)** — biggest interior-node cut; land hash-move-first slice
   before the rest.
4. **T1.3 devirtualise make/unmake.**
5. **T3.1 repack `MoveBase`** — compounds with T1.2/T1.3 (fewer, denser field reads).
6. **T2.1, T2.2** — ordering-cost reductions (require self-play sign-off).
7. **T3.2, T3.3, T4.x** — measure-and-keep micro wins.

After each task: re-run the **golden triple** (or self-play for `ORDERING-ONLY`) and update the
checkbox + **Measured result** line here.

---

## Progress log

| Date | Task | NPS before | NPS after | Δ | Result check | Notes |
|------|------|-----------:|----------:|---|--------------|-------|
|      | T0.1 |            |           |   | _(run golden triple in Release)_ | `TieredPGO` + `AggressiveOptimization` on 10 hot methods |
|      | T0.2 |            |           |   | _(run golden triple in Release)_ | Non-concurrent GC on GsServer; TieredPGO on all host exes |
|      | T1.1 |            |           |   |              |       |
|      | T1.2 |            |           |   |              |       |
|      | T1.3 |            |           |   |              |       |
|      | T3.1 |            |           |   |              |       |

> Keep this table updated — it is the quickest way to see what is done and what each change bought.
