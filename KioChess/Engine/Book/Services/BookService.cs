using DataAccess.Models;
using Engine.Book.Interfaces;
using Engine.Book.Models;
using Engine.DataStructures;
using Engine.Interfaces;
using Engine.Models.Moves;
using System.Runtime.CompilerServices;

namespace Engine.Book.Services
{
    public class BookService : IBookService
    {
        private readonly IPopularMoves _default;
        private List<short> _movesList;
        private readonly Dictionary<string, IPopularMoves> _popularMoves;

        public BookService()
        {
            _default = new PopularMoves0();
            _popularMoves = new Dictionary<string, IPopularMoves>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(string key, IPopularMoves bookMoves)
        {
            _popularMoves.Add(key, bookMoves);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IPopularMoves GetBook(ref MoveKeyList history)
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
        public void SetOpening(List<BookMove> open)
        {
            _movesList = open
                .OrderByDescending(m=>m.Value)
                .Select(m=>m.Id)
                .Take(10)
                .ToList();
        }
    }
}
