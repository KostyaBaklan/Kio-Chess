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
        private readonly PopularMoves _default;
        private readonly Dictionary<string, BookMoves> _moves;
        private readonly Dictionary<string, PopularMoves> _popularMoves;
        private List<short> _movesList;

        public BookService(IConfigurationProvider configuration)
        {
            _suggestedThreshold = configuration.BookConfiguration.SuggestedThreshold;
            _default = new PopularMoves();
            _popularMoves = new Dictionary<string, PopularMoves>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string key, BookMoves bookMoves)
        {
            _moves.Add(key, bookMoves);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string key, PopularMoves bookMoves)
        {
            _popularMoves.Add(key, bookMoves);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PopularMoves GetBook(ref MoveKeyList history)
        {
            history.Order();

            var key = history.AsStringKey();

            return _popularMoves.TryGetValue(key, out var moves) ? moves : _default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<MoveBase> GetOpeningMoves(IMoveProvider moveProvider)
        {
            return _movesList.Select(moveProvider.Get).ToList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetOpening(List<HistoryTotalItem> open)
        {
            _movesList = open.Select(o=>new BookMove { Id = o.Key, Value = o.Total })
                .OrderByDescending(m=>m.Value)
                .Select(m=>m.Id)
                .Take(10)
                .ToList();
        }
    }
}
