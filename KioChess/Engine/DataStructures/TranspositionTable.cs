using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Transposition;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures;

[SkipLocalsInit]
public class TranspositionTable
{
    private readonly TranspositionHashSet WhiteTable;
    private readonly TranspositionHashSet BlackTable;

    private readonly Board _board;

    public TranspositionTable(int capacity, Board board, IConfigurationProvider configurationProvider)
    {
        _board = board; 

        var config = configurationProvider.GeneralConfiguration;

        WhiteTable = new TranspositionHashSet(capacity, config.TranspositionTableDepthFactor, config.TranspositionTableTypeFactor);
        BlackTable = new TranspositionHashSet(capacity, config.TranspositionTableDepthFactor, config.TranspositionTableTypeFactor);
    }

    public int Count => WhiteTable.Count + BlackTable.Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TranspositionEntry GetWhite() => WhiteTable.GetValue(_board.Hash);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TranspositionEntry GetBlack() => BlackTable.GetValue(_board.Hash);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlocked() => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetWhite(TranspositionEntry item) => WhiteTable.Set(_board.Hash, item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBlack(TranspositionEntry item) => BlackTable.Set(_board.Hash, item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        WhiteTable.Clear();
        BlackTable.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update()
    {
        WhiteTable.NewGeneration();
        BlackTable.NewGeneration();
    }

    internal void Resize(int offset)
    {

    }
}