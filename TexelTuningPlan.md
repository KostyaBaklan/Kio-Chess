# Texel Tuning Implementation Plan (KioChess)

Branch: `137-Texel/Dev`. Data source: one extracted Lichess `.pgn` file in `C:\Dev\PGN` (no zstd needed).

## Revision 2 (overrides earlier sections where they conflict)

1. **Texel settings live in their own file**, `TexelTool\Config\TexelConfiguration.json`, copied to the output like `Configuration.json`.
   The `TexelConfiguration` block in the Addendum (section D) moves out of `Engine\Config\Configuration.json` into that file.
   Engine-side additions (`Evaluation.Static.Tempo`, `Evaluation.Static.Taper`) stay in `Engine\Config\Configuration.json`.
2. **Config classes** (`TexelConfiguration`, `TexelDataConfiguration`, ...) live in `TexelTool` (namespace `TexelTool.Config`), not in `Engine`.
   No change to `IConfigurationProvider`, `ConfigurationProvider` or `Boot.cs` is needed, which removes the cross-project risk listed in D.
   `TexelTool` loads the file itself:
   ```csharp
   var json = File.ReadAllText(@"Config\TexelConfiguration.json");
   var texel = JsonConvert.DeserializeObject<TexelConfiguration>(json);
   ```
3. **Project file.** `TexelTool.csproj` currently has no references (verified: only the `OutputType`/`net10.0`/`BaseOutputPath=..\Data` properties).
   It needs the same references as `PgnTool` plus the config copy. `Engine.csproj` already copies `Configuration.json` and `StaticTables.json`
   as `Content`, and `Boot.SetUp()` reads `Config\...` from the output directory, so check that the Engine content reaches the TexelTool output
   (if not, add `Content Include` links as `Engine.csproj` does). Snippet:
   ```xml
   <ItemGroup>
     <ProjectReference Include="..\Engine\Engine.csproj" />
     <ProjectReference Include="..\Engine.Pgn\Engine.Pgn.csproj" />
     <ProjectReference Include="..\Tools.Common\Tools.Common.csproj" />
   </ItemGroup>
   <ItemGroup>
     <Content Include="Config\TexelConfiguration.json">
       <CopyToOutputDirectory>Always</CopyToOutputDirectory>
     </Content>
   </ItemGroup>
   ```
   `Boot.SetUp()` also builds the DB services (`AppDbService`, `OpeningService` from `BookConfiguration.DatabasePath`). Check that it does not fail
   when `C:\Dev\ChessDB\kioapp.db` is missing or locked on the tuning machine.
   `Newtonsoft.Json` is used by `Tools.Common`; add it to `TexelTool` explicitly if it does not flow through.
4. **PGN input** is a plain extracted file, so `PgnStreamReader(string filePath)` is used directly (it already streams with a 64 KB buffer). Drop the zstd package and
   the zstd notes in 4.1 and B. `ProgressPercent` works for a file stream, so use it for progress output.
   Config key: `Data.PgnPath` = `C:\Dev\PGN\<file>.pgn`.
5. **Start order changed** (see below): extraction and a baseline error come first, so that every engine change in
   phases 0-3 can be judged by validation error as well as by matches.

### Where to start

Step 1 - TexelTool scaffolding (no Engine changes):
- Fix `TexelTool.csproj` as above; add the project to `KioChess.sln` if it is not there.
- Add `Config\TexelConfiguration.json` with only the `Data` section and the config classes.
- `Program.cs`: `Boot.SetUp()`, load config, command `extract`.

Step 2 - Extraction (`extract`):
- `PgnStreamReader.ReadGames()` -> filters from config -> replay moves the way `PgnConverter` does -> quiet-position sampling -> binary store.
- Output a small report: games read, games kept, positions written, elapsed time. This gives the real extraction time for your PC.
- Verify the open points from B (move format, comment handling) on the first 1,000 games.

Step 3 - Minimal Phase 0.4 (the only Engine change at this point):
- `Board.EvaluateNoCache(phase)` plus a cache reset. It is small, does not change playing behavior, and lets TexelTool score positions.
- `bench` command: evaluate all stored positions, print microseconds per position. This replaces the guess in Phase 6 with a measured number.

Step 4 - Baseline:
- `kfit` command: fit K and print the training and validation error of the current, untuned evaluation.
- Save these numbers. They are the baseline.

Step 5 - Then phases 0.2, 0.3, 1, 2, 3 in order. After each one, rerun `kfit` (K may change) and compare the validation error, then run the match.
A lower validation error after the tapered evaluation supports that change before you spend time on tuning. It is evidence, not proof of Elo.

Step 6 - Tuner (Phase 5), starting with the cheap groups.

Why this order: extraction and scoring need only one small Engine change, they are independent of the evaluation refactor, and they turn
"is this change better?" into a number you can compute in seconds.

---------------------------------------------------------------------

## Revision 3 - Engine analysis (evaluation, search, move ordering) and strength roadmap

Scope of the review: static reading of `StrategyBase.cs` (TT probe, null move, qsearch lines 440-745 read directly), `Board.Evaluation*.cs`,
`ComplexSorter*`, `MoveHistoryService` and `Configuration.json`. Items marked **[confirmed]** were read by me in the source,
**[reported]** come from a code-survey and still need a quick look, **[hypothesis]** is my inference. Nothing was run or measured,
so every Elo figure below is a guess based on typical results for classical engines, not evidence about this engine.

### R3.1 Corrections to earlier sections of this plan
- **Phase 0.2 "Middle rook bug" is withdrawn [confirmed].** `Board.Evaluation.Rook.cs` only has `EvaluateWhiteRookOpening` and `EvaluateWhiteRookEnd`
  (and the black pair). There is no Middle rook function, so Middle deliberately reuses the Opening code, and the Middle config values still apply
  through the active `EvaluationServiceBase`. Do not "fix" it. In Phase 3 the rook function is merged into one mg/eg pair anyway.
- **Phase 0.3 rule-50 cache concern is mostly moot [confirmed].** `ScaleByRule50` is defined in `Board.Evaluation.Draw.cs` but a search of `Engine`
  found no caller (only comments in `Board.Initialization.cs` that mention a `Board.Evaluation.MopUp.cs`). So the cache currently does not hold
  a rule-50-scaled value. Keep the check as a one-line verification: if you ever wire `ScaleByRule50` in, apply it outside the cached value.
