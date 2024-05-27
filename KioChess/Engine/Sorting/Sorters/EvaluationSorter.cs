using CommonServiceLocator;
using Engine.DataStructures.Moves.Collections;
using Engine.DataStructures.Moves.Lists;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Sorting.Sorters
{
    internal class AttackSorter : MoveSorter<AttackCollection>
    {
        //private int _promotionAlpha;
        private int _attackAlpha;
        private readonly int _attackMargin;
        private readonly int[] _promotionMargin;

        public AttackSorter(Position position) : base(position)
        {
            var configuration = ServiceLocator.Current.GetInstance<IConfigurationProvider>();

            _attackMargin = configuration.AlgorithmConfiguration.MarginConfiguration.AttackMargin;
            _promotionMargin = configuration.AlgorithmConfiguration.MarginConfiguration.PromotionMargins;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void InitializeMoveCollection()
        {
            AttackCollection = new AttackCollection();
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //internal override void ProcessWhitePromotionCaptures(PromotionAttackList moves)
        //{
        //    Position.Make(moves[0]);

        //    AttackBase attack = Position.GetBlackAttackTo(moves[0].To);
        //    if (attack == null)
        //    {
        //        Position.UnMake();
        //        var capturedValue = PromotionAttack.CapturedValue[Board.GetPiece(moves[0].To)];
        //        for (byte i = Zero; i < moves.Count; i++)
        //        {
        //            AddPromotion(moves[i], moves[i].PromotionSee + capturedValue);
        //        }
        //    }
        //    else
        //    {
        //        attack.Captured = WhitePawn;
        //        int see = -Board.StaticExchange(attack);

        //        for (byte i = Zero; i < moves.Count; i++)
        //        {
        //            AddPromotion(moves[i], see);
        //        }
        //        Position.UnMake();
        //    }
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //internal override void ProcessBlackPromotionCaptures(PromotionAttackList moves)
        //{
        //    Position.Make(moves[0]);
        //    AttackBase attack = Position.GetWhiteAttackTo(moves[0].To);
        //    if (attack == null)
        //    {
        //        Position.UnMake();
        //        var capturedValue = PromotionAttack.CapturedValue[Board.GetPiece(moves[0].To)];
        //        for (byte i = Zero; i < moves.Count; i++)
        //        {
        //            AddPromotion(moves[i], moves[i].PromotionSee + capturedValue);
        //        }
        //    }
        //    else
        //    {
        //        attack.Captured = BlackPawn;
        //        int see = -Board.StaticExchange(attack);

        //        for (byte i = Zero; i < moves.Count; i++)
        //        {
        //            AddPromotion(moves[i], see);
        //        }
        //        Position.UnMake();
        //    }
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //internal override void ProcessBlackPromotionMoves(PromotionList moves)
        //{
        //    Position.Make(moves[0]);
        //    AttackBase attack = Position.GetWhiteAttackTo(moves[0].To);
        //    if (attack == null)
        //    {
        //        for (byte i = Zero; i < moves.Count; i++)
        //        {
        //            AddPromotion(moves[i], moves[i].PromotionSee);
        //        }
        //    }
        //    else
        //    {
        //        attack.Captured = BlackPawn;
        //        int see = -Board.StaticExchange(attack);

        //        for (byte i = Zero; i < moves.Count; i++)
        //        {
        //            AddPromotion(moves[i], see);
        //        }
        //    }
        //    Position.UnMake();
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //internal override void ProcessWhitePromotionMoves(PromotionList moves)
        //{
        //    Position.Make(moves[0]);

        //    AttackBase attack = Position.GetBlackAttackTo(moves[0].To);
        //    if (attack == null)
        //    {
        //        for (byte i = Zero; i < moves.Count; i++)
        //        {
        //            AddPromotion(moves[i], moves[i].PromotionSee);
        //        }
        //    }
        //    else
        //    {
        //        attack.Captured = WhitePawn;
        //        int see = -Board.StaticExchange(attack);

        //        for (byte i = Zero; i < moves.Count; i++)
        //        {
        //            AddPromotion(moves[i], see);
        //        }
        //    }
        //    Position.UnMake();
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddPromotion(AttackBase move, int attackValue)
        {
            if (attackValue > _attackAlpha)
            {
                move.See = attackValue;
                AttackCollection.AddWinCapture(move);
            }
            else
            {
                Position.Make(move);
                Position.UnMake();

                if (!move.IsCheck)
                    return;

                move.See = attackValue;
                if (attackValue > 0)
                {
                    AttackCollection.AddWinCapture(move);
                }
                else
                {
                    AttackCollection.AddLooseCapture(move);
                }
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //internal override void ProcessWhitePromotionCaptures(PromotionAttackList promotions)
        //{
        //    PromotionCaptures(promotions);
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //internal override void ProcessBlackPromotionCaptures(PromotionAttackList promotions)
        //{
        //    PromotionCaptures(promotions);
        //}

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
            attack.Captured = Board.GetPiece(attack.To);
            int attackValue = Board.StaticExchange(attack);

            if (_attackAlpha > attackValue && !Board.IsCheck(attack))
                return;

            if (attackValue > 0)
            {
                attack.See = attackValue;
                AttackCollection.AddWinCapture(attack);
            }
            else if (attackValue == 0)
            {
                AttackCollection.AddTrade(attack);
            }
            else
            {
                attack.See = attackValue;
                AttackCollection.AddLooseCapture(attack);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void SetValues(int alpha, int pat)
        {
            //Phase = Board.GetPhase();
            //_promotionAlpha = alpha - pat;
            _attackAlpha = Math.Max(alpha - pat - _attackMargin,-1);
        }
    }
}
