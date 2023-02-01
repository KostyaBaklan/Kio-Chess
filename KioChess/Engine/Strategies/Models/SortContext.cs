using Engine.DataStructures;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using Engine.Sorting.Sorters;
using System.Runtime.CompilerServices;

namespace Engine.Strategies.Models
{
    public abstract  class SortContext
    {
        public bool HasPv;
        public bool IsPvCapture;
        public short Pv;
        public MoveSorter MoveSorter;
        public byte[] Pieces;
        public SquareList[] Squares;
        public SquareList PromotionSquares;

        protected SortContext()
        {
            Squares = new SquareList[6];
            for (int i = 0; i < Squares.Length; i++)
            {
                Squares[i] = new SquareList();
            }
            PromotionSquares = new SquareList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(MoveSorter sorter, MoveBase pv)
        {
            MoveSorter = sorter;
            MoveSorter.SetKillers();

            if(pv!= null)
            {
                HasPv = true;
                Pv = pv.Key;
                IsPvCapture = pv.IsAttack;
            }
            else
            {
                HasPv = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Set(MoveSorter sorter)
        {
            MoveSorter = sorter;
            MoveSorter.SetKillers();
            HasPv = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetForEvaluation(MoveSorter sorter)
        {
            MoveSorter = sorter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessHashMove(MoveBase move)
        {
            MoveSorter.ProcessHashMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessKillerMove(MoveBase move)
        {
            MoveSorter.ProcessKillerMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessCastleMove(MoveBase move)
        {
            MoveSorter.ProcessCastleMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessCaptureMove(AttackBase move)
        {
            MoveSorter.ProcessCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void ProcessMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKiller(short key)
        {
            return MoveSorter.IsKiller(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MoveList GetMoves()
        {
            return MoveSorter.GetMoves();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InitializeSort()
        {
            MoveSorter.InitializeSort();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void FinalizeSort()
        {
            MoveSorter.FinalizeSort();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void ProcessPromotionMoves(PromotionList promotions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void ProcessPromotionCaptures(PromotionAttackList promotionAttackList);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessHashMoves(PromotionList promotions)
        {
            MoveSorter.ProcessHashMoves(promotions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ProcessHashMoves(PromotionAttackList promotions)
        {
            MoveSorter.ProcessHashMoves(promotions);
        }
    }

    public abstract class WhiteSortContext : SortContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessPromotionMoves(PromotionList promotions)
        {
            MoveSorter.ProcessWhitePromotionMoves(promotions);
            //for (int i = 0; i < promotions.Count; i++)
            //{
            //    MoveSorter.ProcessWhitePromotionMove(promotions[i]);
            //}
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessPromotionCaptures(PromotionAttackList promotionAttackList)
        {
            MoveSorter.ProcessWhitePromotionCaptures(promotionAttackList);
        }
    }
    public class WhiteOpeningSortContext : WhiteSortContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessMove(MoveBase move)
        {
            MoveSorter.ProcessWhiteOpeningMove(move);
        }
    }
    public class WhiteMiddleSortContext : WhiteSortContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessMove(MoveBase move)
        {
            MoveSorter.ProcessWhiteMiddleMove(move);
        }
    }
    public class WhiteEndSortContext : WhiteSortContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessMove(MoveBase move)
        {
            MoveSorter.ProcessWhiteEndMove(move);
        }
    }

    public abstract class BlackSortContext : SortContext 
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessPromotionMoves(PromotionList promotions)
        {
            MoveSorter.ProcessBlackPromotionMoves(promotions);
            //for (int i = 0; i < promotions.Count; i++)
            //{
            //    MoveSorter.ProcessBlackPromotionMove(promotions[i]);
            //}
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessPromotionCaptures(PromotionAttackList promotionAttackList)
        {
            MoveSorter.ProcessBlackPromotionCaptures(promotionAttackList);
        }
    }
    public class BlackOpeningSortContext : BlackSortContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessMove(MoveBase move)
        {
            MoveSorter.ProcessBlackOpeningMove(move);
        }
    }
    public class BlackMiddleSortContext : BlackSortContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessMove(MoveBase move)
        {
            MoveSorter.ProcessBlackMiddleMove(move);
        }
    }
    public class BlackEndSortContext : BlackSortContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessMove(MoveBase move)
        {
            MoveSorter.ProcessBlackEndMove(move);
        }
    }
}
