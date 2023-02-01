using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
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
        protected readonly IDataPoolService DataPoolService = ServiceLocator.Current.GetInstance<IDataPoolService>();

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
        public override MoveList Build()
        {
            var winCapturesCount = WinCaptures.Count;
            var capturesCount = winCapturesCount + Trades.Count;

            var moves = DataPoolService.GetCurrentMoveList();
            moves.Clear();

            if (WinCaptures.Count > 0)
            {
                WinCaptures.CopyTo(moves, 0);
                WinCaptures.Clear();
            }

            if (Trades.Count > 0)
            {
                Trades.CopyTo(moves, winCapturesCount);
                Trades.Clear();
            }

            if (LooseCaptures.Count > 0)
            {
                if (winCapturesCount < 1)
                {
                    LooseCaptures.CopyTo(moves, capturesCount);
                }
                else
                {

                }
                LooseCaptures.Clear();
            }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddWinCapture(PromotionList moves)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                WinCaptures.Add(moves[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddLooseCapture(PromotionList moves)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                LooseCaptures.Add(moves[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddTrade(PromotionList moves)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                Trades.Add(moves[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddLooseCapture(PromotionAttackList moves)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                LooseCaptures.Add(moves[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddHashMoves(PromotionAttackList moves)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                HashMoves.Add(moves[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddHashMoves(PromotionList moves)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                HashMoves.Add(moves[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddTrade(PromotionAttackList moves)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                Trades.Add(moves[i]);
            }
        }
    }
}