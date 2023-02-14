using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections
{
    public class MoveCollection: AttackCollection
    {
        private readonly MoveList _killers;
        private readonly MoveList _nonCaptures;

        public MoveCollection(IMoveComparer comparer) : base(comparer)
        {
            _killers = new MoveList();
            _nonCaptures = new MoveList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddKillerMove(MoveBase move)
        {
            _killers.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNonCapture(MoveBase move)
        {
            _nonCaptures.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MoveList Build()
        {
            var moves = DataPoolService.GetCurrentMoveList();
            moves.Clear();

            if (HashMoves.Count + WinCaptures.Count + Trades.Count + _killers.Count > 0)
            {
                if (HashMoves.Count > 0)
                {
                    moves.Add(HashMoves);
                    HashMoves.Clear();
                }

                if (WinCaptures.Count > 0)
                {
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

                if (LooseCaptures.Count > 0)
                {
                    moves.Add(LooseCaptures);
                    LooseCaptures.Clear();
                }

                if (_nonCaptures.Count > 0)
                {
                    _nonCaptures.Sort();
                    moves.Add(_nonCaptures);
                    _nonCaptures.Clear();
                }
            }
            else
            {
                if (_nonCaptures.Count > 0)
                {
                    _nonCaptures.Sort();
                    moves.Add(_nonCaptures);
                    _nonCaptures.Clear();
                }
                if (LooseCaptures.Count > 0)
                {
                    moves.Add(LooseCaptures);
                    LooseCaptures.Clear();
                }
            }

            return moves;
        }
    }
}