- **"TT cutoffs never fire" (survey claim) is wrong [confirmed].** `CommonWhiteSearch` cuts off only when
  `entry.Depth >= depth && entry.Depth < CutoffDepth`, and `CutoffDepth` comes from `CutoffDepth[]` in config (for root depth 10 it is 7, so entries of
  depth 1..6 cut). It is a deliberate restriction: TT cutoffs are disabled for deep entries. It is still a strength question (see S3).
- **`IsRepetition(Ply)` using the root ply** looks deliberate (repetition inside the search tree scores as a draw). Leave it.

### R3.2 What the engine looks like today
- **Search [confirmed]:** plain alpha-beta with full windows. The reviewed search loops show no PVS/null-window re-search.
  LMR reduces to `depth-2`, and re-searches at full depth when the reduced value beats alpha.
  TT with per-colour tables. Reverse futility (depth <= 5), futility (depth <= 2), razoring (depth 3 only), check and one-reply extensions,
  mate-distance pruning, delta pruning in qsearch (SEE based), separate null-search strategy with its own TT.
- **Null move [confirmed]:** the null result does **not** produce a cutoff. `GetNullDepth` returns the normal depth if null fails low, otherwise
  `NullDepthExtendedReduction[depth]`, i.e. a null fail-high only reduces the node's depth. The usual benefit of null move
  (returning `beta` immediately) is not used. This is a large strength/speed question (S2).
- **Default strategy [reported]:** `"Strategy": "lmrd"` is a single fixed-depth search; iterative deepening and aspiration only exist as separate strategies.
  The first move of a game at depth d starts cold (no TT move from shallower iterations).
- **Move ordering [reported]:** `ComplexSorter` with phase-specific build methods (`BuildOpening/Middle/End`, book variants), killers, relative history,
  continuation/counter-move dictionaries in `MoveHistoryService` (`Dictionary<int,short>`, `Dictionary<long,short>`), SEE flags (`LowSee`),
  and sorting limited by `SortDepth[]`. `Position.GetAllMoves()` allocates `List`s and uses LINQ, but looks like a GUI/test helper;
  the search uses `SortContext`/`MoveHistoryList`.
- **Evaluation [confirmed/reported]:** three discrete phases, PST from `StaticTables.json` per phase, king-shield lookup tables built at start-up
  (`short[64*512]`), pawn hash and evaluation cache, pins/x-rays/discovered terms in `Board.PinsAndXRays.cs`, mobility in `Board.Mobility.cs`, mop-up.

### R3.3 Evaluation findings and actions
| # | Finding | Status | Action |
|---|---------|--------|--------|
| E1 | Discrete Opening/Middle/End; Opening hard-coded to plies 0-15; `SetPhase` never returns to Opening | confirmed | Phase 3 (taper). Tapering the PST and all terms is the largest structural fix |
| E2 | Piece values identical in all phases (P100 N325 B325 R500 Q990) | confirmed | Tune; expect bishop > knight, eg pawn > mg pawn, rook/queen shifts |
| E3 | PST asymmetric (e.g. pawn C3=5/F3=0) and quantised to 5 | confirmed | Tune with A-H symmetry, see the PST addendum |
| E4 | Many tactical terms (pins, discovered, x-ray, batteries) in a static eval | reported | Tune late with bounds; allow them to go to ~0 |
| E5 | No tempo term | reported | Phase 1 |
| E6 | Terms I could not find [hypothesis, verify by search]: knight outposts, rook on 7th, trapped/hanging pieces (threats), space, bad bishop | unverified | Add only after Texel, one at a time, each judged by validation error |
| E7 | `GetStaticValue()` (material only, used by sorting and futility) is separate from `Evaluate()` | confirmed | Keep consistent: once material becomes tapered, update both or define static = material mg/eg blend |
| E8 | Evaluation depends on `_moveHistory` (phase, castle rights) | confirmed | Make evaluation a function of the position (Phase 3 removes the phase dependency) |
| E9 | Numeric literals found by grep are only in the king shield lookup build (`512`, `64`) and the rule-50 clamp (`99`, `1000`) | confirmed | These are structural. Move rule-50 constants to config only if `ScaleByRule50` is ever used |

### R3.4 Search and pruning findings (ranked by expected value at depth ~10)
Order matters: the Texel work is independent of these, but any search change also changes the scale the margins were set for, so do
one change at a time and match-test each.

**S1 - PVS (null-window) for non-first moves.** [confirmed absent in the reviewed loops]
Every child gets a full window. With good ordering PVS searches most siblings with `(alpha, alpha+1)` and re-searches only on a fail-high.
Combine with the existing LMR: reduced null-window search, then full-depth null-window search, then full window only if `alpha < v < beta`.
- Value: typically one of the biggest basic search gains for an engine without it; plausibly tens of Elo (guess); speed gain grows with depth.
- Risks: bound handling in TT entries (a null-window result is a bound, not exact; `ComputeTranspositionEntryType` already compares against
  original alpha/beta, which is the right basis); PV tracking and root best-move selection must still use the full window at the root.
- Interactions: LMR re-search condition (`value > alpha`), futility/RFP, `NullWindow = 1` config.
- Sketch:
  ```csharp
  // inside the move loop, i = move index, first move uses the full window
  int v;
  if (i == 0)
      v = -Search(-beta, -alpha, depth - 1);
  else
  {
      v = -Search(-alpha - 1, -alpha, depth - 1 - reduction);        // reduced, null window
      if (v > alpha && reduction > 0) v = -Search(-alpha - 1, -alpha, depth - 1);
      if (v > alpha && v < beta)      v = -Search(-beta, -alpha, depth - 1);
  }
  ```
  Put the window width (`1`) and any thresholds in `Configuration.json` (`NullWindow` already exists).

