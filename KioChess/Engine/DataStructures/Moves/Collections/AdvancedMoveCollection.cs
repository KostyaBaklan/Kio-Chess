using System.Runtime.CompilerServices;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections
{
    public class AdvancedMoveCollection : AttackCollection
    {
        private readonly MoveList _killers;
        private readonly MoveList _nonCaptures;
        private readonly MoveList _suggested;

        public AdvancedMoveCollection(IMoveComparer comparer) : base(comparer)
        {
            _killers = new MoveList();
            _nonCaptures = new MoveList();
            _suggested = new MoveList();
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
            _nonCaptures.ExtractMax(Math.Min(3, _nonCaptures.Count), _suggested);

            var moves = DataPoolService.GetCurrentMoveList();
            moves.Clear();

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

            if (_suggested.Count > 0)
            {
                moves.Add(_suggested);
                _suggested.Clear();
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

            return moves;
        }
    }
}