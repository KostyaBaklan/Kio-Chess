using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Book.Services
{
    public class BookService : IBookService
    {
        private readonly short _suggestedThreshold;
        private readonly BookMoves _defaultBook;
        private readonly Dictionary<string, BookMoves> _moves;
        private List<short> _movesList;

        public BookService(IConfigurationProvider configuration)
        {
            _suggestedThreshold = configuration.BookConfiguration.SuggestedThreshold;
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
        public List<MoveBase> GetOpeningMoves(IMoveProvider moveProvider)
        {
            return _movesList.Select(moveProvider.Get).ToList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetOpening(List<HistoryItem> open)
        {
            _movesList = open.Select(o=>new BookMove { Id = o.Key, Value = o.White - o.Black })
                .Where(m=>m.Value > _suggestedThreshold)
                .OrderByDescending(m=>m.Value)
                .Select(m=>m.Id)
                .ToList();
        }
    }
}