**S2 - Real null-move cutoff.** [confirmed behaviour]
Today a null fail-high only reduces depth. Test returning `beta` (fail-soft: `nullValue`, mate scores clamped) when `depth >= NullDepthThreshold`,
the side has non-pawn material (`IsWhiteNonZugzwang` already exists), not in check, previous move not null (`CanUseNull`), and `eval >= beta`.
At `depth >= ~8` add a verification search at reduced depth to protect against zugzwang.
- Value: large node reduction at depth 8-12; plausibly one of the largest speed gains; strength depends on the zugzwang guard (guess).
- Risks: zugzwang endgames (the engine already has `IsLikelyZugzwangPosition`; keep it as a hard guard), TT pollution (null search has its own TT, keep that),
  mate-score returns (never return an unproven mate from null).
- Keep the current behaviour behind a config switch (`NullConfiguration.Mode`: `Reduce` | `Cutoff` | `CutoffVerify`) so you can A/B test in one build.
- Interactions: reverse futility already covers much of the high-eval case at depth <= 5; null cutoff matters most at depth 6+.

**S3 - TT cutoff restriction (`entry.Depth < CutoffDepth`).** [confirmed]
This disables cutoffs from deep entries. It was probably added to stabilise results or avoid PV truncation, but it throws away the largest TT saving.
Experiment: make the restriction a config option (`CutoffDepth[]` already exists; add a "disabled" value such as 255) and test
non-PV nodes with full cutoffs while keeping the restriction at PV nodes. Also check the TT store: the non-PV path always overwrites
without a depth/age check, while the PV path checks `depth >= entry.Depth` [reported]. Verify and unify the replacement rule
(depth-preferred with ageing) so shallow entries do not evict deep ones.
- Value: potentially large speed gain at depth 10; strength risk is search instability from a deep entry with a stale score.
- Interactions: repetition/draw detection (TT scores can hide repetition), aspiration/PV.

**S4 - Quiescence search.** [reported]
Capture-only qsearch with delta pruning; evasions when in check. Missing: pruning captures with SEE < 0 (it uses SEE only for the delta test),
promotions, and a TT probe/store in qsearch. Add SEE < 0 pruning for non-check, non-promotion captures; keep delta pruning (`DeltaPruningMargin` 375).
- Value: speed (smaller qsearch) with little strength risk; moderate (guess).

**S5 - History updates.** [reported]
Bonus on cutoff is `depth*depth` in the main search and `History++` in the in-check qsearch (inconsistent); there is no malus for the quiet moves
that failed before the cutoff. Add a malus for previously searched quiets, bound/age the table, and use one bonus formula everywhere.
All constants into `HistoryHeuristic` config (`RelativeHistoryFactor` already exists).
- Value: modest ordering gain; low risk. Because relative history is already used, treat this as a tuning/improvement, not a new feature.

**S6 - Late-move pruning and history-based pruning at low depth.** [reported absent]
At depth <= 3-4 skip late quiet moves (move-count thresholds in config) and quiet moves with strongly negative history, when not in check
and not a killer. Interacts with LMR (`Lmrd [2,4,9]`) and futility; start with depth <= 2 and widen if it tests well.

**S7 - Internal iterative reduction.** [reported absent]
When a PV/non-PV node at `depth >= 4` has no TT move, reduce depth by 1. Cheap and usually safe.

**S8 - Aspiration and iterative deepening.** [reported]
The default `lmrd` strategy has no iterative deepening, and the aspiration strategy re-searches once with a full window after a fail. If you
play with time limits (not reviewed), iterative deepening gives a TT move and better ordering for the final iteration, plus time management. Check what
the factory builds for real games before spending effort here; if matches use fixed depth only, skip this.

**S9 - Futility / razoring / reverse futility margins.** [confirmed values]
Margins `[200,450]`, `[150,275,425,600,775]`, razoring only at depth 3 with 700, delta 375. These are plausible, but they are applied at PV nodes
too [reported]. After Texel and after S1-S3, re-tune them with a match test (not Texel). Guard: never prune when `|beta|` is near mate (already done).

**S10 - Correctness checks to do before any strength work.** [reported, verify first]
- Root best-move tracking: `GetFirstMove`/root loops start from `Result.Value` default 0; verify the root still picks a move when every move scores <= 0.
  Test with a lost position, e.g. `7k/8/8/8/8/8/5q2/K7 w - - 0 1` should still return a legal move.
- LMR condition off-by-one between root (`i > lmr`) and inner nodes (`i >= LmrIndex`) in `LmrStrategyBase`: confirm it is intentional.
- Null search strategy has no mate-distance/draw handling beyond `CheckDraw`: confirm it cannot return a mate score to the parent.

### R3.5 Move ordering and heuristics findings
| # | Finding | Status | Action |
|---|---------|--------|--------|
| M1 | Ordering is phase-specific (`BuildOpening/Middle/End`) and uses `SortDepth[]` to limit how many moves are sorted | reported | Keep; once the eval is tapered consider merging opening/middle build paths |
| M2 | Killers: `KillerMoves` (2 slots) [reported size] | reported | Check killers are keyed by ply, not depth; cleared between searches |
| M3 | History/counter/continuation use `Dictionary<int,short>` / `Dictionary<long,short>` with capacity 32767 / 65535 | confirmed (fields) | Dictionary lookups in the hot path are slow and can grow; replace with fixed arrays indexed by `[piece][to]` and `[prevPiece][prevTo][piece][to]`. Keep behaviour identical; verify with a node-count bench |
| M4 | SEE exists (`AttackBase.See`, `LowSee` flags) | reported | Make sure bad captures (SEE < 0) are searched after quiets, and measure |
| M5 | `Position.GetAllMoves()` uses `List<MoveBase>` + LINQ + `Where(...)` | confirmed | Not on the search hot path if only GUI/tests use it; grep callers. If a search path calls it, it must be replaced |
| M6 | No history malus, gravity, or ageing (see S5) | reported | S5 |
| M7 | Allocation check | hypothesis | Run the profiler on a depth-10 search and look at GC count; the plan's rule: no allocations in search/move generation |

### R3.6 Testing method (needed for all of the above)
- **Fixed positions:** keep a 20-50 FEN bench set (opening, middlegame, endgame, zugzwang). Record nodes, time, best move per build. A search change that
  only prunes should reduce nodes at equal depth and keep the best moves mostly the same.
