using Engine.DataStructures;
using Engine.DataStructures.Moves.Collections;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public partial class ComplexSorter : MoveSorter<ComplexMoveCollection>
{
    private readonly int _tradeMargin;
    private readonly int _minusTradeMargin;
    protected readonly BitBoard _minorStartRanks;
    protected readonly BitBoard _whitePawnRank;
    protected readonly BitBoard _blackPawnRank;
    protected readonly BitBoard _whiteForpost;
    protected readonly BitBoard _blackForpost;
    protected readonly PositionsList PositionsList;
    protected readonly AttackList Attacks;
    private bool[] LowSee;

    public ComplexSorter(Position position) : base(position)
    {
        PositionsList = new PositionsList();
        Attacks = [];
        _minorStartRanks = Board.GetRank(0) | Board.GetRank(7);
        _whitePawnRank = Board.GetRank(2);
        _blackPawnRank = Board.GetRank(5);
        _whiteForpost = (Board.GetRank(4) | Board.GetRank(5)).Remove(Board.GetFile(0) | Board.GetFile(7));
        _blackForpost = (Board.GetRank(2) | Board.GetRank(3)).Remove(Board.GetFile(0) | Board.GetFile(7));

        _tradeMargin = ConfigurationProvider.AlgorithmConfiguration.MarginConfiguration.TradeMargin;
        _minusTradeMargin = -_tradeMargin;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessKillerMove(MoveBase move) => AttackCollection.AddKillerMove(move);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void ProcessCounterMove(MoveBase move) => AttackCollection.AddCounterMove(move);

    // Removed helper methods, now in ComplexSorter.Shared.cs

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void SetValues()
    {
        StaticValue = Position.GetStaticValue();
        //Phase = MoveHistoryService.GetPhase();
        LowSee = DataPoolService.GetCurrentLowSee();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void InitializeMoveCollection() => AttackCollection = new ComplexMoveCollection();
}
