using System.Runtime.CompilerServices;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Helpers;

namespace Engine.Models.Moves
{
    public abstract class MoveBase : IEquatable<MoveBase>, IComparable<MoveBase>
    {
        protected MoveBase()
        {
            IsCheck = false;
            EmptyBoard = new BitBoard(0ul);
            IsAttack = false;
            IsPromotion = false;
            IsCastle = false;
            IsEnPassant = false;
            IsPromotionToQueen = false;
        }

        #region Implementation of IMove

        public short Key;
        public bool IsCheck;
        public int Difference;
        public int History;
        public Piece Piece;
        public Square From;
        public Square To;
        public BitBoard EmptyBoard;
        public bool IsAttack;
        public bool IsCastle;
        public bool IsPromotion;
        public bool IsPassed;
        public bool IsEnPassant;
        public bool CanReduce;
        public bool IsIrreversible;
        public bool IsFutile;
        public bool IsPromotionToQueen;
        public bool IsWhite;
        public bool IsBlack;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract bool IsLegal(IBoard board);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsLegalAttack(IBoard board)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void Make(IBoard board, ArrayStack<Piece> figureHistory);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void UnMake(IBoard board, ArrayStack<Piece> figureHistory);

        public void Set(params int[] squares)
        {
            BitBoard v = new BitBoard();
            for (var index = 0; index < squares.Length; index++)
            {
                var s = squares[index];
                var board = s.AsBitBoard();
                v = v | board;
            }

            EmptyBoard = EmptyBoard |= v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsGreater(MoveBase move)
        {
            return History > move.History;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual bool IsQueenCaptured() { return false; }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            string sign = IsAttack ? "x" : "->";
            return $"[{Piece} {From.AsString()} {sign} {To.AsString()}, H={History}]";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && Equals((MoveBase) obj);
        }

        #region Equality members

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(MoveBase other)
        {
            return Key == other.Key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(MoveBase other)
        {
            return other.History.CompareTo(History);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(MoveBase left, MoveBase right)
        {
            return Equals(left, right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(MoveBase left, MoveBase right)
        {
            return !Equals(left, right);
        }

        #endregion

        #endregion
    }
}