- **Match testing:** `cutechess-cli` or your existing `StockFishAppManager` (it exists in the solution) against `baseline.exe`, same time control, a book-neutral
  opening set. SPRT `elo0=0 elo1=5` stops early. 200 games cannot see +10 Elo; plan on 1,000+ at fast time control for each accepted change.
- **One change per commit**, config switch where possible, so a regression can be bisected.

### R3.7 Recommended order (replaces my earlier "Phase" priority; Texel groups keep their own order)
1. S10 correctness checks (cheap, avoids wasted tuning on a bug).
2. Texel infrastructure: Revision 2 steps 1-4 (extract, `EvaluateNoCache`, bench, baseline `kfit`).
3. Phases 1-3 (tempo, widen types, taper), judged by validation error then by a match.
4. First Texel run (material, mobility, tempo, then PST with symmetry), then a match.
5. S1 PVS, then S2 null cutoff (config switch), then S3 TT cutoff test. Match each.
6. S4 qsearch SEE pruning, S5 history malus, M3 array-based history, S7 IIR.
7. S6 late-move/history pruning, S9 margin re-tune, then a second Texel round with the final search (the eval scale interacts with the margins).

Expected overall effect (guess, not measured): the largest likely gains are S1 + S2 (search efficiency) and Texel + tapering (eval); the remaining items are
smaller. Do not expect them to add up exactly, and re-measure after each step.

### R3.8 Where the new settings go
- Search switches and thresholds (`NullConfiguration.Mode`, null verification depth, PVS enable flag, qsearch SEE margin, LMP move-count table,
  history malus factor, IIR depth) go into `Engine\Config\Configuration.json` under `AlgorithmConfiguration`, with matching config classes in
  `Engine/Models/Config` and getters on `IConfigurationProvider`.
  Example:
  ```json
  "NullConfiguration": { "Mode": "CutoffVerify", "VerifyDepth": 8, "NullWindow": 1, "...": "existing" },
  "SearchConfiguration": { "Pvs": true, "IirDepth": 4, "QSeePruning": true, "LmpMoveCounts": [ 0, 6, 10, 16 ] },
  "HistoryHeuristic": { "RelativeHistoryFactor": 0.00001, "MalusFactor": 1.0 }
  ```
- Texel settings stay in `TexelTool\Config\TexelConfiguration.json` (Revision 2).



Evidence vs speculation: file names and line references come from a code survey, not a full read. Verify each before editing.
All time and Elo numbers are estimates. Measure on your PC (Phase 6).

---------------------------------------------------------------------

## Overview

| Phase | Goal | Type |
|-------|------|------|
| 0 | Baseline: fix probable bugs, make eval pure and testable | Pre-implementation |
| 1 | Tempo term | Pre-implementation |
| 2 | Widen parameter types (byte/short -> int) | Pre-implementation |
| 3 | Tapered evaluation (replace discrete phases) | Pre-implementation |
| 4 | Data extraction from Lichess PGN | Tuner |
| 5 | Tuner core (K fit + optimizer) | Tuner |
| 6 | Time estimate and run strategy | Tuner |
| 7 | Validation and Elo match | Release |

Do phases 0-3 in order, each as its own commit with a gauntlet or bench check. Do not tune before phase 3 is done,
otherwise the tuned values are thrown away when the structure changes.

---------------------------------------------------------------------

## Phase 0 - Baseline fixes and an evaluation entry point

### 0.1 Record a baseline
Before any change, save a bench (nodes, time, best move) on a fixed FEN set and run a small match (e.g. 200 games, fast TC) against the current build.
Keep this build as `baseline.exe`.

### 0.2 Fix the Middle-phase rook bug (probable)
File: `Engine/Models/Boards/Evaluation/Board.Evaluation.cs`, `EvaluateWhiteMiddle` and `EvaluateBlackMiddle`.
They call `EvaluateWhiteRookOpening()` / `EvaluateBlackRookOpening()`.

```csharp
// EvaluateWhiteMiddle
if (Unsafe.Add(ref boardBase, Pieces.WhiteRook).Any())
	value += EvaluateWhiteRookMiddle();   // was EvaluateWhiteRookOpening()
```

First check that `EvaluateWhiteRookMiddle` / `EvaluateBlackRookMiddle` exist. If they do not, find out whether the Opening call is intentional
(compare the Opening and Middle config values). If the values are identical, the bug is harmless. Either way phase 3 removes the split.
Run the match after this fix to see whether it changes strength.

### 0.3 Check the evaluation cache vs rule-50 scaling
File: `Board.Evaluation.cs` (`EvaluateCached`) and `Board.Evaluation.Draw.cs` (`ScaleByRule50`).
The cache key is `Hash` + phase + castle rights. If `ScaleByRule50` or any other term depends on state outside the key
(reversible move count, ply, side to move), it must be applied after the cache lookup, not inside `ComputeEvaluation`.

Test: play random playouts and assert cached == uncached.

```csharp
// debug-only check
int cached = board.Evaluate();
int raw = board.ComputeEvaluationForTests(phase); // expose internally for tests
Debug.Assert(cached == raw);
```

### 0.4 Pure evaluation entry point for the tuner
Add to `Board` (internal, not on the search hot path):

```csharp
// Board.Evaluation.cs
internal int EvaluateNoCache(byte phase)
{
	_evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);
	return ComputeEvaluation(phase);   // white-relative, no cache, no rule-50 scaling
}
```

It must not read `MoveHistoryService` (phase and castle rights are passed in or derived from the FEN).
Check whether the pawn hash is keyed only on pawn bitboards. If it is, it stays valid while tuning pawn values only if you clear it
after every parameter change (`Array.Clear` on the three pawn hash tables).
Simplest: add `ResetEvaluationCaches()` that clears both caches, and call it after each parameter change.

### 0.5 FEN loading without history side effects
The tuner needs `SetFen(fen)` and the phase/castle state. Find the existing FEN loader (search `Fen`). If none sets `_phases`/castle rights
consistently, add a helper that sets them from the FEN.

---------------------------------------------------------------------

## Phase 1 - Tempo term

Add `TempoValue` to `BoardEvaluation` (per phase, int) and `Configuration.json`.
Because evaluation is white-relative and cached by Hash, add the tempo in `Position.GetValue()`, not in the cached value:

