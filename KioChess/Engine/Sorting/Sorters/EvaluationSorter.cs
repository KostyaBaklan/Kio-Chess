using Engine.DataStructures.Moves.Collections;
using Engine.DataStructures.Moves.Lists;
using Engine.Models.Boards;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters
{
    internal class AttackSorter : MoveSorter<AttackCollection>
    {
        private int _alpha;
        private int[][] _deltaMarging;
        private int[] _promotionMarging;

        public AttackSorter(Position position) : base(position)
        {
            _deltaMarging = new int[3][];
            for (int i = 0; i < 3; i++)
            {
                _deltaMarging[i] = new int[12]
                {
                    100,325,325,500,975,0,100,325,325,500,975,0
                };
            }
            _promotionMarging = new int[12]
                {
                    100,325,325,500,975,0,100,325,325,500,975,0
                };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void InitializeMoveCollection()
        {
            AttackCollection = new AttackCollection();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackPromotionMoves(PromotionList moves)
        {
            Position.Make(moves[0]);
            Position.GetWhiteAttacksTo(moves[0].To, attackList);

            ProcessPromotions(moves);

            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhitePromotionMoves(PromotionList moves)
        {
            Position.Make(moves[0]);
            Position.GetBlackAttacksTo(moves[0].To, attackList);

            ProcessPromotions(moves);

            Position.UnMake();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessPromotions(PromotionList moves)
        {
            if (attackList.Count == 0)
            {
                AddWinPromotions(moves);
            }
            else
            {
                ProcessPromotionMoves(moves);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessPromotionMoves(PromotionList moves)
        {
            int max = short.MinValue;
            for (byte i = 0; i < attackList.Count; i++)
            {
                var attack = attackList[i];
                attack.Captured = WhitePawn;
                int see = Board.StaticExchange(attack);
                if (see > max)
                {
                    max = see;
                }
            }

            if (100 - max > _alpha)
            {
                if (max < 0)
                {
                    AttackCollection.AddWinCapture(moves);
                }
                else
                {
                    AttackCollection.AddLooseCapture(moves);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddWinPromotions(PromotionList moves)
        {
            for (byte i = 0; i < moves.Count; i++)
            {
                var move = moves[i];

                if (_promotionMarging[move.PromotionPiece] < _alpha) continue;

                AttackCollection.AddWinCapture(move);
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //internal override void ProcessWhitePromotionCaptures(PromotionAttackList promotions) => ProcessPromotionCaptures(promotions);

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //internal override void ProcessBlackPromotionCaptures(PromotionAttackList promotions) => ProcessPromotionCaptures(promotions);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackEndCapture(AttackBase move)
        {
            ProcessCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackEndMove(MoveBase move)
        {
            //AttackCollection.AddNonCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackMiddleCapture(AttackBase move)
        {
            ProcessCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackMiddleMove(MoveBase move)
        {
            //AttackCollection.AddNonCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackOpeningCapture(AttackBase move)
        {
            ProcessCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessBlackOpeningMove(MoveBase move)
        {
            //AttackCollection.AddNonCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessCounterMove(MoveBase move)
        {
           // AttackCollection.AddNonCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessHashMove(MoveBase move)
        {
            //AttackCollection.AddNonCaptureMove(move);
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
            //AttackCollection.AddNonCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteEndCapture(AttackBase move)
        {
            ProcessCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteEndMove(MoveBase move)
        {
            //AttackCollection.AddNonCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteMiddleCapture(AttackBase move)
        {
            ProcessCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteMiddleMove(MoveBase move)
        {
            //AttackCollection.AddNonCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteOpeningCapture(AttackBase move)
        {
            ProcessCaptureMove(move);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessWhiteOpeningMove(MoveBase move)
        {
            //AttackCollection.AddNonCaptureMove(move);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessCaptureMove(AttackBase attack)
        {
            //Position.Make(attack);
            //Position.UnMake();

            attack.Captured = Board.GetPiece(attack.To);
            int attackValue = Board.StaticExchange(attack);

            //if (!attack.IsCheck && _pat + attackValue + 100 < _alpha) return;

            if (attackValue + 100 < _alpha) return;

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
            _alpha = alpha - pat;
        }
    }
}
