using DataAccess.Syzygy;
using Engine.DataStructures.Moves;
using Engine.Interfaces.Config;
using Engine.Models.Moves;
using Engine.Strategies.Base;
using System.Reflection;

internal static partial class EngineEdgeCaseTests
{
    private static int Mate => (int)Boot.GetService<IConfigurationProvider>().Evaluation.Static.Mate;

    private static int TablebaseWin =>
        Mate - 2 * Boot.GetService<IConfigurationProvider>().GeneralConfiguration.DynamicGameDepth;

    private static void CutoffTests()
    {
        int tbWin = TablebaseWin;
        int bound = Mate - 1;

        Setup("Ka1 Qb2 ke8 rh8", false, false, false, false);
        Play("b2h8");
        var strategy = NewStrategy();
        int value = strategy.SearchBlack(-bound, bound, 0);
        Check($"Cutoff: black loses KQvK at clock 0 with exact distance score (got {value})",
            _history.GetReversibleMovesCount() == 0 && !WhiteToMove && value == _history.GetPly() - tbWin);

        Setup("Ka1 Qd1 Ne7 ke8", false, false, false, false);
        Play("a1b1", "e8e7");
        strategy = NewStrategy();
        value = strategy.SearchWhite(-bound, bound, 0);
        Check($"Cutoff: white wins KQvK at clock 0 with exact distance score (got {value})",
            _history.GetReversibleMovesCount() == 0 && WhiteToMove && value == tbWin - _history.GetPly());

        Play("b1a1", "e7e8");
        value = strategy.SearchWhite(-bound, bound, 0);
        Check($"Cutoff: nonzero clock disables the cutoff (got {value})",
            _history.GetReversibleMovesCount() == 2 && Math.Abs(value) < tbWin);

        Setup("Ke1 Qd1 Ne7 ke8", true, false, false, false);
        Play("d1d2", "e8e7");
        strategy = NewStrategy();
        value = strategy.SearchWhite(-bound, bound, 0);
        Check($"Cutoff: castling rights disable the cutoff (got {value})",
            _history.CanCastle() && _history.GetReversibleMovesCount() == 0 && Math.Abs(value) < tbWin);

        Setup("Ka1 Qb2 Nd2 ke8 rh8 pb7", false, false, false, false);
        Play("a1b1", "b7b6");
        strategy = NewStrategy();
        value = strategy.SearchWhite(-bound, bound, 0);
        Check($"Cutoff: six pieces are outside the tables (got {value})",
            _history.GetReversibleMovesCount() == 0 && Math.Abs(value) < tbWin);
    }

    private static MoveHistoryList BuildList(List<MoveBase> moves)
    {
        var list = new MoveHistoryList();
        foreach (MoveBase move in moves) list.Add(new MoveHistory(move.Key, 0));
        return list;
    }

    private static MoveHistoryList Order(StrategyBase strategy, MoveHistoryList list)
    {
        var method = typeof(StrategyBase).GetMethod("OrderTablebaseBoundary", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var args = new object[] { list };
        method.Invoke(strategy, args);
        return (MoveHistoryList)args[0];
    }

    private static int[] Keys(MoveHistoryList list)
    {
        var keys = new int[list.Count];
        for (int i = 0; i < keys.Length; i++) keys[i] = list[i].Key;
        return keys;
    }

    private static int BoundaryRank(MoveBase move)
    {
        if (!move.IsAttack || move.IsCastle) return 2;

        int rank = 2;
        _position.Make(move);
        bool white = WhiteToMove;
        if (!_history.CanCastle()
            && _tablebase.TryProbeWdl(_board, white, _history.GetReversibleMovesCount(), _history.GetEnPassantSquare(white), out TbResult child))
        {
            rank = child switch
            {
                TbResult.Loss => 4,
                TbResult.BlessedLoss => 3,
                TbResult.Draw => 2,
                TbResult.CursedWin => 1,
                _ => 0,
            };
        }
        _position.UnMake();
        return rank;
    }

    private static void BoundaryOrderingTests()
    {
        if (_tablebase.MaxPieces != 5)
        {
            Check($"Boundary: tables up to 5 pieces required (found {_tablebase.MaxPieces})", false);
            return;
        }

        string[] kingCycle = ["b1a1", "e8d8", "a1b1", "d8e8"];

        Setup("Kb1 Qb2 Nd2 ke8 rh8 pb7", false, false, false, false);
        Shuffle(kingCycle, 1);
        var strategy = NewStrategy();
        var moves = _position.GetAllMoves();
        var list = BuildList(moves);
        int[] ranks = moves.Select(BoundaryRank).ToArray();
        int[] expected = Enumerable.Range(0, moves.Count).OrderByDescending(i => ranks[i]).Select(i => (int)moves[i].Key).ToArray();
        int[] original = Keys(list);

        int[] actual = Keys(Order(strategy, list));
        Check("Boundary: some capture has an exact non-draw class", ranks.Max() > 2);
        Check("Boundary: moves are ordered by exact class, stable within a class", actual.SequenceEqual(expected));
        Check("Boundary: ordering is a permutation (no move lost or duplicated)",
            actual.Length == original.Length && actual.OrderBy(k => k).SequenceEqual(original.OrderBy(k => k)));

        Setup("Kb1 Qb2 Nd2 ke8 rh8 pb7", false, false, false, true);
        Shuffle(["b1a1", "h8h7", "a1b1", "h7h8"], 1);
        strategy = NewStrategy();
        moves = _position.GetAllMoves();
        list = BuildList(moves);
        original = Keys(list);
        Check("Boundary: castling rights present, order unchanged",
            _history.CanCastle() && moves.Any(m => m.IsAttack) && Keys(Order(strategy, list)).SequenceEqual(original));

        Setup("Kb1 Qb2 Nd2 ke8 rh8", false, false, false, false);
        Shuffle(kingCycle, 1);
        strategy = NewStrategy();
        moves = _position.GetAllMoves();
        list = BuildList(moves);
        original = Keys(list);
        Check("Boundary: five pieces, order unchanged",
            moves.Any(m => m.IsAttack) && Keys(Order(strategy, list)).SequenceEqual(original));

        Setup("Kb1 Qb2 Nd2 ke8 rh8 pb7 pa7", false, false, false, false);
        Shuffle(kingCycle, 1);
        strategy = NewStrategy();
        moves = _position.GetAllMoves();
        list = BuildList(moves);
        original = Keys(list);
        Check("Boundary: seven pieces, order unchanged",
            moves.Any(m => m.IsAttack) && Keys(Order(strategy, list)).SequenceEqual(original));
    }
}