```csharp
// Position.cs
public int GetValue()
{
	int v = _turn == Turn.White ? _board.Evaluate() : _board.EvaluateOpposite();
	return v + _board.GetTempo();   // always from the side to move's view
}
```

Note: `GetStaticValue()` and qsearch stand-pat should include it too. Check every caller of `GetValue` / `GetStaticValue` for consistency.
Initial value 10-15 cp, tuned later (it is a tuner parameter).
Risk: futility/razoring margins were calibrated without it; effect is small.

---------------------------------------------------------------------

## Phase 2 - Widen parameter types

File: `Engine/Services/Evaluation/EvaluationServiceBase.cs` (fields such as `byte _doubleBishopValue`, `byte _knightMobilityValue`, ...)
and `Engine/Models/Config/BoardEvaluation.cs`, `KingSafetyEvaluation.cs`.

Change `byte`/`short` to `int` for every tunable scalar, and `byte[]` to `int[]` for `PieceAttackValue`, `MobilityValues`, passed-pawn arrays.
Remove `(byte)` casts in `Initialize(phase)`. `CellBuffer<byte>` passed-pawn tables become `CellBuffer<int>` or `short`.
Penalty terms that are subtracted must be handled consistently (check each term is added or subtracted, the sign convention stays the same).
Keep names unchanged to avoid unrelated churn.

Check: bench result must be identical before and after (no behavior change). Speed impact should be negligible; measure nps.

Also add a way to set parameters at runtime for the tuner:

```csharp
// EvaluationServiceBase
public abstract class EvaluationServiceBase
{
	public void ApplyParameters(ReadOnlySpan<int> p, ParameterMap map) { /* copy into fields/tables */ }
}
```

---------------------------------------------------------------------

## Phase 3 - Tapered evaluation

Replace discrete Opening/Middle/End with two scores (mg, eg) and a material-based phase weight.

```csharp
// phase 0..24: N=1, B=1, R=2, Q=4 per piece
int phase = Math.Min(24, 1*(knights+bishops) + 2*rooks + 4*queens);
int score = (mg * phase + eg * (24 - phase)) / 24;
```

Implementation outline:
1. Merge the Opening and Middle config blocks into one `Mg` block (start from Middle values, since Opening and Middle differ only slightly).
   Keep `End` as `Eg`.
2. Evaluation computes both `mgScore` and `egScore` in one pass over pieces (each term adds to both).
   This costs about 1.3-1.6x the current evaluation time (estimate), but the eval cache and pawn hash absorb most of it.
3. Pawn hash entries store `(mg, eg)` pairs.
4. Evaluation cache stores the final blended value. The key is Hash only (phase is derived from the position), plus castle rights if still needed.
5. Keep `MoveHistoryService.SetPhase` only for search heuristics (end-game strategy switch, LMR); evaluation no longer depends on it.
6. Keep `GetMopUpValue` in the eg score only.

Run the match against `baseline.exe`. Expect roughly equal or better strength. If clearly worse, check the signs and the phase
direction before tuning.

Alternative if time is short: keep three discrete phases and tune each block (smaller gain, no cliff fix).

---------------------------------------------------------------------

## Phase 4 - Data extraction (Lichess PGN)

### 4.1 Source and size
Lichess monthly files are `.pgn.zst`, tens of GB compressed. You only need a prefix. Stream and stop at N games. Do not decompress fully.
Options: `zstd -d -c file.pgn.zst | head` style streaming, or a NuGet zstd decoder (e.g. `ZstdSharp.Port`) with `DecompressionStream`.

```csharp
using var fs = File.OpenRead(path);
using var zs = new ZstdSharp.DecompressionStream(fs);
using var reader = new StreamReader(zs);
```

The existing `Engine.Pgn/Streaming` reader can be reused for parsing. Check that it accepts a `TextReader`.

### 4.2 Game filters
- Both players Elo >= 2000 (2200+ is better if available), tags `WhiteElo`/`BlackElo`.
- `Event` contains Classical, Rapid or Blitz (skip Bullet/UltraBullet). Skip `Termination` = Abandoned/Rules infraction.
- Result `1-0`, `0-1` or `1/2-1/2` only (skip `*`).
- Prefer games with `[%eval]` clock comments if present, but this is not required. Evaluate by game result only.

### 4.3 Position filters (per game)
- Skip the first 8 plies (book positions).
- Skip the last 2 plies and everything after a mate or draw is clearly decided.
- Skip positions where the side to move is in check or where the move played is a capture or promotion (non-quiet).
  Cheap and effective approximation of quiet-position selection.
- Sample at most 8-12 positions per game (random or every Nth ply) to reduce correlation between positions from the same game.
- Skip positions with `|material difference| > ~1500` (already decided) - optional.
- Drop duplicate positions by Hash (keep first).

Result of 100,000 games: about 0.8-1.2 million positions (estimate).

### 4.4 Output format
Binary, compact. Evaluation needs a Board, so store FEN-free data:

```csharp
// 4.4 record: 8 bitboards or packed pieces, side to move, castle, result
struct TexelRecord
{
	public ulong WhitePawns, WhiteKnights, WhiteBishops, WhiteRooks, WhiteQueens, WhiteKing;
	public ulong BlackPawns, BlackKnights, BlackBishops, BlackRooks, BlackQueens, BlackKing;
	public byte Castle;      // 4 bits
	public byte Stm;         // 0 white, 1 black
	public sbyte Result;     // 1, 0, -1 (white's POV) -> map to 1.0 / 0.5 / 0.0
}
```

That is about 100 bytes per position, so 1M positions is about 100 MB. If memory is a problem, store FEN text in a text file and parse on load.

### 4.5 Extraction time (estimate)
- Decompress and parse 100k games: about 1-4 minutes of CPU (zstd is fast, PGN SAN parsing dominates).
- Move generation and quiet checks: about 100k x 80 plies x ~5 microseconds = 40 seconds.
- Total: about 2-6 minutes plus disk read. This is a one-off cost.

Put the tool in a new console project (e.g. `TexelTool`) next to `PgnTool`. Reference `Engine` and `Engine.Pgn`. Do not put it in the Engine assembly.

---------------------------------------------------------------------

## Phase 5 - Tuner core

