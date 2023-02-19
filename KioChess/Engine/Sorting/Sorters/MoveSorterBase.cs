using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;

namespace Engine.Sorting.Sorters
{
    public abstract class MoveSorterBase
    {
        protected readonly IKillerMoveCollection[] Moves;
        protected readonly AttackList attackList;
        protected readonly IMoveHistoryService MoveHistoryService;
        protected IMoveComparer Comparer;
        protected IKillerMoveCollection CurrentKillers;
        protected readonly IPosition Position;
        protected readonly MoveList EmptyList;

        protected readonly IBoard Board;
        protected readonly IMoveProvider MoveProvider = ServiceLocator.Current.GetInstance<IMoveProvider>();
        protected readonly IDataPoolService DataPoolService = ServiceLocator.Current.GetInstance<IDataPoolService>();

        protected MoveSorterBase(IPosition position, IMoveComparer comparer)
        {
            EmptyList = new MoveList(0);
            attackList = new AttackList();
            Board = position.GetBoard();
            Comparer = comparer;
            Moves = ServiceLocator.Current.GetInstance<IKillerMoveCollectionFactory>().CreateMoves();
            Position = position;

            MoveHistoryService = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Add(short move)
        {
            Moves[MoveHistoryService.GetPly()].Add(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessHashMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessKillerMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessCaptureMove(AttackBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract MoveList GetMoves();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessWhiteOpeningMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessWhiteMiddleMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessWhiteEndMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessBlackOpeningMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessBlackMiddleMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessBlackEndMove(MoveBase move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool IsKiller(short key)
        {
            return CurrentKillers.Contains(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetKillers()
        {
            CurrentKillers = Moves[MoveHistoryService.GetPly()];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void FinalizeSort();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void InitializeSort()
        {
            attackList.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessWhitePromotionMoves(PromotionList promotions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessBlackPromotionMoves(PromotionList promotions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessWhitePromotionCaptures(PromotionAttackList promotionAttackList);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessBlackPromotionCaptures(PromotionAttackList promotionAttackList);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessHashMoves(PromotionList promotions);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void ProcessHashMoves(PromotionAttackList promotions);
    }
}