# Board Evaluation Performance Analysis & Refactor Plan

## Executive Summary

The `Board.Evaluation.cs` system is a critical hot path in the chess engine, being called millions of times per second during search (in both main search and quiescence search). This document analyzes current performance bottlenecks and provides a comprehensive refactor plan.

---

## Current Architecture Analysis

### Call Flow
```
StrategyBase.EvaluateWhite/Black() 
    ? Board.Evaluate()
        ? ComputeAttacks()
        ? _moveHistory.GetPhase()
        ? _evaluationServiceFactory.GetEvaluationService(phase)
        ? EvaluateMiddle/End/Opening()
            ? EvaluateWhite[Piece][Phase]() - EvaluateBlack[Piece][Phase]()
                ? Multiple piece-specific evaluation methods
                    ? Mobility calculations
                    ? Positional scoring
                    ? Pawn structure analysis
```

### Key Observations

#### 1. **Redundant Phase Detection** (MEDIUM IMPACT - Updated Analysis)
**Current Issue:**
- `GetPhase()` is called on **every evaluation** 
- Phase is retrieved via array lookup: `return _phases[_ply];`
- The phase value itself is correct and must be retrieved each time
- **The bottleneck is not the phase lookup, but what we do with it**

**Actual Performance Impact:**
- Array access is fast (~1-2 cycles)
- The real cost is the **factory lookup** for evaluation service
- **Reassigning `_evaluationService`** on every call
- **Branch misprediction** on 3-way phase comparison

**Updated Understanding:**
- Phase is stored per-ply and is correctly updated on each move
- Within the same ply, phase is constant but we can't safely cache it
- The phase lookup itself is not the bottleneck
- The **evaluation service factory lookup** is unnecessary

**Evidence from code:**
```csharp
public int Evaluate()
{
    ComputeAttacks();
    var phase = _moveHistory.GetPhase();  // ? Called every evaluation
    _evaluationService = _evaluationServiceFactory.GetEvaluationService(phase);
    return phase == Phase.Middle ? EvaluateMiddle() : 
           phase == Phase.End ? EvaluateEnd() : 
           EvaluateOpening();
}
```

#### 2. **Evaluation Service Lookup** (MEDIUM IMPACT)
**Current Issue:**
- `_evaluationService` is set on **every** `Evaluate()` call
- Factory lookup via array index: `_evaluationServices[phase]`
- Field is written but phase rarely changes

**Performance Impact:**
- Unnecessary memory writes
- Cache line pollution
- Potential branch mispredictions on phase checks

#### 3. **Method Call Overhead** (MEDIUM-HIGH IMPACT)
**Current Issue:**
- Deep call chains: `EvaluateWhiteOpening()` ? `EvaluateWhitePawnOpening()` ? evaluation logic
- Separate methods for each piece/phase combination (36+ methods)
- Each adds stack frame overhead despite `AggressiveInlining`

**Evidence:**
- White has 6 pieces × 3 phases = 18 evaluation methods
- Black has 6 pieces × 3 phases = 18 evaluation methods
- Each phase evaluator calls 6+ sub-methods

#### 4. **Duplicate Code Between White/Black** (LOW-MEDIUM IMPACT)
**Current Issue:**
- Near-identical logic for white and black evaluation
- Only differences: piece indices and sign of result
- Code duplication makes maintenance harder

**Example:**
```csharp
private int EvaluateWhiteOpening() { /* evaluates white pieces */ }
private int EvaluateBlackOpening() { /* nearly identical for black */ }
```

#### 5. **Conditional Piece Existence Checks** (LOW IMPACT)
**Current Issue:**
- Each piece type checked individually: `if (board[Pieces.WhiteKnight].Any())`
- Multiple branches in evaluation flow

**Performance Impact:**
- Branch prediction overhead
- Modern CPUs handle these well with branch prediction

#### 6. **Incorrect Rook Evaluation Call in Middle Game** (BUG)
**Found Issues:**
```csharp
// In EvaluateWhiteMiddle() - Line 87
value += EvaluateWhiteRookOpening();  // ? Should be EvaluateWhiteRookMiddle()

// In EvaluateBlackMiddle() - Line 145
value += EvaluateBlackRookOpening();  // ? Should be EvaluateBlackRookMiddle()
```

---

## Performance Bottleneck Summary

### Critical Path Analysis (ordered by impact)

| Issue | Severity | Frequency | Impact | Fix Difficulty |
|-------|----------|-----------|--------|----------------|
| Evaluation service factory lookup | **MEDIUM** | Every eval | 2-4% | Trivial |
| Deep method call chains | **MEDIUM-HIGH** | Every eval | 3-8% | Medium |
| Rook evaluation bug | **HIGH** | Middle game | Correctness | Trivial |
| Redundant service field assignment | **LOW** | Every eval | 1-2% | Trivial |
| White/Black duplication | **LOW** | N/A | Maintenance | Hard |
| Conditional checks | **LOW** | Every eval | <2% | Not worth it |