### 5.1 Error function

```csharp
// E = mean((R - sigmoid(K * eval / 400))^2), R in {0, 0.5, 1}, eval is white-relative
static double Sigmoid(double eval, double k) => 1.0 / (1.0 + Math.Exp(-k * eval / 400.0));

double Error(double k)
{
	double sum = 0;
	for (int i = 0; i < n; i++)
	{
		double d = results[i] - Sigmoid(scores[i], k);
		sum += d * d;
	}
	return sum / n;
}
```

### 5.2 Fit K first
With the current evaluation unchanged, find K minimizing `Error(K)` (golden-section on [0.5, 2.5]).
Then freeze K. Do not re-fit K while tuning.
Compute `scores[]` once, then loop (fast: one eval pass plus cheap K search).

### 5.3 Parameter vector and map
Flat `int[] p` with a table describing each parameter:

```csharp
record ParamInfo(string Name, int Index, int Min, int Max, int Step, string Group);
```

Groups, in tuning order (each tuned separately, with other groups frozen):
1. Material (mg/eg: N, B, R, Q; pawn eg only, keep pawn mg = 100 as the scale anchor)
2. PSTs (6 pieces x 64 x 2 = 768) - use symmetric or file-mirrored PSTs if you want to reduce the count (halves it)
3. Mobility (4 x 2)
4. Passed pawn tables (rank 2-7, mg/eg, plus protected/connected)
5. Pawn structure (doubled, isolated, backward, blocked)
6. Rook terms, bishop pair, pins, batteries
7. Tempo
8. King safety last, small steps, `AttackWeight` kept monotone (clamp after every step)

Total: about 900-1,000 parameters.

### 5.4 Optimizer - two options

**Option A (simplest, uses your existing evaluation, no refactor): local search (coordinate descent).**

```csharp
bool improved = true;
double best = Error(K);
while (improved)
{
	improved = false;
	foreach (var info in group)
	{
		foreach (int delta in new[] { +info.Step, -info.Step })
		{
			Set(info.Index, p[info.Index] + delta);
			ResetEvaluationCaches();
			double e = FullPassError();
			if (e < best) { best = e; improved = true; break; }
			Set(info.Index, p[info.Index] - delta); // revert
		}
	}
}
```

Cost is one full evaluation pass per trial, so about `params x 2 x passes x N x t_eval`.
It is correct and easy, but slow (see Phase 6).

**Option B (much faster, extra work): sparse linear "trace".**
For each position store the nonzero coefficient list `(paramIndex, coeff_mg, coeff_eg)` once, plus the phase.
Because the tapered evaluation is `sum(coeff x param)`, the error and the gradient are computed without calling the engine.
Apply Adam over all parameters at once.

```csharp
// score = (sum_mg * phase + sum_eg * (24 - phase)) / 24
// gradient: dE/dp_j = sum_i  -2 (R_i - s_i) * s_i (1 - s_i) * (K/400) * coeff_ij * phaseWeight_i
```

Non-linear terms (king safety `AttackWeight` table, mop-up) need either a frozen value or a custom coefficient. Tune them with Option A at the end.
Extraction of the traces needs a "tracing" mode in the evaluation, where each term records its coefficient.
That is the most invasive code change; do it only if Option A is too slow.

Recommended: implement Option A first for the first run (material, mobility, tempo - the cheap groups), then decide.
Option A with the eval cache disabled and 1 thread is the baseline in the estimate below.

### 5.5 Parallelism
Positions are independent, so use `Parallel.For` over chunks. Each worker needs its own `Board`, because `Board` and `MoveHistoryService` hold mutable state.
Check that `ContainerLocator` singletons (e.g. `MoveHistoryService`, `EvaluationServiceFactory`) are not shared in a way that blocks this.
Evaluation services hold the parameters: share them read-only during a pass and change them only between passes.
If thread-safety is not possible without a big refactor, run single-threaded for the first version.

### 5.6 Regularization and bounds
- Clamp parameters to sensible ranges (Min/Max in `ParamInfo`).
- Keep the pawn mg value fixed at 100.
- Optionally add a small L2 penalty toward the starting values for PST entries to limit overfitting.
- Hold out 10-20% of positions (split by game, not by position) for validation. Stop when validation error stops decreasing.

### 5.6 Output
Write the tuned values back to `Configuration.json` (a copy, e.g. `Configuration.tuned.json`), and print a diff against the old values.

---------------------------------------------------------------------

## Phase 6 - Time estimate (limited hardware)

Assumptions (replace with measured numbers):
- Positions: 100,000 games x ~10 sampled positions = about 1,000,000.
- Uncached tapered evaluation: about 1.5-4 microseconds per position on a modest PC (the existing eval computes attacks on every call).
  Measure it first with a 10-line benchmark: evaluate 100k positions, divide.
- A full pass (1 thread): 1M x 3 us = about 3 s. Parallel on 4 cores: about 1 s.

| Method | Cost per pass | Passes | Estimated wall time (1 thread / 4 threads) |
|--------|---------------|--------|--------------------------------------------|
| K fit | 1 pass + cheap loop | 1 | seconds |
| Option A, whole vector (~1,000 params, 2 directions, ~5 sweeps) | 3 s | ~10,000 | about 8 h / about 2-3 h |
| Option A, per group, early groups only (~60 params, 10 sweeps) | 3 s | ~1,200 | about 1 h / about 20 min |
| Option A with 300k positions | 1 s | same counts | about 3x faster than above |
| Option B, Adam on traces (1M positions, ~30 active features each) | about 30-100 ms | 2,000-5,000 | about 1-8 min |

Practical guidance:
- Option A is feasible if you reduce the data: 100k games is plenty; use 30-40k games (about 300-400k positions) for the first runs.
- To cut Option A cost by about 3-10x: only re-evaluate positions affected by a parameter, e.g. when tuning a rook PST entry,
  evaluate only positions containing a rook (pre-index by piece type).
- Mobility, material and tempo groups are cheap to tune and give a good share of the gain.
- Option B is the right choice for the PST groups (768 params). If your PC time is limited, it is worth the extra implementation work.
- Use the weak PC only for a few long runs; extraction and the K fit can be done in minutes.
- Make runs resumable: save `p[]` and the group index to disk every sweep.

