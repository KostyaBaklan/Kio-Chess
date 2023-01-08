using System.Runtime.CompilerServices;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.DataStructures.Moves.Collections
{
    public class AttackCollection : MoveCollectionBase
    {
        protected readonly MoveList WinCaptures;
        protected readonly MoveList Trades;
        protected readonly MoveList LooseCaptures;
        protected readonly MoveList HashMoves;

        public AttackCollection(IMoveComparer comparer) : base(comparer)
        {
            WinCaptures = new MoveList();
            Trades = new MoveList();
            LooseCaptures = new MoveList();
            HashMoves = new MoveList();
        }

        #region Implementation of IMoveCollection

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddWinCapture(MoveBase move)
        {
            WinCaptures.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddTrade(MoveBase move)
        {
            Trades.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddLooseCapture(MoveBase move)
        {
            LooseCaptures.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddHashMove(MoveBase move)
        {
            HashMoves.Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MoveBase[] Build()
        {
            var hashMovesCount = HashMoves.Count;
            var winCapturesCount = hashMovesCount + WinCaptures.Count;
            var capturesCount = winCapturesCount + Trades.Count;
            Count = capturesCount + LooseCaptures.Count;

            MoveBase[] moves = new MoveBase[Count];

            if (hashMovesCount > 0)
            {
                HashMoves.CopyTo(moves, 0);
                HashMoves.Clear();
            }

            if (WinCaptures.Count>0)
            {
                WinCaptures.CopyTo(moves, hashMovesCount);
                WinCaptures.Clear();
            }

            if (Trades.Count > 0)
            {
                Trades.CopyTo(moves, winCapturesCount);
                Trades.Clear();
            }

            if (LooseCaptures.Count > 0)
            {
                LooseCaptures.CopyTo(moves, capturesCount);
                LooseCaptures.Clear();
            }

            Count = 0;
            return moves;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddWinCapture(MoveList moves)
        {
            WinCaptures.Add(moves);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddWinCapture(AttackList attackList)
        {
            for (int i = 0; i < attackList.Count; i++)
            {
                WinCaptures.Add(attackList[i]);
            }
        }

        //protected void FindNull(IMove[] moves)
        //{
        //    var i = Array.FindIndex(moves, m => m == null);
        //    if (i >= 0)
        //    {
        //        for (var x = i + 1; x < moves.Length; x++)
        //        {
        //            if (moves[x] != null)
        //            {
        //            }
        //        }
        //    }
        //}

    }
}