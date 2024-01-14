using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves;

public class KillerMoves
{
    private short _old;
    private short _new;

    #region Implementation of IKillerMoveCollection

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(short move)
    {
        if (_new == move || _old == move)
            return;

        _old = _new;
        _new = move;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(short move) => _new == move || _old == move;

    #endregion

    public override string ToString()
    {
        return $"{_new}, {_old}";
    }
}