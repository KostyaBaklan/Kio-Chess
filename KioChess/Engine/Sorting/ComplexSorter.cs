using Engine.DataStructures.Moves;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Boards.Structures;
using Engine.Models.Enums;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters;

public partial class ComplexSorter : MoveSorterBase
{
    private readonly int _tradeMargin;
    private readonly int _minusTradeMargin;
    protected readonly BitBoard _minorStartRanks;
    protected readonly BitBoard _whitePawnRank;
    protected readonly BitBoard _blackPawnRank;
    protected readonly BitBoard _whiteForpost;
    protected readonly BitBoard _blackForpost;
    protected readonly AttackList Attacks;
    protected readonly AttackList MinorLooseAttacks;
    protected readonly AttackList MajorLooseAttacks;
    private bool[] LowSee;
    private int _mobilityValue;
    private int[] _mobilityThresholds;

    public ComplexSorter(Position position) : base(position)
    {
        Attacks = [];
        MinorLooseAttacks = [];
        MajorLooseAttacks = [];
        _minorStartRanks = Board.GetRank(0) | Board.GetRank(7);
        _whitePawnRank = Board.GetRank(2);
        _blackPawnRank = Board.GetRank(5);
        _whiteForpost = (Board.GetRank(4) | Board.GetRank(5)).Remove(Board.GetFile(0) | Board.GetFile(7));
        _blackForpost = (Board.GetRank(2) | Board.GetRank(3)).Remove(Board.GetFile(0) | Board.GetFile(7));

        _tradeMargin = ConfigurationProvider.AlgorithmConfiguration.MarginConfiguration.TradeMargin;
        _minusTradeMargin = -_tradeMargin;
        _mobilityThresholds = ConfigurationProvider.AlgorithmConfiguration.SortingConfiguration.MobilityThreshold;
    }

    // Existing methods remain unchanged
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void GetOpeningMoves(ref MoveHistoryList moves) => MoveCollection.BuildOpening(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void GetBookOpeningMoves(ref MoveHistoryList moves) => MoveCollection.BuildBookOpening(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void GetBookMiddleMoves(ref MoveHistoryList moves) => MoveCollection.BuildBookMiddle(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void GetMiddleMoves(ref MoveHistoryList moves) => MoveCollection.BuildMiddle(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void GetEndMoves(ref MoveHistoryList moves) => MoveCollection.BuildEnd(ref moves);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void GetBookEndMoves(ref MoveHistoryList moves) => MoveCollection.BuildBookEnd(ref moves);

    // Removed helper methods, now in ComplexSorter.Shared.cs

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal override void SetValues()
    {
        StaticValue = Position.GetStaticValue();
        //Phase = MoveHistoryService.GetPhase();
        LowSee = DataPoolService.GetCurrentLowSee();

        byte phase = MoveHistoryService.GetPhase();
        if (phase != Phase.End)
        {
            _mobilityValue = _mobilityThresholds[phase] +
                (Position.GetTurn() == Turn.White
                ? Board.CountTotalWhiteMobility()
                : Board.CountTotalBlackMobility());
        }
    }
}
