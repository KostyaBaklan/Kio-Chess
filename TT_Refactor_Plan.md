# Transposition Table Refactor Plan

Status: PLAN ONLY. No code has been changed. Implement step by step. Run the validation gate after each step.

Files in scope:
- `Engine/DataStructures/TranspositionHashSet.cs`
- `Engine/DataStructures/TranspositionTable.cs`
- `Engine/Models/Transposition/TranspositionEntry.cs` (6 bytes: `short Value, short PvMove, sbyte Depth, byte Type`)
- `Engine/Strategies/Base/StrategyBase.cs` (probe at `CommonWhiteSearch`/`CommonBlackSearch`, stores in the `Common*PvSearch`/`Common*NonPvSearch` methods, `FutilitySearchInternal*`)
- `Engine/Strategies/Base/NullStrategy.cs` (stores at lines ~120 and ~179)
- Config: `GeneralConfiguration.CutoffDepth`, `TranspositionTableDepthFactor`, `TranspositionTableTypeFactor`

Evidence legend: [CONFIRMED] seen in code. [VERIFY] hypothesis, check first. [EXPERIMENT] needs A/B match testing.

---

## Step 0. Baseline (before any change)

Record on 10-20 fixed positions (opening, middlegame, tactical, endgame, mate-in-N) at depth 8, 10 and 12:
- best move, score, nodes, time, and TT hit rate if available
- the node count of a pure-performance change must stay IDENTICAL (steps 5, 6)
- strength-affecting steps (1, 3, 4, 7, 8) need an SPRT or self-play match

