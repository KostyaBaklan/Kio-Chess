using System.Runtime.CompilerServices;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections
{
    public class AdvancedMoveCollection : AttackCollection
    {
        private readonly MoveList _killers;
        private readonly MoveList _nonCaptures;
        private readonly MoveList _checks;
        protected readonly MoveList LooseTrades;

        public AdvancedMoveCollection(IMoveComparer comparer) : base(comparer)
        {
            _killers = new MoveList();
            _nonCaptures = new MoveList();
            _checks = new MoveList();
            LooseTrades = new MoveList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddCheck(MoveBase move)
        {
            _checks.Add(move);
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
        public void AddBadMove(MoveBase move)
        {
            LooseCaptures.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSuggested(MoveBase move)
        {
            _checks.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNonSuggested(MoveBase move)
        {
            LooseTrades.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MoveBase[] Build()
        {
            var hashMovesCount = HashMoves.Count;
            var winCapturesCount = hashMovesCount + WinCaptures.Count;
            var tradesCount = winCapturesCount + Trades.Count;
            var killersCount = tradesCount + _killers.Count;
            int checksCount = killersCount + _checks.Count;
            var nonCapturesCount = checksCount + LooseCaptures.Count;
            var looseTradesCount = nonCapturesCount + _nonCaptures.Count;
            Count = looseTradesCount + LooseTrades.Count;

            MoveBase[] moves = new MoveBase[Count];

            if (checksCount > 0)
            {
                if (hashMovesCount > 0)
                {
                    HashMoves.CopyTo(moves, 0);
                    HashMoves.Clear();
                }

                if (WinCaptures.Count > 0)
                {
                    WinCaptures.CopyTo(moves, hashMovesCount);
                    WinCaptures.Clear();
                }

                if (Trades.Count > 0)
                {
                    Trades.CopyTo(moves, winCapturesCount);
                    Trades.Clear();
                }

                if (_killers.Count > 0)
                {
                    _killers.CopyTo(moves, tradesCount);
                    _killers.Clear();
                }

                if (_checks.Count > 0)
                {
                    _checks.CopyTo(moves, killersCount);
                    _checks.Clear();
                }

                if (LooseCaptures.Count > 0)
                {
                    LooseCaptures.CopyTo(moves, checksCount);
                    LooseCaptures.Clear();
                }

                if (_nonCaptures.Count > 0)
                {
                    _nonCaptures.Sort();
                    _nonCaptures.CopyTo(moves, nonCapturesCount);
                    _nonCaptures.Clear();
                }

                if (LooseTrades.Count > 0)
                {
                    LooseTrades.CopyTo(moves, looseTradesCount);
                    LooseTrades.Clear();
                } 
            }
            else
            {
                var capturesCount = _nonCaptures.Count;
                if (capturesCount > 0)
                {
                    _nonCaptures.Sort();
                    _nonCaptures.CopyTo(moves, 0);
                    _nonCaptures.Clear();
                }

                if (LooseCaptures.Count > 0)
                {
                    LooseCaptures.CopyTo(moves, capturesCount);
                    LooseCaptures.Clear();
                }

                if (LooseTrades.Count > 0)
                {
                    LooseTrades.CopyTo(moves, looseTradesCount);
                    LooseTrades.Clear();
                }
            }
            Count = 0;
            return moves;
        }
    }
}