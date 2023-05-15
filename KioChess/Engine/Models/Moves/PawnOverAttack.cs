using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.Interfaces;

namespace Engine.Models.Moves
{
    public class PawnOverAttack : Attack
    {
        protected IMoveHistoryService history;
        public MoveBase EnPassant;

        public PawnOverAttack()
        {
            history = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal()
        {
            return history.IsLast(EnPassant.Key) && EnPassant.IsEnPassant;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegalAttack()
        {
            return IsLegal();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make()
        {
            Board.Remove(EnPassant.Piece, EnPassant.To);
            Board.Move(Piece, From, To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake()
        {
            Board.Move(Piece, To, From);
            Board.Add(EnPassant.Piece, EnPassant.To);
        }
    }
}