### Estimated Call Frequency
- **Quiescence Search**: 80-90% of all evaluations
- **Regular Search**: 10-20% of evaluations
- **Total evaluations**: Millions per second in deep searches
- **Phase changes during search**: Frequent (every capture of major piece can change phase)
- **Phase retrieval**: Every evaluation (necessary and correct)

---

## Refactor Plan

### Important Clarification: Why Simple Phase Caching Won't Work

**Original flawed idea:** Cache phase by ply to avoid repeated lookups.

**Why it's wrong:**
```
Position A (ply 50): 2 Queens, 4 Rooks ? Middle Game
    ?? Try Move 1: Qxc6 (capture queen)
    ?   ?? Position B (ply 51): 1 Queen, 4 Rooks ? END GAME (phase changed!)
    ?       ?? Evaluate() at ply 51 ? needs END GAME evaluation
    ?
    ?? Unmake Move 1, Try Move 2: Kh1 (quiet)
        ?? Position C (ply 51): 2 Queens, 4 Rooks ? MIDDLE GAME  
            ?? Evaluate() at ply 51 ? needs MIDDLE GAME evaluation
```

**Key insight:** 
- Both Position B and Position C are at **ply 51**
- They have **different piece counts** ? **different phases**
- Caching by ply would be incorrect
- The phase array `_phases[_ply]` is updated by `SetPhase()` when each move is made
- This is why the current design stores phase per-ply and updates it on move make

**What we CAN optimize:**
- Pre-cache the 3 evaluation service objects (they never change)
- Eliminate factory array lookup on every evaluation
- Keep calling `GetPhase()` every time (this is necessary and correct)

### Phase 1: Quick Wins (Immediate, ~3-6% improvement)

#### 1.1 Optimize Phase and Service Lookup
**Goal**: Reduce redundant operations without incorrect caching

**Problem Analysis:**
The original plan to cache by ply was **incorrect** because:
- Phase is stored per-ply in `MoveHistoryService._phases[_ply]`
- Phase is set when moves are made via `SetPhase()` ? `_board.IsEndGame()`
- **Within the same ply**, the board state is CONSTANT during evaluation
- Multiple evaluations at the same ply will always return the same phase
- However, we cannot cache across moves at the same ply level in the move history

**The Real Issue:**
On every `Evaluate()` call, we:
1. Call `_moveHistory.GetPhase()` - array lookup with bounds check
2. Call `_evaluationServiceFactory.GetEvaluationService(phase)` - another array lookup
3. Assign to `_evaluationService` field - memory write
4. Branch on phase value (3-way comparison)

**Better Approach - Inline Everything:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public int Evaluate()
{
    ComputeAttacks();
    
    var phase = _moveHistory.GetPhase();
    
    // Use switch expression for better codegen
    return phase switch
    {
        Phase.Opening => EvaluateOpeningDirect(),
        Phase.Middle => EvaluateMiddleDirect(),
        Phase.End => EvaluateEndDirect(),
        _ => EvaluateMiddleDirect() // Default to middle
    };
}

// Direct evaluation methods that get service inline
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private int EvaluateOpeningDirect()
{
    _evaluationService = _evaluationServiceFactory.GetEvaluationService(Phase.Opening);
    return EvaluateWhiteOpening() - EvaluateBlackOpening();
}
```

**Alternative - Pre-cache services per phase (Safe):**
Since there are only 3 phases and services are immutable:
```csharp
public partial class Board
{
    private EvaluationServiceBase _openingService;
    private EvaluationServiceBase _middleService;
    private EvaluationServiceBase _endService;
    
