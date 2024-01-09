using CommonServiceLocator;
using Engine.Interfaces;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Lists;

public class MoveValueList
{
    protected const int MateOffset = 1750000000;
    protected const int HashOffset = 1500000000;
    protected const int BookOffset = 1250000000;
    protected const int WinOffset = 1000000000;
    protected const int TradeOffset = 750000000;
    protected const int KillerOffset = 500000000;
    protected const int CounterOffset = 250000000;
    protected const int SuggestedOffset = 0;
    protected const int ForwardOffset = -250000000;
    protected const int LooseOffset = -500000000;
    protected const int NonCaptureOffset = -750000000;
    protected const int BackwardOffset = -1000000000;
    protected const int NotSuggestedOffset = -1250000000;
    protected const int LooseNonCaptureOffset = -1500000000;
    protected const int BadOffset = -1750000000;

    public byte Count;
    public readonly MoveValue[] _items;
    public static IMoveProvider MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();

    public MoveValueList() : this(128)
    {
    }
    public MoveValueList(int capacity)
    {
        _items = new MoveValue[capacity];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(MoveValue move)
    {
        _items[Count++] = move;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Count = 0;
    }

    public MoveBase this[byte i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            SetMaximum(i);
            return MoveProvider.Get(_items[i].Key);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetMaximum(byte i)
    {
        byte m = i;
        for (byte j = (byte)(i + 1); j < Count; j++)
        {
            if (_items[j].IsGreater(_items[m]))
            {
                m = j;
            }
        }

        if (m > i)
        {
            Swap(i, m); 
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Swap(byte i, byte j)
    {
        var temp = _items[j];
        _items[j] = _items[i];
        _items[i] = temp;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(MoveBase[] moves)
    {
        for (int i = 0; i < moves.Length; i++)
        {
            Add(moves[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(MoveBase moveBase)
    {
        Add(new MoveValue(moveBase.Key));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ProcessHashMove(MoveBase move)
    {
        _items[Count++] = new MoveValue(move.Key, HashOffset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddSuggestedBookMove(MoveBase move)
    {
        _items[Count++] = new MoveValue(move.Key, BookOffset + move.BookValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddWinCapture(AttackBase move)
    {
        _items[Count++] = new MoveValue(move.Key, WinOffset + move.See);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddLooseCapture(AttackBase move)
    {
        _items[Count++] = new MoveValue(move.Key, LooseOffset + move.See);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddTrade(AttackBase move)
    {
        _items[Count++] = new MoveValue(move.Key, TradeOffset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddWinCaptures(PromotionAttackList promotions, short attackValue)
    {
        for (int i = 0; i < promotions.Count; i++)
        {
            _items[Count++] = new MoveValue(promotions._items[i].Key, WinOffset + attackValue);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddLooseCapture(PromotionAttackList promotions, short attackValue)
    {
        for (int i = 0; i < promotions.Count; i++)
        {
            _items[Count++] = new MoveValue(promotions._items[i].Key, LooseOffset + attackValue);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddWinCapture(PromotionList promotions)
    {
        for (int i = 0; i < promotions.Count; i++)
        {
            _items[Count++] = new MoveValue(promotions._items[i].Key, WinOffset);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddLooseCapture(PromotionList promotions)
    {
        for (int i = 0; i < promotions.Count; i++)
        {
            _items[Count++] = new MoveValue(promotions._items[i].Key, LooseOffset);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddHashMoves(PromotionList promotions)
    {
        for (int i = 0; i < promotions.Count; i++)
        {
            _items[Count++] = new MoveValue(promotions._items[i].Key, HashOffset);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddHashMoves(PromotionAttackList promotions)
    {
        for (int i = 0; i < promotions.Count; i++)
        {
            _items[Count++] = new MoveValue(promotions._items[i].Key, HashOffset);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddForwardMove(MoveBase move)
    {
        _items[Count++] = new MoveValue(move.Key, ForwardOffset + move.RelativeHistory);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddNonCapture(MoveBase move)
    {
        _items[Count++] = new MoveValue(move.Key, NonCaptureOffset + move.RelativeHistory);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddNonSuggested(MoveBase move)
    {
        _items[Count++] = new MoveValue(move.Key, NotSuggestedOffset + move.RelativeHistory);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddMateMove(MoveBase move)
    {
        _items[Count++] = new MoveValue(move.Key, MateOffset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddCounterMove(MoveBase move)
    {
        _items[Count++] = new MoveValue(move.Key, CounterOffset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddKillerMove(MoveBase move)
    {
        _items[Count++] = new MoveValue(move.Key, KillerOffset + move.RelativeHistory);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddLooseNonCapture(MoveBase move)
    {
        _items[Count++] = new MoveValue(move.Key, LooseNonCaptureOffset + move.RelativeHistory);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddSuggested(MoveBase move)
    {
        _items[Count++] = new MoveValue(move.Key, SuggestedOffset + move.RelativeHistory);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddBad(MoveBase move)
    {
        _items[Count++] = new MoveValue(move.Key, BadOffset + move.RelativeHistory);
    }
}
