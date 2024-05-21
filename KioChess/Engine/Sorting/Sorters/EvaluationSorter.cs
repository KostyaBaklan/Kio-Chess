using Engine.DataStructures.Moves.Collections;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters
{
    internal class EvaluationSorter : MoveSorter<EvaluationMoveCollection>
    {
        private int _alpha;
        private int _pat;
        private int[][] _deltaMarging;

        public EvaluationSorter(Position position) : base(position)
        {
            _deltaMarging = new int[3][];
            for (int i = 0; i < 3; i++)
            {
                _deltaMarging[i] = new int[12]
                {
                    100,325,325,500,975,0,100,325,325,500,975,0
                };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void InitializeMoveCollection()
        {
            AttackCollection = new EvaluationMoveCollection();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackEndCapture(AttackBase move)
        {
            ProcessCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackEndMove(MoveBase move)
        {
            AttackCollection.AddNonCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackMiddleCapture(AttackBase move)
        {
            ProcessCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackMiddleMove(MoveBase move)
        {
            AttackCollection.AddNonCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackOpeningCapture(AttackBase move)
        {
            ProcessCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackOpeningMove(MoveBase move)
        {
            AttackCollection.AddNonCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessCounterMove(MoveBase move)
        {
            AttackCollection.AddNonCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessHashMove(MoveBase move)
        {
            AttackCollection.AddNonCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessHashMoves(PromotionList promotions)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessHashMoves(PromotionAttackList promotions)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessKillerMove(MoveBase move)
        {
            AttackCollection.AddNonCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteEndCapture(AttackBase move)
        {
            ProcessCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteEndMove(MoveBase move)
        {
            AttackCollection.AddNonCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteMiddleCapture(AttackBase move)
        {
            ProcessCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteMiddleMove(MoveBase move)
        {
            AttackCollection.AddNonCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteOpeningCapture(AttackBase move)
        {
            ProcessCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteOpeningMove(MoveBase move)
        {
            AttackCollection.AddNonCaptureMove(move);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessCaptureMove(AttackBase attack)
        {
            //Position.Make(attack);
            //Position.UnMake();

            attack.Captured = Board.GetPiece(attack.To);
            int attackValue = Board.StaticExchange(attack);

            //if (!attack.IsCheck && _pat + attackValue + 100 < _alpha) return;

            if (_pat + attackValue + 100 < _alpha) return;

            if (attackValue > 0)
            {
                attack.See = attackValue;
                AttackCollection.AddWinCapture(attack);
            }
            else if (attackValue < 0)
            {
                attack.See = attackValue;
                AttackCollection.AddLooseCapture(attack);
            }
            else
            {
                AttackCollection.AddTrade(attack);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void SetValues(int alpha, int pat)
        {
            Phase = Position.GetPhase();
            _alpha = alpha;
            _pat = pat;
        }
    }
}