    // Initialize once in constructor
    private void InitializeEvaluationServices()
    {
        _openingService = _evaluationServiceFactory.GetEvaluationService(Phase.Opening);
        _middleService = _evaluationServiceFactory.GetEvaluationService(Phase.Middle);
        _endService = _evaluationServiceFactory.GetEvaluationService(Phase.End);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Evaluate()
    {
        ComputeAttacks();
        
        return _moveHistory.GetPhase() switch
        {
            Phase.Opening => EvaluateOpeningWith(_openingService),
            Phase.Middle => EvaluateMiddleWith(_middleService),
            Phase.End => EvaluateEndWith(_endService),
            _ => EvaluateMiddleWith(_middleService)
        };
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int EvaluateOpeningWith(EvaluationServiceBase service)
    {
        _evaluationService = service;
        return EvaluateWhiteOpening() - EvaluateBlackOpening();
    }
}
```

**Benefits:**
- Eliminates factory lookup on every evaluation
- Pre-cached services are guaranteed correct
- Still calls `GetPhase()` every time (correct behavior)
- Simple, safe change

**Risks:**
- Minimal - services are immutable and phase-specific

#### 1.2 Fix Rook Evaluation Bug
**Goal**: Correct middle game rook evaluation

**Files to modify:**
- `Board.Evaluation.cs` lines 87, 145

**Change:**
```csharp
// Line 87 - EvaluateWhiteMiddle()
- value += EvaluateWhiteRookOpening();
+ value += EvaluateWhiteRookMiddle();

// Line 145 - EvaluateBlackMiddle()
- value += EvaluateBlackRookOpening();
+ value += EvaluateBlackRookMiddle();
```

---

### Phase 2: Method Inlining & Simplification (Medium effort, ~8-15% improvement)

#### 2.1 Flatten Evaluation Call Hierarchy
**Goal**: Reduce method call overhead

**Current structure:**
```
Evaluate() 
  ? EvaluateMiddle()
    ? EvaluateWhiteMiddle()
      ? EvaluateWhitePawnMiddle()
      ? EvaluateWhiteKnightMiddle()
      ? ... (6 methods)
    ? EvaluateBlackMiddle()
      ? EvaluateBlackPawnMiddle()
      ? ... (6 methods)
```

**Proposed structure:**
```
Evaluate()
  ? EvaluateMiddle()  // Inline all piece logic directly
```

**Implementation approach:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private int EvaluateMiddle()
{
    int whiteValue = 0;
    int blackValue = 0;
    ref var boardBase = ref _boards[0];
    
    // White Pawns - inline logic
    whiteValue += EvaluateWhitePawnsInline(ref boardBase);
    whiteValue += EvaluateWhiteKingMiddle();
    
    // Check and evaluate other white pieces
    if (Unsafe.Add(ref boardBase, Pieces.WhiteKnight).Any())
        whiteValue += EvaluateWhiteKnightsInline(ref boardBase);
    
    // ... similar for all pieces ...
    
    return whiteValue - blackValue;
}
```

**Benefits:**
- Reduces call stack depth
- Better instruction cache utilization
- Compiler can better optimize across piece evaluations

**Risks:**
- Longer methods (harder to read)
- May exceed inline size limits (monitor generated IL)

#### 2.2 Consider Template-Based Approach
**Goal**: Eliminate white/black duplication

**Option A: Generic helper methods**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private int EvaluatePawnsGeneric<TColor>(ref BitBoard boardBase) 
    where TColor : struct, IColor
{
    // Use TColor.IsWhite to branch at compile-time
    // Single implementation for both colors
}
```

**Option B: Keep separate but share implementation patterns**
- Lower risk, easier maintenance
- Recommended approach for now

---

### Phase 3: Algorithm Optimizations (Higher effort, ~5-10% improvement)

#### 3.1 Incremental Evaluation
**Goal**: Only recalculate changed evaluations

**Concept:**
- Cache piece-square table values
- Update incrementally on make/unmake
- Similar to how Stockfish does it

**Complexity:** HIGH
**Benefit:** Significant for material/PST, less for dynamic features
**Recommendation:** Consider for future optimization

#### 3.2 Lazy Evaluation
**Goal**: Don't evaluate everything if not needed

**Implementation ideas:**
- For quiescence: skip some expensive computations (mobility, king safety)
- Separate `EvaluateFast()` for qsearch vs `EvaluateFull()` for main search
- Progressive evaluation: compute more only if needed

**Example:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
protected int EvaluateWhite(int alpha, int beta)
{
    int standPat = _board.EvaluateFast();  // Material + PST only
    
    if (standPat >= beta)
        return beta;
    
    if (standPat + LAZY_MARGIN < alpha)
        return alpha;
    
    // Only do full eval if needed
    return _board.EvaluateFull();
}
```

#### 3.3 SIMD Optimization for Bitboard Operations
**Goal**: Vectorize multiple piece evaluations

**Approach:**
- Use `System.Runtime.Intrinsics` (AVX2/SSE)
- Process multiple squares simultaneously
- Most beneficial for pawn evaluation

**Complexity:** VERY HIGH
**Benefit:** 10-30% for specific operations
**Recommendation:** Profile first, optimize bottlenecks only

---

### Phase 4: Structural Refactoring (Long-term)

#### 4.1 Separate Fast and Full Evaluation Paths
```csharp
// For quiescence search (95% of calls)
public int EvaluateFast()
{
    // Material + PST only
    // No mobility, king safety
}

// For regular search
public int EvaluateFull()
{
    // Complete evaluation
}
```

#### 4.2 Evaluation Term Accumulator
**Pattern from Stockfish:**
```csharp
private struct EvalAccumulator
{
    public int Material;
    public int PST;
    public int Mobility;
    public int PawnStructure;
    public int KingSafety;
    
    public int Total => Material + PST + Mobility + PawnStructure + KingSafety;
}
```

**Benefits:**
- Can disable expensive terms selectively
- Better for debugging
- Easier to tune

---

## Benchmarking Strategy

### Metrics to Track
1. **Nodes per second (NPS)** - Primary metric
2. **Average evaluation time** - Per evaluation cost
3. **Cache hit rates** - For phase caching
4. **Search depth reached** - Fixed time control
5. **Quality of moves** - Must not regress

### Test Positions
Use standard benchmark positions:
- Initial position
- Middle game positions
- Endgame positions
- Tactical positions (high quiescence activity)

### Benchmark Command
```bash
# Before changes
dotnet run --project BenchmarkTool -c Release -- benchmark

# After each optimization
dotnet run --project BenchmarkTool -c Release -- benchmark
```

### Success Criteria
- **Phase 1**: +3-6% NPS improvement, no move quality regression
- **Phase 2**: +8-15% additional NPS improvement
- **Phase 3**: +5-10% additional NPS improvement
- **Overall Target**: +15-30% total NPS improvement

---

## Implementation Recommendations

### Priority Order
1. ? **Fix rook evaluation bug** (5 minutes, critical correctness issue)
2. ? **Pre-cache evaluation services** (30 minutes, low-medium impact, very low risk)
3. ? **Benchmark and validate** (1 hour)
4. ? **Flatten method hierarchy** (4-8 hours, medium-high impact)
5. ? **Benchmark and validate** (2 hours)
6. ?? **Consider lazy evaluation** (Future work)
7. ?? **Consider incremental evaluation** (Major refactor)

### Testing Strategy
- ? Unit tests for each piece evaluation
- ? Regression tests comparing evaluations before/after
- ? Perft tests to ensure move generation unchanged
- ? Play test games: old vs new engine
- ? Tactical test suite (WAC, etc.)

### Risk Mitigation
- Make changes incrementally
- Benchmark after each change
- Keep old code commented for comparison
- Use version control branches
- Validate with test suite before merging

---

## Additional Observations

### Code Quality Issues
1. **Inconsistent naming**: Some methods use `Evaluate[Piece][Phase]`, others `Get[Piece]Value`
2. **Magic numbers**: Many hardcoded values throughout evaluation
3. **Limited documentation**: Complex evaluation logic lacks comments
4. **Test coverage**: Unknown if comprehensive evaluation tests exist

### Future Considerations
1. **Neural network evaluation**: Modern engines use NNUE
2. **Tuning framework**: Automatic evaluation weight optimization
3. **Evaluation cache**: Hash table for evaluated positions
4. **Multi-threading**: Shared evaluation data structures

---

## Conclusion

The current evaluation system has several optimization opportunities:

**Quick wins** (Phase 1):
- Fix critical bug in rook evaluation
- Pre-cache evaluation services (eliminate factory lookups)
- ~3-6% improvement with minimal risk

**Medium-term** (Phase 2):
- Flatten call hierarchy
- Reduce method call overhead
- ~8-15% additional improvement

**Long-term** (Phase 3-4):
- Lazy evaluation
- Incremental evaluation
- Separate fast/full paths
- ~5-10% additional improvement

**Total potential improvement: 15-30% in nodes per second**

The evaluation is already well-optimized with:
- Aggressive inlining
- Unsafe pointer arithmetic
- Bitboard operations
- Skip locals init

The main bottlenecks are architectural (redundant lookups, deep call chains) rather than algorithmic. The proposed refactoring focuses on reducing overhead while maintaining code correctness and readability.

---

## Appendix: File Reference

### Files Analyzed
- `Engine\Models\Boards\Evaluation\Board.Evaluation.cs` - Main evaluation orchestration
- `Engine\Models\Boards\Evaluation\Board.Evaluation.Pawn.cs` - Pawn evaluation
- `Engine\Models\Boards\Evaluation\Board.Evaluation.Knight.cs` - Knight evaluation
- `Engine\Models\Boards\Board.Mobility.cs` - Mobility calculations
- `Engine\Services\MoveHistoryService.cs` - Phase tracking
- `Engine\Services\Evaluation\EvaluationServiceFactory.cs` - Service factory
- `Engine\Strategies\Base\StrategyBase.cs` - Search integration

### Related Systems
- **Attack computation**: Called before each evaluation
- **Mobility calculation**: Integrated into piece evaluation
- **Position evaluation**: Used in search cutoffs
- **Transposition table**: Caches position values, not evaluation components
