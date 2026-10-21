# Futility / Reverse Futility / Razoring Refactor Plan

Branch: `131-Futility`. Files: `Engine/Strategies/Base/StrategyBase.cs`, `Engine/Strategies/Lmr/LmrStrategyBase.cs`, `Engine/Services/MoveProvider.Core.cs`, `Engine/Config/Configuration.json`.

## 1. Current design

Class tree: `StrategyBase` holds all pruning. `LmrStrategyBase` overrides only `SearchInternalWhite/Black`. `NullStrategy`, `NegaMaxMemoryStrategy`, `IteretiveDeepingStrategy`, `AspirationStrategy` and `End/*` derive directly from `StrategyBase`. The `IdLmrDeepEnd` and `IdItemLmrDeepEnd` strategies override `SearchWhite/Black` (not verified whether they reach this pruning).

Node flow (`Common{White,Black}{Pv,NonPv}Search`):
1. TT probe
2. null-move depth adjustment (`CanUseNull`)
3. `GetCurrentContext` -> `SetupSearchContext` generates and sorts all moves
4. `SearchResultType` chosen: EndGame (no moves) / OneReply / Check (extension) / else `depth > RazoringDepth ? None : GetSearchResultType`
5. `SetSearchValueX` dispatches

`GetSearchResultType` (RazoringDepth = FutilityDepth + 1 = 3):
- depth 1..2: `eval + AlphaMargin < alpha` -> AlphaFutility (skip quiet non-check moves, fail-hard `alpha` if all skipped, no TT store); `eval - BetaMargin > beta` -> BetaFutility (return `beta`, no TT store)
- depth 3: `eval + AlphaMargin < alpha` -> Razoring (plain depth-1 reduction, no qsearch verification)

Margins: `AlphaMargins[phase] = [pawn, bishop+off0, rook+pawn+off1, queen+off2]`, same for Beta with `BetaOffset`. Indexed by depth, so depth 1 ~ bishop, depth 2 ~ rook+pawn, depth 3 ~ queen.

## 2. Findings

### Confirmed bugs
1. `isLowPiece` in promotion loops checked `WhiteBishop` twice, missing `BlackBishop` (colour asymmetry in `IsFutile`, `CanReduce`, `CanNotReduceNext`). **Fixed in Phase 0.**
2. Promotion-extension pawn pushes (7th/2nd rank) kept `IsFutile = true`. **Fixed in Phase 0.**
3. Config coupling: margin arrays have 4 entries, `FutilityDepth > 2` throws IndexOutOfRange; `new int[3][]` vs `ess.Length`. (Phase 0 step 4)

### Soundness risks
4. No mate-score guard in futility/reverse futility/razoring. **Fixed in Phase 0.**
5. Reverse futility runs in PV nodes, without non-pawn-material / zugzwang guard, after move generation.
6. Fail-hard returns lose information (`beta` / `alpha` instead of `eval -/+ margin`).
7. Partially-pruned futility nodes are stored in TT at full depth.
8. Razoring reduced depth stacks with LMR; TT store uses reduced depth (PV check `depth >= entry.Depth`).
9. Futile-move test runs after `MakeX` (needs `IsCheck` computed on make); make/unmake wasted on pruned moves.
10. No exemptions for TT/PV move, killers, high history, passed-pawn pushes; mixed `<` / `<=`.

### Strength
- Margins are large (~330/600/900 cp), so pruning rarely fires; razoring with a queen margin is nearly dead code.
- Only depths 1-2 covered; reverse futility at depths 3-5 is the main gain at depth ~10.
- No LMP, no history/SEE pruning, no `improving`, razoring has no qsearch verification.

### Performance
- Move generation + sorting happen before the pruning decision; BetaFutility discards them. Biggest waste.
- Jagged `AlphaMargins[phase][depth]` lookups, `GetValue()` / `GetPhase()` per node.
- Make/unmake for skipped futile moves.
- White/Black futility loops duplicated and diverge (`a` update).

## 3. Plan

### Phase 0 - fixes
1. [DONE] Fix `isLowPiece`:
```csharp
var isLowPiece = move.PromotionPiece == Pieces.WhiteKnight || move.PromotionPiece == Pieces.WhiteBishop
			  || move.PromotionPiece == Pieces.BlackKnight || move.PromotionPiece == Pieces.BlackBishop;
```
2. [DONE] Promotion-extension pushes are not futile:
```csharp
move.IsFutile = !move.IsPromotionExtension && !move.IsAttack && !move.IsPromotion;
```
3. [DONE] Mate guard in `GetSearchResultType`:
```csharp
int mateThreshold = GetMateThreshold();
bool alphaSafe = Math.Abs(alpha) < mateThreshold;
bool betaSafe = Math.Abs(beta) < mateThreshold;
if (!alphaSafe && !betaSafe) return SearchResultType.None;
...
if (alphaSafe && value + AlphaMargins[phase][depth] < alpha) return SearchResultType.AlphaFutility;
if (betaSafe && value - BetaMargins[phase][depth] > beta) return SearchResultType.BetaFutility;
```
4. [TODO] Robust margin tables: validate `FutilityDepth + 1 <= AlphaMargins[i].Length - 1` at construction (throw a clear config exception); size outer arrays by `ess.Length`; use `<=` consistently.