The numbers above are rough estimates. Run the micro-benchmark first and recompute.

---------------------------------------------------------------------

## Phase 7 - Validation

1. Validation error on the held-out set must be lower than before tuning. If training error falls and validation rises, you are overfitting.
2. Sanity: start position evaluates close to the tempo value; mirrored positions give the opposite score.
3. Bench: nps must not drop significantly vs. phase 3.
4. Match: tuned vs. baseline (and vs. the phase 3 build), at least 1,000 games at fast TC. A 2-3% score difference is about 15-20 Elo.
   Use SPRT (e.g. `elo0=0, elo1=5`) if you have cutechess-cli. Do not claim Elo from the Texel error alone.
5. Re-check pruning margins (futility 200/450, reverse futility 150-775, razoring 700, delta 375) if the tuned material values changed the scale.
6. Repeat the cycle after any evaluation structure change.

Suggested regression positions:
- `rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1` -> eval = tempo only.
- Mirror test: for 1,000 random positions, `eval(pos) == -eval(mirror(pos))` (white-relative, tempo excluded).
- `8/8/8/4k3/8/3K4/4R3/8 w - - 98 100` -> check rule-50 scaling is not cached incorrectly.

---------------------------------------------------------------------

## Addendum - Project layout, existing PGN infrastructure, configuration rules

This section overrides Phase 4/5 wherever they conflict. No code has been changed yet.

### A. Is a new tuner project needed?
Yes: create one console project, `TexelTool` (net10.0). Reasons:
- Existing tools (`PgnTool`, `PgnManager`) have a different purpose, and the tuner needs its own data store and optimizer.
- The Engine must not depend on tuner code.
- It follows the existing pattern: tools reference `Engine`, `Engine.Pgn` and `Tools.Common`.

References for `TexelTool.csproj`: `Engine`, `Engine.Pgn`, `Tools.Common`. Copy the `Config\*.json` link/copy settings from `PgnTool.csproj`
(`Boot.SetUp()` reads `Config\Configuration.json` and `Config\StaticTables.json` from the output directory).
Add the project to `KioChess.sln`. The only new package is a zstd decoder (see C), and only if you do not decompress with an external tool.

No new project is needed for the engine-side changes (phases 0-3): they live in `Engine`.

### B. Existing PGN infrastructure to reuse (do not write a new parser)
Found in `Engine.Pgn`:
- `Streaming/PgnStreamReader`: `new PgnStreamReader(Stream stream, bool leaveOpen)` or by file path.
  - `ReadGames()` is a lazy `IEnumerable<PgnGame>` with tags and parsed moves.
  - `ReadGameInfo()` is tags only (cheap pre-filter, but it reads the whole stream, so it is not useful for a prefix scan).
  - `ReadGameText()` returns raw text.
  - Also exposes `Position`, `Length` and `ProgressPercent` (progress will not be meaningful over a zstd stream, because `Length` is unsupported on a decompression stream; guard it).
- `Models/PgnGameInfo`: `WhiteElo`, `BlackElo`, `MinElo`, `Event`, `Result`, `GetTag(name)`.
- `Models/PgnGame`: tags, `Result`, moves.
- `PgnTool/PgnConverter` and `Boot.SetUp()` (`Tools.Common`) show how to replay moves on a `Position` and how to build the DI container.

Reuse plan:
1. Open the `.pgn` (or a `ZstdSharp.DecompressionStream`) and pass the stream to `PgnStreamReader(stream)`.
2. Enumerate `ReadGames()` and stop after `MaxGames` (config).
3. Filter on tags using the same tag names (`WhiteElo`, `BlackElo`, `Event`, `Termination`, `Result`).
   `PgnGame` has `Tags`; if it has no `WhiteElo` helper, add one to `PgnGame` (small change in `Engine.Pgn`, not a new parser).
4. Replay moves with the same code path `PgnConverter` uses (look at `PgnConverter.cs` lines ~29-70), and sample quiet positions.

Open points to verify when implementing (not verified yet):
- `PgnGame.Moves` format (SAN strings vs `MoveBase`) and how comments such as `{ [%clk ...] }` are handled.
- Whether the parser accepts Lichess movetext with `[%eval ...]` and `[%clk ...]` comments.
- `Length` on non-seekable streams: `PgnStreamReader.Length` will throw. Do not call `ProgressPercent` for zstd input.

### C. Threading constraint (important for the tuner)
`Boot.SetUp()` registers `MoveHistoryService`, `EvaluationServiceFactory`, `MoveProvider` and `DataPoolService` as container singletons.
The `Position` constructor resolves them. So all `Position`/`Board` instances share one `MoveHistoryService`, which holds ply and phase state.
Consequences:
- Parallel evaluation with several `Position` objects is not safe as-is.
- Mitigation for the first version: the tuner never replays games during tuning. It evaluates from a precomputed feature/trace store or from a
  single `Position` per thread with phase/castle passed explicitly (phase 0.4 `EvaluateNoCache(...)` must not touch `MoveHistoryService`).
- Preferred: Option B (traces). Extraction is single-threaded (replay needs the shared history), tuning does not need the Engine at all.
  This also removes the Engine from the hot loop and is the main reason Option B is recommended for the limited-hardware case.

### D. Everything configurable goes to `Configuration.json`
No magic numbers in code. Add these sections and matching types (names follow the existing config style, such as `GeneralConfiguration`):

