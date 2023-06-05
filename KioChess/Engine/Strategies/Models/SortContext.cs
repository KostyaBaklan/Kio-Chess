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
        public MoveSorterBase MoveSorter;
        public byte[] Pieces;
        public SquareList[] Squares;
        public SquareList PromotionSquares;
        public int Ply;

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
        public void Set(MoveSorterBase sorter, MoveBase pv)
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
        public void Set(MoveSorterBase sorter)
        {
            MoveSorter = sorter;
            MoveSorter.SetKillers();
            HasPv = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetForEvaluation(MoveSorterBase sorter)
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
        public abstract void ProcessCaptureMove(AttackBase move);

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
        public override void ProcessCaptureMove(AttackBase move)
        {
            MoveSorter.ProcessWhiteOpeningCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessMove(MoveBase move)
        {
            MoveSorter.ProcessWhiteOpeningMove(move);
        }
    }
    public class WhiteMiddleSortContext : WhiteSortContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessCaptureMove(AttackBase move)
        {
            MoveSorter.ProcessWhiteMiddleCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessMove(MoveBase move)
        {
            MoveSorter.ProcessWhiteMiddleMove(move);
        }
    }
    public class WhiteEndSortContext : WhiteSortContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessCaptureMove(AttackBase move)
        {
            MoveSorter.ProcessWhiteEndCapture(move);
        }

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
        public override void ProcessCaptureMove(AttackBase move)
        {
            MoveSorter.ProcessBlackOpeningCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessMove(MoveBase move)
        {
            MoveSorter.ProcessBlackOpeningMove(move);
        }
    }
    public class BlackMiddleSortContext : BlackSortContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessCaptureMove(AttackBase move)
        {
            MoveSorter.ProcessBlackMiddleCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessMove(MoveBase move)
        {
            MoveSorter.ProcessBlackMiddleMove(move);
        }
    }
    public class BlackEndSortContext : BlackSortContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessCaptureMove(AttackBase move)
        {
            MoveSorter.ProcessBlackEndCapture(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ProcessMove(MoveBase move)
        {
            MoveSorter.ProcessBlackEndMove(move);
        }
    }
}