### Phase 1 - restructure (speed)
5. [DONE] Pre-generation reverse futility in `Common*Search`, implemented as `TryReverseFutilityWhite/Black` in `StrategyBase` (called at the top of `CommonWhite/BlackPvSearch` and `CommonWhite/BlackNonPvSearch`, before null move and move generation). Uses colour-specific `_board.Evaluate()` / `_board.EvaluateOpposite()`, `BetaMargins[phase][depth]`, depth < RazoringDepth, not in check, `|beta| < mateThreshold`, side non-zugzwang; returns fail-soft `eval - margin`. Note: the engine's "Pv" methods are TT-hit paths (not true PV nodes) and there is no PVS null window, so no PV exclusion is applied.
```csharp
if (!IsPv && depth <= ReverseFutilityDepth && !MoveHistory.IsLastMoveWasCheck()
	&& Math.Abs(beta) < GetMateThreshold() && _board.IsWhiteNonZugzwang())
{
	int eval = Position.GetValue();
	int margin = ReverseFutilityMargins[phase][depth];
	if (eval - margin >= beta) return eval - margin; // fail-soft
}
```
6. [DONE] Removed `BetaFutility` (enum value, `SetSearchValueX` cases, `GetSearchResultType` branch); `GetSearchResultType(alpha, depth)` now handles alpha futility and razoring only.
7. [DONE] Flattened margins: `AlphaMargins[phase * AlphaStride + depth]` (futility depths 1..FutilityDepth, then razoring depths up to RazoringDepth) and `BetaMargins[phase * BetaStride + depth]`.

### Phase 2 - configuration
8. [DONE] Explicit per-phase margin arrays in `MarginConfiguration` (`FutilityMargin`, `ReverseFutilityMargin`, `RazoringMargin`; index 0 = depth 1). Depths derive from array lengths (`FutilityDepth = len`, `ReverseFutilityDepth = len`, `RazoringDepth = FutilityDepth + razoring len`); `GeneralConfiguration.FutilityDepth` and the offset arrays were removed. No enable flags (a zero-length array disables a feature). The constructor validates 3 phases of equal length. Values were migrated 1:1 from the old formulas (pawn 100, bishop 325, rook 500, queen 990). Original proposal, superseded:
```json
"PruningConfiguration": {
  "FutilityEnabled": true, "FutilityDepth": 2, "FutilityMargin": [0, 150, 300],
  "ReverseFutilityEnabled": true, "ReverseFutilityDepth": 5, "ReverseFutilityMargin": [0, 90, 180, 270, 360, 450],
  "RazoringEnabled": true, "RazoringDepth": 3, "RazoringMargin": [0, 0, 250, 450]
}
```

### Phase 3 - strength
9. Retune margins (starting points above are untested guesses).
10. [DONE - untested] Reverse futility extended to depth 5 by lengthening `ReverseFutilityMargin` to `[90, 180, 270, 360, 450]` per phase (no code change; depth comes from array length). Non-PV exclusion is NOT implemented: the engine has no PVS/null-window and no PV flag, so window width cannot identify PV nodes. Reverse futility only returns fail-high values (`> beta`), so it cannot replace an exact PV score, but it can truncate PV lines.
11. Real razoring: if `eval + margin < alpha`, qsearch with null window at alpha; return if `<= alpha`, else normal search. Do not stack with LMR.
12. Futility exemptions: TT move, killers, passed-pawn pushes, high relative history.
13. Late-move pruning for quiet moves at depth <= 2-3 (interacts with `LmrIndex`).
14. Optional `improving` flag (needs per-ply static eval).

### Phase 4 - validation
- Perft (must be unchanged by Phase 0), tactical suites (WAC, ECM, mate-in-N), A/B match tests per step using feature toggles.
- Regression positions: black bishop promotion; zugzwang endgame `8/8/p1p5/1p5p/1P5p/8/PPP2K1p/4R1rk w - - 0 1` with reverse futility; hanging queen at depth 1-2; quiet mate-in-2 for mate guard.

## 4. Interactions
- Null move: reverse futility before it; avoid both firing on one node.
- LMR: razoring and LMR stack today; retune together.
- TT: pre-generation cutoffs store nothing (if added later: LowerBound, reduced depth).
- Check/OneReply paths already skip pruning; keep.
- Aspiration/ID share margins; verify End strategies separately.
