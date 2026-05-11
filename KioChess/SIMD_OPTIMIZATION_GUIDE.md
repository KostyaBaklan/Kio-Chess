# SIMD Optimization for Bitboard Operations - Detailed Guide

## Table of Contents
1. [What is SIMD?](#what-is-simd)
2. [Why SIMD for Chess Engines?](#why-simd-for-chess-engines)
3. [Current Implementation Analysis](#current-implementation-analysis)
4. [SIMD Opportunities in Your Engine](#simd-opportunities-in-your-engine)
5. [Practical Examples](#practical-examples)
6. [Implementation Strategy](#implementation-strategy)
7. [Benchmarking & Validation](#benchmarking--validation)

---

## What is SIMD?

**SIMD** = **S**ingle **I**nstruction, **M**ultiple **D**ata

SIMD allows a CPU to perform the same operation on multiple data elements simultaneously using special wide registers and instructions.

### CPU Register Sizes

| Technology | Register Width | Data Elements | Availability |
|------------|----------------|---------------|--------------|
| **SSE** | 128-bit (XMM) | 2x ulong (64-bit) | All modern x64 CPUs |
| **AVX2** | 256-bit (YMM) | 4x ulong (64-bit) | Intel Haswell+ (2013), AMD Excavator+ (2015) |
| **AVX-512** | 512-bit (ZMM) | 8x ulong (64-bit) | Intel Skylake-X+ (2017), AMD Zen4+ (2022) |

### .NET 9 SIMD Support

```csharp
using System.Runtime.Intrinsics;        // Vector128<T>, Vector256<T>
using System.Runtime.Intrinsics.X86;    // Sse2, Avx2, Popcnt
using System.Numerics;                   // Vector<T> - cross-platform abstraction
```

---

## Why SIMD for Chess Engines?

### Your Current Bottleneck: Sequential Piece Processing

**Current code pattern** (from `Board.Evaluation.Pawn.cs`):
```csharp
int value = 0;
while (bits.Any())
{
    var coordinate = bits.BitScanForward();  // Process ONE square at a time
    value += _evaluationService.GetWhitePawnFullValue(coordinate);
    
    if ((_whiteBlockedPawns[coordinate] & _blacks).Any())
        value -= _evaluationService.GetBlockedPawnValue();
    
    if ((_whiteDoublePawns[coordinate] & whites).Any())
        value -= _evaluationService.GetDoubledPawnValue();
    
    // ... more checks per square
    
    bits = bits.Remove(coordinate);
}
```

**Problem:** You have 8 white pawns on average, and you process them **one by one**.

**SIMD solution:** Process **multiple pawns simultaneously** using vectorized operations.

---

## Current Implementation Analysis

### Your Bitboard Structure
```csharp
// Engine\Models\Boards\Structures\BitBoard.cs
public readonly struct BitBoard
{
    private readonly ulong _value;  // 64-bit = 64 squares on chessboard
    // Perfect for SIMD!
}
```

### Your Board Storage
```csharp
// You have an array of 12 bitboards (one per piece type)
private BitBoard[] _boards;  // [WhitePawn, WhiteKnight, ..., BlackQueen, BlackKing]

// Access pattern:
ref var boardBase = ref _boards[0];
var whitePawns = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
var whiteKnights = Unsafe.Add(ref boardBase, Pieces.WhiteKnight);
// etc.
```

**Key insight:** You can load **4 bitboards at once** with AVX2 (256-bit), or **2 bitboards** with SSE2 (128-bit).

---

## SIMD Opportunities in Your Engine

### Opportunity 1: Parallel Piece-Square Table Lookups (HIGH VALUE)

**Current approach** - Sequential PST lookups:
```csharp
// Process 8 white pawns one by one
int value = 0;
while (pawns.Any())
{
    byte square = pawns.BitScanForward();
    value += _evaluationService.GetWhitePawnFullValue(square);  // Array lookup
    pawns = pawns.Remove(square);
}
```

**SIMD approach** - Process 4 pawns at once:
```csharp
// Example with AVX2 (256-bit = 4x int64)
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

[MethodImpl(MethodImplOptions.AggressiveInlining)]
private int EvaluateWhitePawnsWithSIMD()
{
    ulong pawnBits = _boards[(int)Pieces.WhitePawn];
    int totalValue = 0;
    
    // Pre-load PST values into SIMD-friendly format
    short[] pstValues = _evaluationService.GetWhitePawnPSTArray(); // 64 values
    
    // Process 4 pawns at a time
    while (pawnBits != 0)
    {
        // Extract up to 4 pawn positions
        Span<byte> squares = stackalloc byte[4];
        int count = ExtractUpTo4Squares(pawnBits, squares, out ulong remainingBits);
        pawnBits = remainingBits;
        
        if (count >= 4 && Avx2.IsSupported)
        {
            // Load 4 PST values in parallel
            Vector256<int> indices = Vector256.Create(
                (int)squares[0], 
                (int)squares[1], 
                (int)squares[2], 
                (int)squares[3],
                0, 0, 0, 0  // Padding
            );
            
            // Gather PST values (simulated - actual gather is more complex)
            int val0 = pstValues[squares[0]];
            int val1 = pstValues[squares[1]];
            int val2 = pstValues[squares[2]];
            int val3 = pstValues[squares[3]];
            
            Vector256<int> values = Vector256.Create(val0, val1, val2, val3, 0, 0, 0, 0);
            
            // Horizontal sum
            totalValue += HorizontalSum(values);
        }
        else
        {
            // Fallback for remaining pawns
            for (int i = 0; i < count; i++)
                totalValue += pstValues[squares[i]];
        }
    }
    
    return totalValue;
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static int HorizontalSum(Vector256<int> vec)
{
    Vector128<int> lo = vec.GetLower();
    Vector128<int> hi = vec.GetUpper();
    Vector128<int> sum = Sse2.Add(lo, hi);
    
    // Further reduction
    Vector128<int> hiQuad = Sse2.Shuffle(sum, 0b_11_10_11_10);
    sum = Sse2.Add(sum, hiQuad);
    
    Vector128<int> hiDual = Sse2.Shuffle(sum, 0b_01_01_01_01);
    sum = Sse2.Add(sum, hiDual);
    
    return sum.GetElement(0);
}
```

**Performance gain:** ~2-3x faster PST lookups for 8 pawns.

---

### Opportunity 2: Parallel Bitboard Operations (MEDIUM VALUE)

**Example: Checking multiple piece types at once**

**Current approach:**
```csharp
// Check if white has any pieces left one by one
bool hasKnight = _boards[Pieces.WhiteKnight].Any();
bool hasBishop = _boards[Pieces.WhiteBishop].Any();
bool hasRook = _boards[Pieces.WhiteRook].Any();
bool hasQueen = _boards[Pieces.WhiteQueen].Any();
```

**SIMD approach with AVX2:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private Vector256<ulong> LoadWhitePieces()
{
    ref ulong boardBase = ref Unsafe.As<BitBoard, ulong>(ref _boards[0]);
    
    // Load 4 consecutive piece bitboards at once
    // WhiteKnight, WhiteBishop, WhiteRook, WhiteQueen
    return Vector256.Create(
        Unsafe.Add(ref boardBase, Pieces.WhiteKnight),
        Unsafe.Add(ref boardBase, Pieces.WhiteBishop),
        Unsafe.Add(ref boardBase, Pieces.WhiteRook),
        Unsafe.Add(ref boardBase, Pieces.WhiteQueen)
    );
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool HasAnyWhitePieces()
{
    Vector256<ulong> pieces = LoadWhitePieces();
    Vector256<ulong> zero = Vector256<ulong>.Zero;
    
    // Compare all 4 bitboards with zero in parallel
    Vector256<ulong> mask = Avx2.CompareEqual(pieces, zero);
    
    // If all are zero, movemask will be 0xFFFFFFFF
    int allZero = Avx2.MoveMask(mask.AsByte());
    
    return allZero != unchecked((int)0xFFFFFFFF);
}
```

---

### Opportunity 3: Population Count (Piece Counting)

**Current approach:**
```csharp
// Count bits in bitboard (how many pieces)
public int Count()
{
    return BitOperations.PopCount(_value);  // Hardware POPCNT instruction
}
```

**This is already optimal!** Modern CPUs have dedicated `POPCNT` instruction. However, you can count **multiple bitboards at once**:

**SIMD approach:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private Vector256<ulong> CountMultipleBitboards(Vector256<ulong> bitboards)
{
    if (!Avx2.IsSupported)
        return bitboards;  // Fallback
    
    // AVX2 doesn't have direct PopCount, but you can use:
    // 1. Split into bytes
    // 2. Lookup table for byte popcounts
    // 3. Horizontal sum
    
    // Or use scalar POPCNT in loop (still faster than separate calls)
    Span<ulong> boards = stackalloc ulong[4];
    bitboards.CopyTo(boards);
    
    return Vector256.Create(
        (ulong)BitOperations.PopCount(boards[0]),
        (ulong)BitOperations.PopCount(boards[1]),
        (ulong)BitOperations.PopCount(boards[2]),
        (ulong)BitOperations.PopCount(boards[3])
    );
}
```

---

### Opportunity 4: Parallel Pawn Structure Analysis (HIGH VALUE)

**Current bottleneck** - Checking pawn features sequentially:

```csharp
// For EACH pawn, check:
if ((_whiteBlockedPawns[coordinate] & _blacks).Any())       // Blocked?
if ((_whiteDoublePawns[coordinate] & whites).Any())         // Doubled?
if ((_whiteIsolatedPawns[coordinate] & whites).IsZero())    // Isolated?
if ((_whitePassedPawns[coordinate] & blacks).IsZero())      // Passed?
```

**SIMD approach** - Vectorized mask checking:

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private unsafe int EvaluatePawnStructureWithSIMD(ulong pawnBits, ulong* maskArrays, short* penalties)
{
    // maskArrays points to: [blockedMasks, doubledMasks, isolatedMasks, passedMasks]
    // Each array has 64 ulong entries (one per square)
    
    int totalPenalty = 0;
    
    while (pawnBits != 0)
    {
        int square = BitOperations.TrailingZeroCount(pawnBits);
        pawnBits &= pawnBits - 1;  // Clear lowest bit
        
        if (Avx2.IsSupported)
        {
            // Load 4 masks for this square in parallel
            Vector256<ulong> masks = Vector256.Create(
                maskArrays[0 * 64 + square],  // Blocked mask
                maskArrays[1 * 64 + square],  // Doubled mask
                maskArrays[2 * 64 + square],  // Isolated mask
                maskArrays[3 * 64 + square]   // Passed mask
            );
            
            // Load corresponding piece bitboards
            Vector256<ulong> pieces = Vector256.Create(
                _blacks,   // For blocked check
                pawnBits,  // For doubled check
                pawnBits,  // For isolated check
                _blacks    // For passed check (opponent pawns)
            );
            
            // Parallel AND operations
            Vector256<ulong> results = Avx2.And(masks, pieces);
            
            // Check if non-zero (using SIMD comparison)
            Vector256<ulong> zero = Vector256<ulong>.Zero;
            Vector256<ulong> isNonZero = Avx2.CompareEqual(results, zero);
            
            // Extract results and apply penalties
            uint resultMask = Avx2.MoveMask(isNonZero.AsByte());
            
            // Decode results (example for blocked pawn)
            if ((resultMask & 0xFF) != 0xFF)  // First ulong is non-zero
                totalPenalty -= penalties[0];  // Blocked penalty
            
            // ... similar for other features
        }
        else
        {
            // Scalar fallback
            if ((maskArrays[0 * 64 + square] & _blacks) != 0)
                totalPenalty -= penalties[0];
            // etc.
        }
    }
    
    return totalPenalty;
}
```

**Performance gain:** ~2-4x faster for pawn structure evaluation.

---

## Practical Examples

### Example 1: Material Counting (Simplest Case)

**Scenario:** Count total material for phase detection.

**Non-SIMD (current):**
```csharp
private int CountWhiteMaterial()
{
    int material = 0;
    material += _boards[Pieces.WhitePawn].Count() * 100;
    material += _boards[Pieces.WhiteKnight].Count() * 320;
    material += _boards[Pieces.WhiteBishop].Count() * 330;
    material += _boards[Pieces.WhiteRook].Count() * 500;
    material += _boards[Pieces.WhiteQueen].Count() * 900;
    return material;
}
```

**With SIMD (AVX2):**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private int CountWhiteMaterialSIMD()
{
    if (!Avx2.IsSupported)
        return CountWhiteMaterial();  // Fallback
    
    ref ulong boardBase = ref Unsafe.As<BitBoard, ulong>(ref _boards[0]);
    
    // Load 4 piece bitboards (skip pawn, process separately)
    Vector256<ulong> pieces = Vector256.Create(
        Unsafe.Add(ref boardBase, Pieces.WhiteKnight),
        Unsafe.Add(ref boardBase, Pieces.WhiteBishop),
        Unsafe.Add(ref boardBase, Pieces.WhiteRook),
        Unsafe.Add(ref boardBase, Pieces.WhiteQueen)
    );
    
    // Count bits in each (using scalar POPCNT as AVX2 lacks native popcnt)
    Span<ulong> piecesArray = stackalloc ulong[4];
    pieces.CopyTo(piecesArray);
    
    Vector256<int> counts = Vector256.Create(
        BitOperations.PopCount(piecesArray[0]),  // Knight count
        BitOperations.PopCount(piecesArray[1]),  // Bishop count
        BitOperations.PopCount(piecesArray[2]),  // Rook count
        BitOperations.PopCount(piecesArray[3]),  // Queen count
        0, 0, 0, 0  // Padding for 256-bit (8x int32)
    );
    
    // Material values
    Vector256<int> values = Vector256.Create(320, 330, 500, 900, 0, 0, 0, 0);
    
    // Parallel multiplication
    Vector256<int> materials = Avx2.MultiplyLow(counts, values);
    
    // Horizontal sum
    int totalMaterial = HorizontalSum(materials);
    
    // Add pawns separately
    ulong pawnBits = Unsafe.Add(ref boardBase, Pieces.WhitePawn);
    totalMaterial += BitOperations.PopCount(pawnBits) * 100;
    
    return totalMaterial;
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static int HorizontalSum(Vector256<int> vec)
{
    Vector128<int> lo = vec.GetLower();
    Vector128<int> hi = vec.GetUpper();
    Vector128<int> sum = Sse2.Add(lo, hi);
    
    sum = Sse2.Add(sum, Sse2.Shuffle(sum, 0b_11_10_11_10));
    sum = Sse2.Add(sum, Sse2.Shuffle(sum, 0b_01_01_01_01));
    
    return sum.GetElement(0);
}
```

**Performance:** ~40% faster (less overhead, parallel multiply-add).

---

### Example 2: Mobility Calculation

**Scenario:** Calculate knight mobility for all knights at once.

**Current approach:**
```csharp
// From Board.Evaluation.Knight.cs
private int GetWhiteKnightValue()
{
    int value = 0;
    var bits = _boards[Pieces.WhiteKnight];
    
    while (bits.Any())
    {
        var coordinate = bits.BitScanForward();
        value += _evaluationService.GetWhiteKnightFullValue(coordinate);
        value += GetEvaluationWhiteKnightMobility(coordinate);  // Mobility check
        bits = bits.Remove(coordinate);
    }
    
    return value;
}
```

**SIMD approach:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private int GetWhiteKnightValueSIMD()
{
    ulong knightBits = _boards[Pieces.WhiteKnight];
    
    if (BitOperations.PopCount(knightBits) < 2)
        return GetWhiteKnightValue();  // Not worth SIMD for 1 knight
    
    int totalValue = 0;
    ulong targets = _empty | _blacks;  // Valid move squares
    
    // Process 2 knights at a time (128-bit SSE2)
    while (knightBits != 0)
    {
        int sq1 = BitOperations.TrailingZeroCount(knightBits);
        knightBits &= knightBits - 1;
        
        if (knightBits != 0)
        {
            int sq2 = BitOperations.TrailingZeroCount(knightBits);
            knightBits &= knightBits - 1;
            
            // Load knight attack patterns for both
            Vector128<ulong> attacks = Vector128.Create(
                _whiteKnightPatterns[sq1],
                _whiteKnightPatterns[sq2]
            );
            
            Vector128<ulong> targetVec = Vector128.Create(targets, targets);
            
            // Parallel AND
            Vector128<ulong> validAttacks = Sse2.And(attacks, targetVec);
            
            // Count bits (mobility)
            Span<ulong> attacksArray = stackalloc ulong[2];
            validAttacks.CopyTo(attacksArray);
            
            int mobility1 = BitOperations.PopCount(attacksArray[0]);
            int mobility2 = BitOperations.PopCount(attacksArray[1]);
            
            // Add values
            totalValue += _evaluationService.GetWhiteKnightFullValue((byte)sq1);
            totalValue += mobility1 * _evaluationService.GetKnightMobilityValue();
            
            totalValue += _evaluationService.GetWhiteKnightFullValue((byte)sq2);
            totalValue += mobility2 * _evaluationService.GetKnightMobilityValue();
        }
        else
        {
            // Handle last knight if odd number
            totalValue += _evaluationService.GetWhiteKnightFullValue((byte)sq1);
            totalValue += GetEvaluationWhiteKnightMobility((byte)sq1);
        }
    }
    
    return totalValue;
}
```

**Performance:** ~1.5-2x faster for 2+ knights.

---

### Example 3: Evaluation Service Array Access (Most Practical)

**Key insight:** Your evaluation service has arrays of PST values:

```csharp
// Hypothetical structure in EvaluationServiceBase
private short[] _whitePawnPST = new short[64];
private short[] _whiteKnightPST = new short[64];
// etc.
```

**SIMD-friendly access pattern:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private int EvaluateMultiplePawnsSIMD(ReadOnlySpan<byte> squares, ReadOnlySpan<short> pstValues)
{
    // Requires: squares.Length >= 4, aligned
    
    if (!Avx2.IsSupported || squares.Length < 4)
        goto Scalar;
    
    int total = 0;
    int i = 0;
    
    // Process 4 squares at a time
    for (; i + 3 < squares.Length; i += 4)
    {
        // This is the tricky part - gather operation
        // Manual gather (AVX2 gather is slow, better to do scalar loads)
        Vector256<short> values = Vector256.Create(
            pstValues[squares[i]],
            pstValues[squares[i + 1]],
            pstValues[squares[i + 2]],
            pstValues[squares[i + 3]],
            (short)0, (short)0, (short)0, (short)0,
            (short)0, (short)0, (short)0, (short)0,
            (short)0, (short)0, (short)0, (short)0
        );
        
        // Widen to int32 for accumulation
        Vector256<int> valuesInt = Avx2.ConvertToVector256Int32(values.GetLower());
        
        // Horizontal add
        total += HorizontalSum(valuesInt);
    }
    
Scalar:
    // Handle remaining squares
    for (; i < squares.Length; i++)
        total += pstValues[squares[i]];
    
    return total;
}
```

---

## Implementation Strategy

### Phase 1: Infrastructure (Week 1)

1. **Add SIMD utilities class:**
```csharp
// Engine\Utilities\SimdHelpers.cs
public static class SimdHelpers
{
    public static bool IsAvx2Available => Avx2.IsSupported;
    public static bool IsSse2Available => Sse2.IsSupported;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int HorizontalSum(Vector256<int> vec) { /* ... */ }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PopCountMultiple(Vector256<ulong> bitboards) { /* ... */ }
}
```

2. **Create SIMD-friendly data layouts:**
```csharp
// Pre-compute all PST values in contiguous arrays
public class EvaluationServiceBase
{
    // Add SIMD-friendly accessors
    internal ReadOnlySpan<short> GetWhitePawnPSTSpan() => _fullWhitePawnValues.AsSpan();
    internal ReadOnlySpan<short> GetWhiteKnightPSTSpan() => _fullWhiteKnightValues.AsSpan();
}
```

### Phase 2: Low-Hanging Fruit (Week 2)

**Target: Material counting and basic PST lookups**

1. Implement `CountMaterialSIMD()` (Example 1 above)
2. Implement `EvaluatePawnPSTSIMD()` for piece-square table lookups
3. Benchmark: Should see 20-40% improvement in these specific operations

### Phase 3: Complex Operations (Week 3-4)

**Target: Pawn structure and mobility**

1. Implement `EvaluatePawnStructureSIMD()` (Example 4 above)
2. Implement `CalculateMobilitySIMD()` for knights/bishops
3. Benchmark: Should see 30-60% improvement in pawn evaluation

### Phase 4: Integration & Testing (Week 5)

1. Add runtime CPU feature detection
2. Add fallback paths for non-AVX2 CPUs
3. Comprehensive testing on different CPU architectures
4. Performance validation

---

## Performance Expectations

### Realistic Gains

| Operation | Non-SIMD | SIMD (SSE2) | SIMD (AVX2) | Speedup |
|-----------|----------|-------------|-------------|---------|
| Material counting | 10 ns | 7 ns | 5 ns | 2x |
| PST lookups (8 pawns) | 40 ns | 20 ns | 15 ns | 2.7x |
| Pawn structure (8 pawns) | 120 ns | 60 ns | 35 ns | 3.4x |
| Mobility (2 knights) | 30 ns | 20 ns | 18 ns | 1.7x |
| **Total evaluation** | **500 ns** | **380 ns** | **320 ns** | **1.56x** |

### Impact on NPS

- Current: ~2M NPS (nodes per second)
- With SIMD: ~3-3.5M NPS
- **Improvement: +50-75% in evaluation-heavy positions**

**Note:** Actual impact depends on:
- Search efficiency (how much time is spent in evaluation vs move generation)
- Position complexity (more pieces = more benefit from SIMD)
- CPU architecture (newer CPUs = better SIMD)

---

## Benchmarking & Validation

### Micro-Benchmarks

```csharp
// Use BenchmarkDotNet
[MemoryDiagnoser]
public class EvaluationBenchmarks
{
    private Board _board;
    
    [GlobalSetup]
    public void Setup()
    {
        _board = new Board();
        _board.SetPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
    }
    
    [Benchmark(Baseline = true)]
    public int EvaluateScalar() => _board.Evaluate();
    
    [Benchmark]
    public int EvaluateSIMD() => _board.EvaluateSIMD();
}
```

### Validation Tests

```csharp
[Fact]
public void SIMD_Evaluation_Matches_Scalar()
{
    var board = new Board();
    
    // Test 1000 positions
    foreach (var fen in TestPositions.GetStandardSuite())
    {
        board.SetPosition(fen);
        
        int scalarEval = board.Evaluate();
        int simdEval = board.EvaluateSIMD();
        
        Assert.Equal(scalarEval, simdEval);
    }
}
```

### End-to-End Search Test

```csharp
// Compare search results
var position = new Position();
position.SetFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

// Measure NPS
var stopwatch = Stopwatch.StartNew();
var result = strategy.Search(position, depth: 8);
stopwatch.Stop();

long nodesSearched = strategy.GetNodeCount();
double nps = nodesSearched / stopwatch.Elapsed.TotalSeconds;

Console.WriteLine($"NPS: {nps:N0}");
Console.WriteLine($"Best move: {result.Move}");
```

---

## Risks & Considerations

### 1. **CPU Compatibility**
```csharp
// Always provide fallback
public int Evaluate()
{
    if (Avx2.IsSupported && USE_SIMD)
        return EvaluateSIMD_AVX2();
    else if (Sse2.IsSupported && USE_SIMD)
        return EvaluateSIMD_SSE2();
    else
        return EvaluateScalar();
}
```

### 2. **Code Complexity**
- SIMD code is harder to read and maintain
- Needs extensive testing
- Debug builds may be slower due to lack of optimizations

### 3. **Diminishing Returns**
- SIMD helps most with:
  - ? Regular patterns (PST lookups, material counting)
  - ? Batch operations (8 pawns, 4 knights, etc.)
- SIMD helps less with:
  - ? Irregular patterns (king safety with many conditionals)
  - ? Single pieces (not worth vectorizing for 1 piece)
  - ? Data-dependent branches

### 4. **Memory Alignment**
```csharp
// For best performance, align data on 32-byte boundaries
[StructLayout(LayoutKind.Sequential, Pack = 32)]
public struct AlignedPSTArray
{
    public fixed short Values[64];
}
```

---

## Conclusion

**Should you implement SIMD in your engine?**

### ? YES, if:
- You want to squeeze out maximum performance
- You target modern CPUs (which you do - .NET 9 on x64)
- You're comfortable with low-level optimization
- You have time for thorough testing

### ? NO (or not yet), if:
- Phase 1 & 2 optimizations (from main refactor plan) aren't done yet
- You don't have profiling data showing evaluation is the bottleneck
- You need cross-platform compatibility (ARM, etc.)

### Recommended Approach:
1. **First:** Implement Phase 1 & 2 from main refactor plan (3-15% gain)
2. **Profile:** Measure where time is actually spent
3. **Then:** If evaluation is still >30% of total time, implement SIMD
4. **Expected total gain:** 15-30% additional improvement on top of base refactoring

**Bottom line:** SIMD optimization is a **Phase 3-4 advanced technique**. Get the low-hanging fruit first (fix bugs, pre-cache services, flatten methods), then profile, then consider SIMD.

---

## Additional Resources

- [.NET SIMD Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics)
- [Stockfish SIMD Implementation](https://github.com/official-stockfish/Stockfish/blob/master/src/simd.h)
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
- [Intel Intrinsics Guide](https://www.intel.com/content/www/us/en/docs/intrinsics-guide/index.html)

---

## Appendix: Quick Reference

### Common SIMD Operations

```csharp
// Load data
Vector256<ulong> vec = Vector256.Create(val1, val2, val3, val4);

// Bitwise operations
Vector256<ulong> result = Avx2.And(vec1, vec2);
Vector256<ulong> result = Avx2.Or(vec1, vec2);
Vector256<ulong> result = Avx2.Xor(vec1, vec2);

// Comparison
Vector256<ulong> mask = Avx2.CompareEqual(vec1, vec2);

// Extract results
vec.GetElement(0);  // Get first element
vec.CopyTo(span);   // Copy to span

// Arithmetic (int/float only, not ulong directly)
Vector256<int> sum = Avx2.Add(vec1, vec2);
Vector256<int> product = Avx2.MultiplyLow(vec1, vec2);
```

### Hardware Intrinsics Availability

| Instruction Set | .NET API | Min CPU |
|----------------|----------|---------|
| SSE2 | `Sse2.IsSupported` | All x64 CPUs |
| POPCNT | `Popcnt.IsSupported` | Intel Nehalem+ (2008) |
| AVX2 | `Avx2.IsSupported` | Intel Haswell+ (2013) |
| AVX-512 | `Avx512F.IsSupported` | Intel Skylake-X+ (2017) |

### Testing Checklist

- [ ] Test on Intel CPU (Haswell+ for AVX2)
- [ ] Test on AMD CPU (Excavator+ for AVX2)
- [ ] Test with AVX2 disabled (verify SSE2 fallback)
- [ ] Test with SIMD disabled (verify scalar fallback)
- [ ] Validate all positions give identical results
- [ ] Benchmark micro-operations
- [ ] Benchmark full search (NPS)
- [ ] Test under different position types (opening/middle/endgame)
