using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Collections;

public class ComplexMoveCollection : ExtendedMoveCollection
{
    protected readonly MoveList _looseNonCapture;

    public ComplexMoveCollection(IMoveComparer comparer) : base(comparer, 7)
    {
        _looseNonCapture = new MoveList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLooseNonCapture(MoveBase move)
    {
        _looseNonCapture.Add(move);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void ProcessOtherMoves(MoveList moves)
    {
        if (_notSuggested.Count > 0)
        {
            moves.SortAndCopy(_notSuggested, Moves);
            _notSuggested.Clear();
        }

        if (_looseNonCapture.Count > 0)
        {
            moves.SortAndCopy(_looseNonCapture, Moves);
            _looseNonCapture.Clear();
        }

        if (_bad.Count > 0)
        {
            moves.Add(_bad);
            _bad.Clear();
        }
    }
}
