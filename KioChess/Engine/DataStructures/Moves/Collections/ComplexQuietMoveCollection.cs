using Engine.DataStructures.Moves.Lists;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Collections;

public class ComplexQuietMoveCollection : ComplexMoveCollection
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void ProcessNonCaptures(MoveList moves)
    {
        if (_suggested.Count > 0)
        {
            moves.SortAndCopy(_suggested, Moves);
            _suggested.Clear();
        }
        if (_forwardMoves.Count > 0)
        {
            moves.SortAndCopy(_forwardMoves, Moves);
            _forwardMoves.Clear();
        }
        if (_nonCaptures.Count > 0)
        {
            moves.SortAndCopy(_nonCaptures, Moves);
            _nonCaptures.Clear();
        }
        if (LooseCaptures.Count > 0)
        {
            LooseCaptures.SortBySee();
            moves.Add(LooseCaptures);
            LooseCaptures.Clear();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void SetPromisingMoves(MoveList moves)
    {
        if (WinCaptures.Count > 0)
        {
            WinCaptures.SortBySee();
            moves.Add(WinCaptures);
            WinCaptures.Clear();
        }

        if (Trades.Count > 0)
        {
            moves.Add(Trades);
            Trades.Clear();
        }

        if (_killers.Count > 0)
        {
            moves.Add(_killers);
            _killers.Clear();
        }

        if (_counters.Count > 0)
        {
            moves.Add(_counters[0]);
            _counters.Clear();
        }
    }
}
