using Engine.Book.Interfaces;
using Engine.DataStructures;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
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
        public short CounterMove;
        public MoveSorterBase MoveSorter;
        public byte[] Pieces;
        public SquareList[] Squares;
        public SquareList PromotionSquares;
        public int Ply;
        public Dictionary<short, int> Book;

        protected static Dictionary<short, int> _defaultValue = new Dictionary<short, int>();
        public static bool UseBooking;
        public static IPosition Position;
        public static IBookService BookService;
        public static IMoveHistoryService MoveHistory;

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
        public void Set(MoveSorterBase sorter, MoveBase pv = null)
        {
            MoveSorter = sorter;
            MoveSorter.SetKillers();
            CounterMove = sorter.GetCounterMove();

            if (pv != null)
            {
                HasPv = true;
                Pv = pv.Key;
                IsPvCapture = pv.IsAttack;
            }
            else
            {
                HasPv = false;
            }

            UpdateBook();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetForEvaluation(MoveSorterBase sorter)
        {
            MoveSorter = sorter;

            UpdateBook();
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
        public void ProcessCounterMove(MoveBase move)
        {
            MoveSorter.ProcessCounterMove(move);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void SetBook();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddSuggestedBookMove(MoveBase move)
        {
            MoveSorter.AddSuggestedBookMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddNonSuggestedBookMove(MoveBase move)
        {
            MoveSorter.AddNonSuggestedBookMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UpdateBook()
        {
            if (UseBooking)
            {
                SetBook();
            }
            else
            {
                Book = _defaultValue;
            }
        }
    }

    public abstract class WhiteSortContext : SortContext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetBook()
        {
            MoveKeyList history = stackalloc short[MoveHistory.GetSequenceSize()];

            MoveHistory.GetSequence(ref history);

            Book = BookService.GetWhiteBookValues(ref history);
        }

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
        protected override void SetBook()
        {
            MoveKeyList history = stackalloc short[MoveHistory.GetSequenceSize()];

            MoveHistory.GetSequence(ref history);

            Book = BookService.GetBlackBookValues(ref history);
        }

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