```json
"TexelConfiguration": {
  "Data": {
    "PgnPath": "C:\\Dev\\PGN\\lichess_db_standard_rated_2024-01.pgn.zst",
    "OutputPath": "C:\\Dev\\Texel\\positions.bin",
    "MaxGames": 100000,
    "MinElo": 2000,
    "MaxEloDifference": 400,
    "ExcludedEvents": [ "Bullet", "UltraBullet" ],
    "ExcludedTerminations": [ "Abandoned", "Rules infraction" ],
    "SkipOpeningPlies": 8,
    "SkipLastPlies": 2,
    "MaxPositionsPerGame": 10,
    "SkipChecks": true,
    "SkipCaptures": true,
    "SkipPromotions": true,
    "MaxMaterialDifference": 1500,
    "RandomSeed": 12345,
    "ValidationFraction": 0.15
  },
  "Optimizer": {
    "Method": "coordinate",            // coordinate | adam
    "KMin": 0.5,
    "KMax": 2.5,
    "KTolerance": 0.0001,
    "MaxSweeps": 20,
    "MinImprovement": 1e-8,
    "Threads": 1,
    "Groups": [ "Material", "Mobility", "Tempo", "PassedPawns", "PawnStructure", "PieceSquare", "KingSafety" ],
    "Adam": { "LearningRate": 1.0, "Beta1": 0.9, "Beta2": 0.999, "Epsilon": 1e-8, "Epochs": 3000 },
    "L2Penalty": 0.0,
    "CheckpointPath": "C:\\Dev\\Texel\\checkpoint.json",
    "OutputConfigurationPath": "C:\\Dev\\Texel\\Configuration.tuned.json"
  },
  "Parameters": {
    "PawnValueAnchor": 100,
    "Steps":  { "Material": 5, "Mobility": 1, "Tempo": 1, "PassedPawns": 2, "PawnStructure": 1, "PieceSquare": 1, "KingSafety": 1 },
    "Bounds": { "Material": [ 0, 1500 ], "Mobility": [ -20, 40 ], "Tempo": [ 0, 40 ], "PassedPawns": [ 0, 300 ], "PawnStructure": [ -60, 60 ], "PieceSquare": [ -150, 150 ], "KingSafety": [ 0, 200 ] }
  }
},
```

Engine-side additions (also in `Configuration.json`, not in code):

```json
"Evaluation": {
  "Static": {
    "Tempo": { "Opening": 12, "End": 6 },
    "Taper": {
      "PhaseWeights": [ 0, 1, 1, 2, 4, 0 ],  // P, N, B, R, Q, K
      "MaxPhase": 24
    },
    "Draw": { "...": "existing" }
  }
}
```

Types to create (in `Engine/Models/Config`, mirroring existing classes with public getters/setters so Newtonsoft binds them):

```csharp
public class TexelConfiguration
{
    public TexelDataConfiguration Data { get; set; }
    public TexelOptimizerConfiguration Optimizer { get; set; }
    public TexelParametersConfiguration Parameters { get; set; }
}

public class TexelDataConfiguration
{
    public string PgnPath { get; set; }
    public string OutputPath { get; set; }
    public int MaxGames { get; set; }
    public int MinElo { get; set; }
    public int MaxEloDifference { get; set; }
    public string[] ExcludedEvents { get; set; }
    public string[] ExcludedTerminations { get; set; }
    public int SkipOpeningPlies { get; set; }
    public int SkipLastPlies { get; set; }
    public int MaxPositionsPerGame { get; set; }
    public bool SkipChecks { get; set; }
    public bool SkipCaptures { get; set; }
    public bool SkipPromotions { get; set; }
    public int MaxMaterialDifference { get; set; }
    public int RandomSeed { get; set; }
    public double ValidationFraction { get; set; }
}

public class TexelOptimizerConfiguration { /* Method, KMin, KMax, KTolerance, MaxSweeps, MinImprovement, Threads, Groups, Adam, L2Penalty, CheckpointPath, OutputConfigurationPath */ }
public class AdamConfiguration { public double LearningRate, Beta1, Beta2, Epsilon; public int Epochs; }
public class TexelParametersConfiguration { /* PawnValueAnchor, Steps (Dictionary<string,int>), Bounds (Dictionary<string,int[]>) */ }
public class TempoConfiguration { public int Opening { get; set; } public int End { get; set; } }
public class TaperConfiguration { public int[] PhaseWeights { get; set; } public int MaxPhase { get; set; } }
```

Wiring: add `TexelConfiguration` to `Configuration`, and expose it through `IConfigurationProvider` (property `TexelConfiguration`).
`Boot.SetUp()` passes sections to the `ConfigurationProvider` constructor, so its signature and the call in `Boot.cs` need one extra argument.
Check other callers of `new ConfigurationProvider(...)` (the engine app, the `EvaluationEditor`, tests) when you do it.
`EvaluationEditor` edits the evaluation config; keep new `Evaluation` fields compatible with it.

Existing numbers that currently look hard-coded and must move into config in phases 0-3 (verify each by search while implementing):
- Phase boundaries: `16` plies in `MoveHistoryService` (opening length), and the `IsEndGame()` thresholds (already in `BoardState`).
- The 24 / piece weights for tapering (use `Taper` above).
- Rule-50, mop-up and contempt values are already in `Evaluation.Static.Draw`; keep them there.
- Pawn hash and evaluation cache sizes are already config (`PawnHashBits`, `EvaluationCacheBits`).
- The pawn eval anchor (100) and the K search bounds are config as above.

### E. Updated phase 4 deliverables for `TexelTool`
- `Program.cs`: `Boot.SetUp()`, read `TexelConfiguration`, command line: `extract`, `tune`, `bench`, `validate`.
- `Extraction/PositionExtractor.cs`: reuses `PgnStreamReader.ReadGames()` and the `PgnConverter` replay logic.
- `Data/TexelRecord.cs` and `Data/TexelStore.cs`: binary read/write (see 4.4).
- `Tuning/ParameterMap.cs`, `Tuning/ErrorFunction.cs`, `Tuning/KFitter.cs`, `Tuning/CoordinateOptimizer.cs`, `Tuning/AdamOptimizer.cs`.
- `Tuning/ConfigurationWriter.cs`: writes the tuned values into `Configuration.tuned.json`.

---------------------------------------------------------------------

## Checklist

- [ ] 0.1 baseline bench and match saved
- [ ] 0.2 Middle rook fix
- [ ] 0.3 cache / rule-50 check
- [ ] 0.4 `EvaluateNoCache` and cache reset
- [ ] 0.5 FEN loader sets phase/castle state
- [ ] 1 tempo term
- [ ] 2 parameter types widened, bench identical
- [ ] 3 tapered evaluation, match vs baseline
- [ ] 4 `TexelTool` extraction, 100k games
- [ ] 5.2 K fitted
- [ ] 5.4 optimizer (A, then B if needed)
- [ ] 6 timing measured
- [ ] 7 validation and match
