using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Enums;

namespace Engine.Models.Moves
{
    public class PawnOverAttack : Attack
    {
        protected IMoveHistoryService history;
        public MoveBase EnPassant;

        public PawnOverAttack()
        {
            Type = MoveType.EatOver;
            history = ServiceLocator.Current.GetInstance<IMoveHistoryService>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegal(IBoard board)
        {
            return history.IsLast(EnPassant.Key) && EnPassant.IsEnPassant;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsLegalAttack(IBoard board)
        {
            return IsLegal(board);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Make(IBoard board, ArrayStack<Piece> figureHistory)
        {
            board.Remove(EnPassant.Piece, EnPassant.To);
            board.Move(Piece, From, To);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void UnMake(IBoard board, ArrayStack<Piece> figureHistory)
        {
            board.Move(Piece, To, From);
            board.Add(EnPassant.Piece, EnPassant.To);
        }
    }
}