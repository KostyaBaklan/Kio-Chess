using Engine.DataStructures.Moves.Lists;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.DataStructures.Moves.Collections
{
    public class EvaluationMoveCollection : AttackCollection
    {
        protected readonly MoveList NonCaptureMoves;
        public EvaluationMoveCollection() : base()
        {
            NonCaptureMoves = new MoveList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddNonCaptureMove(MoveBase move) => NonCaptureMoves.Add(move);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MoveList Build()
        {
            var moves = DataPoolService.GetCurrentMoveList();
            moves.Clear();

            if (WinCaptures.Count > 0)
            {
                WinCaptures.SortBySee();
                moves.Add(WinCaptures);
                WinCaptures.Clear();
            }
            if (Trades.Count > 0)
            {
                moves.Add(Trades);
                Trades.Clear();
            }
            //if (NonCaptureMoves.Count > 0)
            //{
            //    moves.SortAndCopy(NonCaptureMoves, Moves);
            //    NonCaptureMoves.Clear();
            //}
            if (LooseCaptures.Count > 0)
            {
                if (moves.Count < 1)
                {
                    LooseCaptures.SortBySee();
                    moves.Add(LooseCaptures); 
                }
                LooseCaptures.Clear();
            }
            return moves;
        }
    }
}
