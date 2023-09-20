using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.DataStructures;
using System.Runtime.CompilerServices;

namespace Engine.Book.Services
{
    public class BookService : IBookService
    {
        private readonly BookMoves _defaultBook;
        private readonly Dictionary<string, BookMoves> _moves;

        public BookService()
        {
            _defaultBook = new BookMoves();
            _moves = new Dictionary<string, BookMoves>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string key, BookMoves bookMoves)
        {
            _moves.Add(key, bookMoves);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BookMoves GetBook(ref MoveKeyList history)
        {
            history.Order();

            var key = history.AsStringKey();

            return _moves.TryGetValue(key, out var moves) ? moves : _defaultBook;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BookMoves GetBook(string key)
        {
            return _moves[key];
        }
    }
}
