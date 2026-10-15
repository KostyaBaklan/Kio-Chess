# Null-Move Refactor Plan (Kio-Chess)

Branch: `125-Null/03-Draw`. Status: plan only, no code changed.
Constraints: current null move "works well"; performance is a first-class requirement;
the main search procedure / main TT must not be reused for the null search.

Legend: [V] verified in code, [H] hypothesis (needs measurement), [S] speculation.

---------------------------------------------------------------------------------------------------

## 1. Verified facts about the current implementation

| # | Fact | Where |
|---|------|-------|
| F1 | Null move is a turn flip only: `Position.SetXTurn()`. No history entry, no ply change, no hash change. | `StrategyBase.CalculateWhiteDepth/CalculateBlackDepth` |
| F2 | Null search is a private full-width mini-searcher (`NullWindowSearchWhite/Black`): no TT, no killers/LMR/extensions, no null recursion, ends in qsearch (`EvaluateX`). Moves come from `GetMovesForNullSearch` = evaluation sort context + `Sorters[depth]` + TT PV move. | `StrategyBase` `#region Null Search` |
| F3 | A null fail-high does NOT cut. It lowers depth to `NullDepthExtendedReduction[depth]`, disables null in the whole subtree (`SetNull()` -> `_nullMoves[ply]=false`, inherited by `AddWhite/AddBlack`), then searches normally. If the reduced depth < 1 it returns `EvaluateX(alpha, beta)`. | `GetNullDepth`, `CommonXxxSearch` |
| F4 | `Board.Hash` = piece-square keys + castling keys. No side-to-move key. No en-passant key. | `Board.Manipulation.cs`, `Board.Castling.cs` |
| F5 | The TT is split per colour (`WhiteTable`/`BlackTable`), so the side to move is implicit in the table choice. | `TranspositionTable` |
| F6 | `IsRepetition(rootPly)` scans `_boardHistory[i]` for `i = ply-4, ply-6, ...`, assuming equal parity == equal side to move. | `MoveHistoryService` |
| F7 | En passant legality = `history.IsLast(EnPassant.Key) && EnPassant.IsEnPassant` (last history entry must be the double push). | `PawnOverAttack.IsLegal` |
| F8 | `Board.EvaluateCached` is keyed by `Hash` (+phase+castle rights). The evaluation is white-relative, i.e. side independent. | `Board.Evaluation.cs` |
| F9 | `SearchContext.Add(key)` writes killers (shared per ply in `DataPoolService`), `SetCounterMove` and `SetCountermoveHistory`, all keyed by `_history[_ply].Key`. Search loops also do `move.History += d*d` and `move.Butterfly++` on the shared `MoveBase` objects. | `SearchContext`, `StrategyBase.SearchInternalX` |
| F10 | `_counterMoves` is `new short[MovesCount]` indexed by `Key`. An out-of-range key throws. | `MoveHistoryService.SetCounterMoves/GetHeuristicMoves` |
| F11 | The current null searcher never calls `context.Add`, so it does not write killers/history/counter moves. It only reads them via the sort context (with the last real move's key, semantically wrong for the flipped side but harmless). | `GetMovesForNullSearch` |
| F12 | `AddWhite/AddBlack` write `_sequence[_ply]` (opening-book key sequence) when `_ply < _depth`. | `MoveHistoryService` |
| F13 | `SetPhase()` only recomputes from ply 36; before that `_phases` is pre-filled by ply index. | `MoveHistoryService.SetPhase` |
| F14 | `StrategyBase` ctor accepts a `TranspositionTable`, builds margins/sorters, resolves singletons, and calls virtual `CreateEndGameStrategy()` (end strategies return `null` to stop recursion). `SearchWhite/Black` and `SearchInternalWhite/Black` are `virtual`. `LmrStrategyBase` takes `(depth, position, table, lmrTables)`. | `StrategyBase`, `LmrStrategyBase`, `IdLmrDeepEndStrategy` |
| F15 | `DataPoolService` contexts (`SearchContext`, `SortContext`) are indexed by `MoveHistoryService.GetPly()`; killers live in them. The main node calls null BEFORE acquiring its own context at ply P. | `DataPoolService`, `CommonXxxSearch` |
| F16 | Null params: `NullWindow=1`, `NullDepthThreshold=1` (min plies from root), `NullDepthReduction=[0,0,0,0,1,2,3,3,4,5,6,...]`, `NullDepthExtendedReduction=[0,0,0,0,1,2,3,4,4,5,6,...]` (differs only at depth 7). Tables have 21 entries. | `Configuration.json`, `NullConfiguration` |
| F17 | Zugzwang guard: `IsWhite/BlackNonZugzwang` = rook or queen present, or >= 2 knights+bishops. | `Board.State.cs` |
| F18 | `NullWindowSearchX` assigns `best = -child` instead of `max`. Harmless today because only `< beta` is tested. | `StrategyBase` |

---------------------------------------------------------------------------------------------------

## 2. Problems, re-ranked after the full check

### P1 - Parity mismatch inside the null subtree (correctness, low frequency)
Because the ply does not advance on the flip (F1) while the side does, positions at ply `P+k` have the
opposite side to move compared with what F6 assumes. `IsRepetition` therefore compares same-placement
positions with opposite side to move. A hit needs the null-side to recreate, in one move, a piece
placement seen earlier at the "wrong" parity AND reversible count >= 4, so it is rare. Effect: a spurious
`0` score in the null subtree (fail-low when `beta > 0`; spurious fail-high when `beta <= 0`).
[V] mechanism, [H] frequency (instrument it, see Phase 0).

Correction to the earlier analysis: a side-to-move hash key is NOT required. If the null move occupies
a real ply (entry in `_history`), ply parity and side to move are consistent again (the null consumes
exactly the ply a move would). See Phase 1. This avoids changing `Board.Hash`, which would split the
evaluation cache by side (F8) and touch TT/book code.

### P2 - Fake stalemate at the null root (correctness, very rare)
`moves.Count < 1` and last move not a check -> returns 0 -> `nullValue = 0`, which is `>= beta` for
`beta <= 0` and signals a spurious null success. Should mean "null not applicable". Fix: return the maximum
score (so `nullValue < beta`) only at the null root node.

### P3 - Null is a depth reduction with an unusually expensive probe (strength/speed)
A successful probe saves only the reduction, and the probe has no TT and is not gated by static eval.
[H] A static-eval gate (`staticEval >= beta - margin`) removes a large share of failing probes.
[S] Magnitude unknown; measure null attempts vs successes in Phase 0.

### P4 - No PV exclusion; depth 1-3 behave like unverified pruning (strength risk)
`beta - alpha > 1` nodes still try null. For depth 1-3 the table value is 0, so a null qsearch >= beta
returns `EvaluateX(alpha, beta)` with no margin. Candidate for tuning, not for blind change.

### P5 - Robustness
- `NullDepthReduction[depth]` can go out of range if depth exceeds 20 (endgame `++depth`, extensions).
- `best = -child` (F18) would break any future use of the returned score.
- `NullWindow` is a constant 1; `NullDepthExtendedReduction` is nearly a duplicate.

### P6 - Not a problem (checked)
- En passant under the flip: the flipped side needs the enemy double push as last move (F7), but the last
  move is its own, so self-capture is impossible. [V]
- Current null does not pollute killers/history/counter moves (F11). [V]
- Context slot clobbering: null runs before the node acquires its ply context (F15). [V]

---------------------------------------------------------------------------------------------------

## 3. Target architecture

```
StrategyBase (main / end strategies)
   |  CanUseNull(beta) [+ optional eval gate]
   |  Position.MakeNull()           <- real ply, NullMove in history, turn flipped
   |  v = -_nullStrategy.SearchX(1-beta .. , depth-R)      (own TT, own sorters, no shared writes)
   |  Position.UnMakeNull()
   v
NullStrategy : StrategyBase  (CreateEndGameStrategy()/CreateNullStrategy() => null)
   - own TranspositionTable (small, bounds only, per-colour as today)
   - SearchWhite/Black override: CheckDraw, MDP, qsearch, CommonXSearch (no end-game hand-off)
   - SearchInternalWhite/Black override: lean loop + simple LMR, NO writes to History/Butterfly/killers/counters
   - extensions disabled (MaxExtensionPly/MaxOneReplyPly = -1)
   - NullSorter (Phase 5, only if profiling justifies)
```

Principles
1. Every behavioural change is behind a config flag; default = today's behaviour until A/B proves a gain.
2. No allocation, no LINQ, no new virtual dispatch on hot paths beyond what `SearchWhite/Black` already has.
3. Null search is read-only with respect to shared heuristics (killers, history, counter moves).
4. `Board.Hash`, TT layout, opening book and evaluation cache stay untouched.

---------------------------------------------------------------------------------------------------

## 4. Implementation phases

Each phase ends with a gate. Do not start the next phase until the gate passes.

### Phase 0 - Baseline and instrumentation (no behaviour change)

Goal: numbers to compare against and proof of how often P1/P2 occur.

Steps
- [ ] 0.1 Pick a fixed suite: ~50-100 FENs (opening, middlegame, endgames incl. K+minor+pawns, rook endings,
	  shuffling positions) + the existing `BenchmarkTool` / `StockfishBenchmark` runners. Record at depth 8, 10:
	  nodes, time, nps, best move, score, TT size.
- [ ] 0.2 Temporary counters (compile-time `#if NULL_STATS`, not shipped): null attempts, null fail-highs,
	  nodes inside null searches, time in null search, count of `CheckDraw()==true` inside null subtree
	  (P1 frequency), count of null-root `moves.Count<1` (P2 frequency).
- [ ] 0.3 Save results in `Docs/NullBaseline.csv` (or chat) for later diff.

Gate: baseline reproducible run-to-run (single-threaded, fixed depth gives identical node counts).

#### Phase 0 - implementation status (done in code)

Implemented in `KioChess/Engine/Strategies/Base/StrategyBase.cs`:
- `public static class NullStats` (same file). All methods are `[Conditional("NULL_STATS")]`, so without the
  symbol the calls (and their argument evaluation) are removed by the compiler: zero cost, behaviour unchanged.
  Counters: `Attempts`, `FailHighs`, `Nodes` (null-subtree nodes), `Ticks` (time in null probes),
  `DrawHits` (P1: `CheckDraw()==true` inside a null subtree), `RootNoMoves` (P2: null root without legal moves).
  `Reset()` and `Report()` (`Report()` is not conditional; it returns zeros when the symbol is off).
- Hooks: `CalculateWhiteDepth/CalculateBlackDepth` (Begin/End), `NullWindowSearchWhite/Black` (Node, Draw, NoMoves).

How to use
1. Enable: Engine project Properties -> Build -> Conditional compilation symbols: `NULL_STATS`
   (or `dotnet build -p:DefineConstants=NULL_STATS`). Use Release.
2. Before each position/suite run call `NullStats.Reset()`; after it log `NullStats.Report()` together with
   nodes, time, nps, best move, score at depth 8 and 10 (items 0.1 and 0.3).
3. Run the suite twice; node counts and null counters must be identical (gate).
4. Interpretation: `DrawHits > 0` or `RootNoMoves > 0` confirms P1/P2 occur and quantifies them;
   `FailHighs / Attempts` and `Ticks` vs total time quantify P3.
5. Keep the numbers (chat or a CSV) as the baseline for Phase 1. Build the shipping binary without `NULL_STATS`.

Not done here (needs the engine run, not code): the FEN suite itself and the recorded baseline values.

### Phase 1 - Real null ply (correctness, behaviour-preserving apart from P1/P2)

Rationale: ply parity == side parity again, repetition and heuristics read sane history, and en
passant stays illegal after the null because the last history key is not a pawn push.

1.1 New `NullMove : MoveBase` (`Engine/Models/Moves/NullMove.cs`), two singletons (white/black) so `IsWhite`
	stays meaningful:
```csharp
public sealed class NullMove : MoveBase
{
	public static short SentinelKey;                 // = MovesCount, set once at startup
	public override bool IsLegal() => false;
	public override void Make() { }
	public override void UnMake() { }
}
// fields: Key = SentinelKey; Piece = a pawn constant of the side; IsAttack = false; IsCheck = false;
// CanNotReduceNext = false; IsPromotionExtension = false; IsIrreversible = true; To = From = 0.
```
1.2 Counter-move arrays get one extra slot so the sentinel key is a valid index (F10):
```csharp
// MoveHistoryService.SetCounterMoves(int size): allocate size + 1, fill -1.
```
	Dictionary-based CMH (`_countermoveHistory`, `_continiousMoveHistory`) accept any key; nothing to do.
1.3 `MoveHistoryService.AddNull` (does NOT touch `_sequence`, F12; copies phase, F13):
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
internal void AddNull(MoveBase nullMove)
{
	int ply = _ply;
	_history[++_ply] = nullMove;
	_nullMoves[_ply] = false;                    // no null inside a null subtree
	_checks[_ply] = false;
	_phases[_ply] = _phases[ply];
	_castleHistory[_ply] = _castleHistory[ply];
	_reversibleMovesHistory[_ply] = _zero;       // null is irreversible: repetition cannot look past it
	_boardHistory[_ply] = _board.Hash;
}
```
	`Remove()` already works (`_history[_ply--].UnMake()` is a no-op for `NullMove`).
	Known trade-off: the 50-move clock restarts inside the null subtree (never creates false draws; can only
	miss a real 50-move draw that is >= 100 plies away). Acceptable for a reduced-depth probe.
1.4 `Position.MakeNullWhite/MakeNullBlack/UnMakeNullWhite/UnMakeNullBlack`
	(`Engine/Models/Boards/Position.cs`), mirroring `MakeWhite/UnMakeWhite`:
```csharp
public void MakeNullWhite()  { _moveHistoryService.AddNull(NullMove.White); _turn = Turn.Black; }
public void UnMakeNullWhite(){ _moveHistoryService.Remove(); _turn = Turn.White; }
```
1.5 Switch `CalculateWhiteDepth/CalculateBlackDepth` to the new primitives; `NullWindowSearchX` unchanged
	(still private, still no TT) so node counts stay comparable.
```csharp
private sbyte CalculateWhiteDepth(int beta, sbyte depth)
{
	Position.MakeNullWhite();                    // black to move, ply+1
	int nullValue = -NullWindowSearchBlack(NullWindow - beta, NullDepthReduction[depth]);
	Position.UnMakeNullWhite();
	return GetNullDepth(beta, depth, nullValue);
}
```
1.6 P2 fix: in `NullWindowSearchX`, when `moves.Count < 1` and `GetPly() == _nullRootPly` return `SearchValue`
	(so `nullValue < beta`). `_nullRootPly` is a field set in `CalculateXDepth`.
1.7 Audit every reader of `_history[_ply]` / `GetLastMove()` / `IsRecapture()` / `IsLast()` /
	`GetHeuristicMoves()` / sorters that use the last move (`To`, `IsAttack`, `Piece`) for assumptions that
	break with a null entry. Fix by making `NullMove` fields neutral, or guard in the reader.
1.8 Clamp the null tables (P5): `NullDepthReduction[Math.Min(depth, len-1)]` or validate length at load.
1.9 Fix `best = Math.Max(best, -child)` in both `NullWindowSearchX` (P5).

Gate
- perft/make-unmake round trip unchanged (null never reaches perft).
- Assertion test: after any search, `GetPly()`, `Hash`, `_turn`, `_nullMoves`, castle rights equal pre-search values.
- Same suite as Phase 0: best moves equal or explainable; nps within +-1%; node count differences only where
  P1/P2 counters from Phase 0 were > 0. No allocations (check with the existing benchmark's GC counters).

Rollback: single commit; revert `CalculateXDepth` to the flip (primitives can stay unused).

### Phase 2 - Cheap, flag-gated pruning-quality experiments (still the old null searcher)

Add to `NullConfiguration` (all default OFF / current behaviour):
```
bool UseEvalGate;  int EvalGateMargin;     // skip null when Position.GetValue() < beta - margin
bool NullNotInPv;                          // skip when beta - alpha > 1
int  NullMinDepth;                         // today implicit via table zeros (depth 1-3 behaviour)
```
Steps
- [ ] 2.1 Implement `CanUseNull(alpha, beta, depth)` overload honouring the flags (inlined, no extra loads when OFF).
- [ ] 2.2 A/B on the Phase 0 suite: time-to-depth, nodes, bestmove agreement with the deeper reference search.
- [ ] 2.3 Match test (self-play, fixed time or depth, a few hundred games; treat Elo as approximate).

Gate: a flag becomes default only if time-to-depth improves without best-move/score degradation on tactical
positions and the match is non-negative. Otherwise keep OFF.

Interaction notes: eval gate uses `Position.GetValue()` (already used by futility in `GetSearchResultType`);
it interacts with razoring/futility margins (`AlphaMargins/BetaMargins`) and with TT cutoffs
(`CutoffDepth`). Keep the TT cutoff before the null probe, as today.

### Phase 3 - `NullStrategy` with its own TT (speed, isolation)

Why: null-subtree positions are mostly unique to the null search (opponent moves twice), have reduced depth
and different path semantics; they would pollute the main TT. A separate table lets the null probe
benefit from transpositions between sibling probes and between iterations.

3.1 Add to `StrategyBase`:
```csharp
protected NullStrategy _nullStrategy;
protected virtual StrategyBase CreateNullStrategy() => new NullStrategy(Depth, Position);   // lazily, see 3.4
```
	`NullStrategy` overrides both `CreateEndGameStrategy()` and `CreateNullStrategy()` to return `null`
	(same recursion-stopper pattern as `IdLmrDeepEndStrategy`).
3.2 `NullStrategy : StrategyBase` (`Engine/Strategies/Null/NullStrategy.cs`)
	- ctor: `base(depth, position, new TranspositionTable(capacity, board, config))`. Size from
	  `TranspositionTableService.Create(depth)` capacity divided by 4-8 (confirm `TranspositionHashSet`
	  capacity semantics before choosing; keep it cache friendly).
	- `InitializeSorters(depth, position, MoveSorterProvider.GetSimple(position))` initially (reuse existing sorters).
	- `SetExtensionThresholds`: force `MaxExtensionPly = MaxOneReplyPly = -1` (no extensions inside null).
	- Override `SearchWhite/SearchBlack`:
```csharp
public override int SearchWhite(int alpha, int beta, sbyte depth)
{
	if (CheckDraw()) return 0;
	if (TryMateDistancePruning(ref alpha, ref beta, out int v)) return v;
	if (depth < 1) return EvaluateWhite(alpha, beta);
	return CommonWhiteSearch(alpha, beta, depth);          // own Table; CanUseNull is false (flag set by AddNull)
}
```
	  (no `IsEndPhase()` hand-off, so no use of the main end-game strategy or its TT)
	- Override `SearchInternalWhite/Black` with the lean loop: same as base but
	  WITHOUT `move.Butterfly++`, `move.History += ...`, `context.Add(...)` (F9). Optional simple LMR: from move
	  index >= K, non-check, `move.CanReduce`, depth >= 3, reduce 1 ply and re-search on fail-high (mirrors
	  `LmrStrategyBase`, but read-only).
	- Futility/razoring in `SetSearchValueX` reuse base logic; `FutilitySearchInternalX` also writes
	  `context.Add`/`History` (F9): either override those too, or disable futility paths in null via a
	  protected flag. Decide after reading their remaining call sites.
3.3 Entry points in the owner strategy replace the private searcher:
```csharp
Position.MakeNullWhite();
int nullValue = -_nullStrategy.SearchBlack(NullWindow - beta, 1 - ..., NullDepthReduction[depth]);
Position.UnMakeNullWhite();
```
	Window stays zero-width: `(-beta, -beta+1)` from the null side's view.
3.4 Lifecycle (must mirror the main table): forward `Clear()`, `ExecuteAsyncAction()` (`Table.Update()` generation),
	`Resize` and `SetExtensionThresholds(ply)` from the owner to `_nullStrategy`. Create lazily on first use.
	`IdLmrDeepEndStrategy` delegates all searching to `_strategy`, so only the inner end strategy ever creates one.
3.5 TT stores are bounds only; use `ToTTValue/FromTTValue` as the main search does; never store the result in
	the owner's table.
3.6 Keep `UseNullStrategy` flag (default OFF). With the flag OFF the Phase 1 private searcher is used.

Gate: with flag ON vs Phase 1: nodes inside null probes down, nps >= baseline - 1%, identical or better
best-move agreement, memory delta documented, no allocation in the null path, state assertions from Phase 1 pass.

Rollback: flag OFF.

### Phase 4 - Pruning-with-verification mode (strength, optional)

Config `NullMode`: `Reduce` (today), `Prune`.
- `Prune`: after a null fail-high at depth < `NullVerifyDepth` return `beta` (clamp mate scores); at depth >=
  `NullVerifyDepth` run a verification search at `depth - R` with null disabled, cut only if it also fails high.
- R formula option: `R = base + depth / divisor (+ min((eval - beta) / evalDivisor, max))`, configured instead of tables.
- Interactions: TT cutoff order, futility/razoring margins, one-reply and check extensions, zugzwang guard,
  mate-distance pruning. Never store the null score as exact in any TT.
Gate: match + tactical/zugzwang suite (Fine #70 `8/k7/3p4/p2P1p2/P2P1P2/8/8/K7 w - - 0 1`, best `Kb1`; K+minor vs K+pawn;
rook endgames with shuffling). Only adopt if strictly non-negative.

### Phase 5 - `NullSorter` (only if profiling shows sorting dominates null cost)

Facts that shape it: the null side's first ply is expected to be an all-node (ordering irrelevant), the next ply
is a cut-node (ordering matters). Sorting cost is therefore best spent from ply 2 of the probe.
Design (if justified)
- Dedicated `MoveSorterBase` subclass + contexts registered like the existing eval contexts in `DataPoolService`.
- Order: TT move -> winning captures (SEE >= 0, MVV-LVA) -> killers (READ ONLY) -> history quiets (read `RelativeHistory`) -> losing captures.
- Skip `GetHeuristicMoves()` dictionary lookups (`_countermoveHistory`, `_continiousMoveHistory`) at the null root.
- Lazy selection (pick best remaining) instead of full sort, if the move list API allows; otherwise partial sort.
Gate: profiler share of sorting inside null probes (before/after), nodes unchanged or lower, nps higher.
Do not start without that profile (repo rule: no unmeasured micro-optimisation).

### Phase 6 - Tuning and cleanup
- Collapse `NullDepthReduction` / `NullDepthExtendedReduction` / `NullWindow` per the chosen mode; rename
  `NullDepthThreshold` -> `NullMinPlyFromRoot`; remove dead code (private searcher, `GetCurrentNullSortContext`,
  `GetMovesForNullSearch`) only after the flag-OFF path is retired.
- Update `DrawConfiguration` comments if the "contempt not applied in null" statement changes.

---------------------------------------------------------------------------------------------------

## 5. Validation matrix

| Check | Tool | Phase |
|-------|------|-------|
| State restored after search (ply, hash, turn, `_nullMoves`, castle, `_checks`) | assertion test | 1, 3 |
| Null never leaves `NullMove` in `_history` after exceptions/early returns | code review + assertion | 1, 3 |
| En passant illegal right after a null; legal in the main line | unit test with double-push position | 1 |
| Repetition inside null subtree does not look past the null | unit test on `IsRepetition` with synthetic history | 1 |
| No allocations in null path | benchmark GC counters | 1, 3, 5 |
| nps / time-to-depth / nodes | `BenchmarkTool`, `StockfishBenchmark` | every phase |
| Zugzwang and endgame regressions | suite above | 2, 4 |
| Self-play match (approximate Elo) | existing StockFish compare tools | 2, 4, 6 |

---------------------------------------------------------------------------------------------------

## 6. Risk register (reassessed)

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| `NullMove` entry read by code that assumes a real move (sorters, `IsRecapture`, counter moves) | Medium | Medium (wrong ordering) to High (IndexOutOfRange) | Sentinel key slot (1.2), neutral fields, audit 1.7, assertion tests |
| `_sequence` / book corrupted by null key | Low (avoided by design) | High | `AddNull` never writes `_sequence` |
| State not restored on an exit path | Low | High | Single entry/exit pair around the probe; assertions in tests |
| 50-move clock restarts in null subtree | Certain | Negligible | Documented trade-off; never creates false draws |
| Extra ply consumes `DataPoolService` capacity | Low | Low | Resize margin is 64 plies; +1 ply is irrelevant |
| Phase 1 changes node counts vs baseline (P1/P2 fixes) | Low | Low | Expected only where Phase 0 counters were > 0 |
| `NullStrategy` base ctor side effects (`DataPoolService.Initialize`, margins, sorters) | Medium | Low | Idempotent today (also used by end strategies); verify `RefreshRows` behaviour |
| `NullStrategy` inherits base writes to `History/Butterfly/killers/counters` | High if not overridden | Medium (silent ordering damage) | Override `SearchInternalX` and futility loops; test: heuristics tables identical before/after a null probe |
| Own TT sizing / generation / clear forgotten | Medium | Medium (stale entries, memory) | Forward lifecycle calls (3.4), test new-game reset |
| Memory: one extra TT per strategy that searches | Low | Low | Small table; only inner end strategy duplicates |
| Eval gate / PV exclusion / prune mode hurt tactics or zugzwang | Medium | Medium | Flag default OFF; A/B + matches; keep guard and verification |
| Hash change (rejected) | n/a | n/a | Not needed after Phase 1; avoids eval-cache split (F8) and book/TT churn |
| Full replacement by main search (rejected) | n/a | n/a | User requirement; own strategy gives LMR/TT without shared state |

Dropped from the earlier proposal because the code check showed it is unnecessary or harmful here:
- side-to-move key in `Board.Hash` (real null ply fixes parity; hash change would split the eval cache);
- sharing the main search body with the main TT/heuristics.

---------------------------------------------------------------------------------------------------

## 7. Suggested implementation order (checklist)

- [ ] Phase 0 baseline + counters
- [ ] Phase 1 real null ply (1.1-1.9) + assertion/unit tests
- [ ] Phase 2 flags (eval gate, PV exclusion) + A/B
- [ ] Phase 3 `NullStrategy` + own TT + flag + A/B
- [ ] Phase 4 prune/verify mode (optional) + match
- [ ] Phase 5 `NullSorter` (only after profiling)
- [ ] Phase 6 config cleanup

Elo/speed statements in this document are hypotheses until measured with the phase gates above.