Suggested sanity positions:
- `6k1/5ppp/8/8/8/8/5PPP/3R2K1 w - - 0 1` (Rd8#, mate score normalization)
- `r1bqkbnr/pppp1ppp/2n5/4p3/2B1P3/5Q2/PPPP1PPP/RNB1K1NR w KQkq - 0 1` (Qxf7# tactic)
- a K+P zugzwang endgame such as `8/8/8/3k4/8/3K4/3P4/8 w - - 0 1`

---

## Step 1. Do not store a hash move for fail-low (UpperBound) nodes

[CONFIRMED] Stores pass `context.BestMove` regardless of `entryType`. On a fail-low every move failed to raise alpha, so "best" is arbitrary. As a hash move it is tried first at later visits and wastes the best ordering slot.

Rule:
- Exact / LowerBound: store the move.
- UpperBound: store `-1` (the "no move" value that `TranspositionEntry` uses by default).
- If an entry for the same key already exists, `Set` (step 3) keeps the old move when the new `PvMove < 0`.

Change at 4 sites in `StrategyBase` (White/Black x Pv/NonPv) and the 2 sites in `NullStrategy`:

```csharp
TranspositionEntryType entryType = ComputeTranspositionEntryType(alpha, beta, context.Value);
short move = entryType == TranspositionEntryType.UpperBound ? (short)-1 : context.BestMove;

Table.SetWhite(new TranspositionEntry(depth, (short)ToTTValue(context.Value), move, entryType));
```

Checks:
- [VERIFY] Every consumer of `entry.PvMove` (`GetCurrentContext(alpha, depth, entry.PvMove)` and the sorters) handles `-1` and does not index an array with it.
- [VERIFY] `NullStrategy` stores `bestKey` (line ~120/179); apply the same rule there.
- Risk: very low. Expected effect: better first-move ordering on re-visited fail-low nodes. Gate: SPRT or a node count decrease.

---

## Step 2. Fix static fields and generation age

[CONFIRMED] `_depthFactor` and `_typeFactor` are `static` but assigned in the constructor. `_typeFactor` is never used. Age is computed as `Generation - currentGeneration` on `ushort`s and breaks at wraparound.

```csharp
private readonly int _depthFactor;
private readonly int _typeFactor;
private readonly int _replaceMargin;   // see step 3

// BucketEntry.GetPriority becomes an instance-aware helper on the set:
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private int Priority(ref BucketEntry e)
{
	int age = (ushort)(_currentGeneration - e.Generation);        // modular age
	int typeBonus = e.Entry.Type == TranspositionEntryType.Exact ? _typeFactor : 0;
	return e.Entry.Depth * _depthFactor + typeBonus - age;
}
```

Notes:
- `BucketEntry` is a nested struct, so the factors must be passed in or the priority function moved to `TranspositionHashSet`. Moving it is the simplest option.
- `_typeFactor` finally gives the config value a meaning (Exact bonus). Start with the configured value; tune after.
- Risk: low. `Bucket.Set` (the eviction helper) is removed in step 3.

---

## Step 3. Rewrite `TranspositionHashSet.Set`: single pass and conditional replacement

[CONFIRMED] Problems in the current `Set`:
1. A matching key is overwritten unconditionally, even by a shallower result. The `depth >= entry.Depth` guard exists only in the Pv-path stores. The NonPv path, `NullStrategy` and `FutilitySearchInternal*` have no guard.
2. Empty detection uses `Depth == 0` before the later slots' key match.
3. Up to 10 compares, with no preservation of the old move.

Note: stores only happen with `depth >= 1`, so the "depth 0 is a valid entry" case is mostly latent. [VERIFY] `SetSearchValue*` can never leave `depth <= 0` on a store. If it can, the entry is silently lost.

New logic: one loop that finds a match, otherwise the victim.

```csharp
private const int BucketSize = 5;

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public void Set(ulong key, TranspositionEntry item)
{
	ref Bucket bucket = ref GetBucket(key);              // step 6; until then: ref _buckets[key & _mask]
	uint entryKey = (uint)(key >> KeyShift);

	ref BucketEntry first = ref bucket.Entry1;
	ref BucketEntry victim = ref first;
	int victimPriority = int.MaxValue;

	for (int i = 0; i < BucketSize; i++)
	{
		ref BucketEntry e = ref Unsafe.Add(ref first, i);

		if (e.Entry.Depth == EmptySlotKey)               // empty slot: best victim
		{
			victim = ref e;
			victimPriority = int.MinValue;
			continue;
		}

		if (e.Key == entryKey)                           // same position
		{
			bool replace = item.Depth + _replaceMargin >= e.Entry.Depth
						|| e.Generation != _currentGeneration;
			if (replace)
			{
				if (item.PvMove < 0) item.PvMove = e.Entry.PvMove;   // keep old hash move (step 1)
				e.Entry = item;
			}
			e.Generation = _currentGeneration;
			return;
		}

		int p = Priority(ref e);
		if (p < victimPriority) { victim = ref e; victimPriority = p; }
	}

	victim.Entry = item;
	victim.Key = entryKey;
	victim.Generation = _currentGeneration;
}
```

Notes:
- The match test requires `Depth != 0`, so the "Key == 0 matches an empty slot" artifact disappears.
- `Unsafe.Add(ref first, i)` relies on the 5 `BucketEntry` fields being contiguous. This holds under `Sequential, Pack = 1`. Add a `Debug.Assert(Unsafe.SizeOf<BucketEntry>() == 12)`.
- `_replaceMargin` starts at 0, which equals the existing Pv-path semantic (`depth >= entry.Depth`). [EXPERIMENT] tune 0 to 2.
- When the old entry's generation is stale it is always replaced, so the table follows the current game.
- Keep the Pv-path guard in `StrategyBase` initially. It is now redundant but harmless. Remove it only after the node count is identical.
- Risk: medium (replacement affects search results). Gate: node counts will change; run SPRT.

---

## Step 4. CutoffDepth review (probe-side semantics)

[CONFIRMED] In both probes:

```csharp
if (entry.Depth >= depth && entry.Depth < CutoffDepth)   // StrategyBase lines ~560 and ~588
```

`CutoffDepth = generalConfiguration.CutoffDepth[Depth]`, indexed by ROOT depth. Example config: `[0,1,2,2,2,3,3,cd7..cd11,6,6,6,6,7,7,8,8]`.

Findings:
1. The gate compares the STORED `entry.Depth`, not the current node's remaining `depth`. Result: a node with `depth = 2` cannot use a perfectly valid entry searched to depth 8 (`8 >= cutoff`) even though it is the strongest information available. The probable intent was "do not cut at nodes close to the root" (PV stability, avoiding truncated PVs and wrong scores from graph-history interaction), which is a property of the CURRENT node `depth` (or ply), not of the entry.
2. Replacement priority is `Depth * factor`, so the highest-depth entries (those with `entry.Depth >= CutoffDepth`) are protected the longest but can never produce a cutoff. They only supply the move ordering hint. Pure table pollution for cutoffs.
3. There is no PV/non-PV distinction. Exact entries return even in windows with `beta - alpha > 1`, which can truncate the PV. [VERIFY] how the PV is reconstructed (from the TT or from `context`). If it comes from the TT, an Exact cutoff in a PV node loses PV length.
4. The gate affects what the cutoff, depth and `entry.Depth >= depth` mean after extensions/reductions (entries store the modified depth, which is conservative for reductions and slightly optimistic for extensions).

[EXPERIMENT] Variants, each as a separate SPRT vs. baseline:
- A (baseline): current gate.
- B: gate on node depth, so a high-depth node (near the root) is never cut but any entry can cut low nodes:

```csharp
if (entry.Depth >= depth && depth < CutoffDepth)
```

- C: gate on window instead of depth (standard practice), with the cutoff disabled only at PV nodes:

```csharp
bool isPv = beta - alpha > 1;
if (entry.Depth >= depth && (!isPv || entry.Type != TranspositionEntryType.Exact))
```

  and keep an optional `depth < CutoffDepth` guard only for the root-adjacent plies.
- D: B + C combined.

Also consider a mate-score guard: do not cut on `|value| >= mate threshold` unless the ply normalization (`FromTTValue`) is verified symmetric with `ToTTValue`.

Expected: B/C usually gain strength at depth ~10 because high-depth entries finally produce cutoffs. Risk: PV instability near the root; check root move changes on the baseline positions.

---

## Step 5. Futility-pruned stores (`FutilitySearchInternal*`)

[VERIFY/EXPERIMENT] A node that pruned moves by margin searched a subset. A fail-low result (UpperBound, `value <= alpha`) is only "true" under the assumption that the pruned moves could not have beaten alpha. If that is wrong, a later probe with a LOWER alpha cuts on an upper bound that is false. The error is heuristic and bounded by the margin.

Options (A/B tested):
- A (baseline): store as today.
- B: store with `depth - 1` when at least one move was pruned (reduced trust), keeping Exact/LowerBound unchanged.
- C: do not store UpperBound from nodes that pruned any move.

```csharp
// needs a flag set by the futility pruning in the move loop, e.g. context.HasPrunedMoves
sbyte storeDepth = (context.HasPrunedMoves && entryType == TranspositionEntryType.UpperBound)
	? (sbyte)(depth - 1) : depth;
if (storeDepth > 0) Table.SetWhite(new TranspositionEntry(storeDepth, ..., move, entryType));
```

Null reduction and extensions: stored depth is the modified depth. Accepted as conservative, with no change.

Risk: low, but a no-op without an SPRT. Do it last among the strength changes.

---

## Step 6. Memory layout: real 64-byte alignment, no bounds checks

[CONFIRMED] `Bucket[]` data begins at array offset 16 and is not 64-byte aligned. Each probe often touches two cache lines. Index shifting cannot fix this (elements are 64 bytes, so the misalignment is constant). Allocate raw bytes on the pinned object heap and align manually:

```csharp
private readonly byte[] _storage;     // pinned, never moves
private readonly int _alignOffset;
private readonly ulong _mask;

public unsafe TranspositionHashSet(int capacityMB, int depthFactor, int typeFactor, int replaceMargin = 0)
{
	// ... bucketCount computed as today (power of 2) ...
	_storage = GC.AllocateArray<byte>(checked(bucketCount * 64 + 64), pinned: true);   // zeroed
	nint addr = (nint)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(_storage));
	_alignOffset = (int)((64 - (addr & 63)) & 63);
	_mask = (ulong)(bucketCount - 1);
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
private ref Bucket GetBucket(ulong key)
{
	ref byte b = ref MemoryMarshal.GetArrayDataReference(_storage);
	return ref Unsafe.As<byte, Bucket>(ref Unsafe.Add(ref b, (nint)_alignOffset + (nint)((key & _mask) << 6)));
}

public void Clear() => Array.Clear(_storage);
```

Notes:
- Requires `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` in the Engine csproj.
- `Count`, `GetValue`, `Set`, `Clear` must use `GetBucket`; `Count` iterates `bucketCount` stored in a field.
- Pinned arrays above ~1 GB need `checked`; `capacityMB * 1024 * 1024` in the current ctor overflows `int` at 2048 MB, so use `long`/`nuint` math ([CONFIRMED] latent bug).
- Fix the stale comments ("6 slots", "perfectly aligned").
- Gate: node counts identical; measure NPS before and after.

Optional prefetch (after `MakeMove`, when `_board.Hash` is updated):

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public unsafe void Prefetch(ulong key)
{
	if (Sse.IsSupported)
		Sse.Prefetch0(Unsafe.AsPointer(ref GetBucket(key)));
}
```

Expose `TranspositionTable.PrefetchWhite()/PrefetchBlack()`, call at the child's side to move. Benefit is largest with big tables (>= 64 MB).

---

## Step 7. Merge White/Black tables (optional) [VERIFY first]

[CONFIRMED] Two sets, each with the full `capacityMB`, so memory is 2x the configured value. [VERIFY] whether the Zobrist hash already includes the side to move. If yes, a single table of `2 * capacityMB` shares load better (a side's entries are not wasted when the other side dominates), and `GetWhite/GetBlack` and `SetWhite/SetBlack` become one pair.
If the hash does not include the side, positions that differ only by side to move already collide logically; separate tables then are correct and this step must NOT be done.

Risk: medium. Gate: identical results with the side included in the key.

---

## Step 8. Statistics and cleanup

- [CONFIRMED] `Count` scans all buckets: O(N). `Size => Table.Count` ([VERIFY] callers). Replace by a sampled fill rate:

```csharp
public int Permill()
{
	int n = Math.Min(1000, _bucketCount), used = 0;
	for (int i = 0; i < n; i++) used += GetBucketByIndex(i).Count();
	return used * 1000 / (n * BucketSize);
}
```

- Remove `TranspositionTable.IsBlocked`, the empty `Resize(int)` and the unused `_default` re-allocation if no callers exist (check `StrategyBase.IsBlocked()` at line ~1477 and `DataPoolService.Resize`).
- [VERIFY] `_mateThreshold = Mate - capacity` (StrategyBase ~1461): confirm `ToTTValue/FromTTValue` normalize by ply symmetrically and that `Mate` fits in `short` (`Value` is a `short`, so `|Mate| <= 32767`). A silent overflow on `(short)ToTTValue(...)` corrupts mate scores.
- Thread safety: entries are 12 bytes, so writes are not atomic. If parallel search is ever added, use a lock-less XOR-check scheme or a lock per bucket.

---

## Recommended order

1. Step 0 baseline.
2. Step 1 (hash move for UpperBound). Small and safe.
3. Step 2 + 3 (priority, conditional replacement). SPRT.
4. Step 6 (alignment). Identical node count, NPS gain.
5. Step 4 (CutoffDepth variants B/C/D). SPRT per variant.
6. Step 5 (futility store policy). SPRT.
7. Step 7, Step 8 (cleanup, optional merge).

## Regression checklist per step
- Node count unchanged for pure performance steps (2 without type bonus, 6).
- Mate puzzles return the same distance-to-mate.
- No illegal move from a TT hash move (`PvMove` validated or generated from the legal list; 32-bit key false positive risk ~ 1 per 10^8 to 10^9 probes).
- Repetition/50-move draws still detected (CheckDraw runs before the probe; keep it that way).
- No Elo claim without match results.
