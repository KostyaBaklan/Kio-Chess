using DataAccess.Syzygy;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Strategies.Base;
using Engine.Models.Enums;
using Engine.Models.Moves;
using Engine.Services;
using Engine.Services.Syzygy;
using System.Reflection;

/// <summary>
/// Engine-level edge cases for castling rights, en-passant targets and the 50-move rule.
/// Every test first removes all moves (Position.Clear), wipes the board, installs its own piece placement and
/// initial castling rights, and then plays real engine moves so MoveHistoryService, Position and the
/// tablebase service are exercised exactly as in search.
/// </summary>
internal static partial class EngineEdgeCaseTests
{
    private static int _failed;

    private static Position _position = null!;
    private static Board _board = null!;
    private static MoveHistoryService _history = null!;
    private static MoveProvider _moves = null!;
    private static ITablebaseService _tablebase = null!;
    private static MethodInfo _setRights = null!;

    private const string PieceLetters = "PNBRQKpnbrqk";

    public static int Run()
    {
        _failed = 0;
        var appDb = Boot.GetService<DataAccess.Interfaces.IAppDbService>();
        appDb.Connect();
        if (!Engine.Models.Hash.MoveHashSequenceHasher.IsInitialized)
            Engine.Models.Hash.MoveHashSequenceHasher.Initialize(appDb.GetAllMoveHashValues());
        _position = new Position();
        _board = _position.GetBoard();
        _history = Boot.GetService<MoveHistoryService>();
        _history.CreateSequenceCache(new Dictionary<UInt128, Engine.Dal.Models.PopularMoves>());
        _history.CreatePopularCache(new Dictionary<UInt128, Engine.DataStructures.Moves.MoveHistory[]>());
        _moves = Boot.GetService<MoveProvider>();
        _tablebase = Boot.GetService<ITablebaseService>();
        _setRights = typeof(MoveHistoryService).GetMethod("SetInitialCastlingRights",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        Console.WriteLine("--- Engine edge cases: en passant ---");
        EnPassantTests();
        Console.WriteLine("--- Engine edge cases: castling ---");
        CastlingTests();
        Console.WriteLine("--- Engine edge cases: 50-move rule ---");
        FiftyMoveTests();
        Console.WriteLine("--- Engine edge cases: tablebase service ---");
        TablebaseServiceTests();
        Console.WriteLine("--- Engine edge cases: strategy ---");
        StrategyTests();
        Console.WriteLine("--- Engine edge cases: search cutoffs ---");
        CutoffTests();
        Console.WriteLine("--- Engine edge cases: tablebase boundary ordering ---");
        BoundaryOrderingTests();
        Console.WriteLine("--- Engine edge cases: DTZ root ---");
        DtzTests();
        Console.WriteLine("--- Engine edge cases: transposition ---");
        TranspositionTests();

        Reset(true, true, true, true);
        return _failed;
    }

    #region Harness

    private static void Check(string name, bool condition)
    {
        if (!condition) _failed++;
        Console.WriteLine($"{(condition ? "PASS" : "FAIL")}: {name}");
    }

    private static byte Sq(string s) => (byte)((s[1] - '1') * 8 + (s[0] - 'a'));

    /// <summary>Removes every move, wipes the board and sets the initial castling rights.</summary>
    private static void Reset(bool whiteSmall, bool whiteBig, bool blackSmall, bool blackBig)
    {
        _position.Clear();

        for (byte sq = 0; sq < 64; sq++)
        {
            for (byte piece = 0; piece < 12; piece++)
            {
                if (!_board.GetPieceBits(piece).IsSet(sq)) continue;
                if (piece < Pieces.BlackPawn) _board.RemoveWhite(piece, sq);
                else _board.RemoveBlack(piece, sq);
            }
        }

        _setRights.Invoke(_history, [whiteSmall, whiteBig, blackSmall, blackBig]);
    }

    /// <summary>Installs pieces given as e.g. "Ke1 Ra1 pd7" (upper case = white, lower case = black).</summary>
    private static void Place(string pieces)
    {
        foreach (string token in pieces.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            byte piece = (byte)PieceLetters.IndexOf(token[0]);
            byte square = Sq(token.Substring(1));
            if (piece < Pieces.BlackPawn) _board.AddWhite(piece, square);
            else _board.AddBlack(piece, square);
        }
    }

    private static void Setup(string pieces, bool ws = true, bool wb = true, bool bs = true, bool bb = true)
    {
        Reset(ws, wb, bs, bb);
        Place(pieces);
    }

    private static MoveBase Find(string uci)
    {
        byte from = Sq(uci.Substring(0, 2));
        byte to = Sq(uci.Substring(2, 2));
        char promo = uci.Length > 4 ? char.ToLower(uci[4]) : ' ';
        byte piece = _board.GetPiece(from);

        foreach (MoveBase move in _moves.GetAll())
        {
            if (move.From != from || move.To != to || move.Piece != piece) continue;

            if (promo == ' ')
            {
                if (move.IsPromotion) continue;
            }
            else
            {
                byte promoted = move switch
                {
                    PromotionMove pm => pm.PromotionPiece,
                    PromotionAttack pa => pa.PromotionPiece,
                    _ => (byte)255,
                };
                if (promoted == 255 || PieceLetters[promoted] != promo && PieceLetters[promoted] != char.ToUpper(promo))
                    continue;
            }

            if (move.IsLegal()) return move;
        }

        throw new InvalidOperationException($"No legal engine move for {uci}");
    }

    /// <summary>Plays moves in order. The first move of the game goes through MakeFirst, like a real game.</summary>
    private static void Play(params string[] moves)
    {
        foreach (string uci in moves)
        {
            MoveBase move = Find(uci);
            if (!_history.Any()) _position.MakeFirst(move);
            else _position.Make(move);
        }
    }

    private static bool WhiteToMove => _position.GetTurn() == Turn.White;

    private static int Ep => _history.GetEnPassantSquare(WhiteToMove);

    private static string Rights() =>
        $"{(_history.CanDoWhiteSmallCastle() ? "K" : "")}{(_history.CanDoWhiteBigCastle() ? "Q" : "")}" +
        $"{(_history.CanDoBlackSmallCastle() ? "k" : "")}{(_history.CanDoBlackBigCastle() ? "q" : "")}";

    #endregion

    #region En passant

    private static void EnPassantTests()
    {
        // Black double push next to a white pawn creates a target on d6.
        Setup("Kb1 Pe5 kh8 pd7", false, false, false, false);
        Play("b1a1", "d7d5");
        Check("EP: black double push beside white pawn -> d6", Ep == Sq("d6"));
        Check("EP: wrong side to move (null-move guard) -> 0", _history.GetEnPassantSquare(false) == 0);
        Check("EP: reversible count is 0 after pawn push", _history.GetReversibleMovesCount() == 0);

        // En-passant capture itself must not create a target.
        Play("e5d6");
        Check("EP: after en-passant capture -> 0 (white pawn on d6, d5 removed)",
            Ep == 0 && _history.GetEnPassantSquare(true) == 0 && _history.GetEnPassantSquare(false) == 0);
        Check("EP: captured pawn removed", !_board.GetPieceBits(Pieces.BlackPawn).IsSet(Sq("d5")));

        // Unmake restores the exact state.
        _position.UnMake();
        Check("EP: unmake capture restores target d6", Ep == Sq("d6"));
        _position.UnMake();
        Check("EP: unmake double push removes target", _history.GetEnPassantSquare(false) == 0);

        // Redo and let it expire after a different move.
        Play("d7d5");
        Check("EP: redo double push -> d6", Ep == Sq("d6"));
        Play("a1b1", "h8g8");
        Check("EP: expires after two further plies", Ep == 0);

        // Single push never creates a target.
        Setup("Kb1 Pe5 kh8 pd6", false, false, false, false);
        Play("b1a1", "d6d5");
        Check("EP: single pawn push -> 0", Ep == 0);

        // Double push with no adjacent enemy pawn: no actionable target.
        Setup("Kb1 Pa5 kh8 pd7", false, false, false, false);
        Play("b1a1", "d7d5");
        Check("EP: double push without adjacent enemy pawn -> 0", Ep == 0);

        // White double push mirrors the logic: target on the third rank, black to move.
        Setup("Kb1 Pe2 kh8 pd4", false, false, false, false);
        Play("e2e4");
        Check("EP: white double push beside black pawn (d4) -> e3", Ep == Sq("e3"));
        Check("EP: white double push, wrong side query -> 0", _history.GetEnPassantSquare(true) == 0);
        Play("d4e3");
        Check("EP: black en-passant capture -> 0", Ep == 0 && _history.GetEnPassantSquare(true) == 0);
        Check("EP: white pawn captured on e4", !_board.GetPieceBits(Pieces.WhitePawn).IsSet(Sq("e4")));

        // Ordinary quiet and capture moves never create a target.
        Setup("Kb1 Pe4 kh8 pd5", false, false, false, false);
        Play("b1a1", "d5e4");
        Check("EP: ordinary capture -> 0", Ep == 0);
    }

    #endregion

    #region Castling

    private const string CastleSetup = "Ke1 Ra1 Rh1 Nb1 Pb7 ke8 ra8 rh8";

    private static void CastlingTests()
    {
        Setup(CastleSetup);
        Play("b1c3");
        Check("Castle: untouched start keeps KQkq", Rights() == "KQkq" && _history.CanCastle());

        Setup(CastleSetup);
        Play("e1d1");
        Check("Castle: white king move clears both white rights", Rights() == "kq");
        _position.UnMake();
        Play("b1c3");
        Check("Castle: unmake king move restores KQkq", Rights() == "KQkq");

        Setup(CastleSetup);
        Play("b1c3", "e8d8");
        Check("Castle: black king move clears both black rights", Rights() == "KQ");

        Setup(CastleSetup);
        Play("a1a2");
        Check("Castle: white a1 rook move clears Q only", Rights() == "Kkq");
        _position.UnMake();
        Play("h1h2");
        Check("Castle: white h1 rook move clears K only", Rights() == "Qkq");

        Setup(CastleSetup);
        Play("b1c3", "a8a7");
        Check("Castle: black a8 rook move clears q only", Rights() == "KQk");
        Play("c3b1", "h8h7");
        Check("Castle: black h8 rook move clears k too", Rights() == "KQ");
        _position.UnMake();
        Check("Castle: unmake restores k", Rights() == "KQk");
        _position.UnMake();
        _position.UnMake();
        Check("Castle: unmake back to a8 move restores state after b1c3", Rights() == "KQkq");

        // Rook captured on its original square clears the victim's right (and the mover's).
        Setup(CastleSetup);
        Play("h1h8");
        Check("Castle: Rh1xh8 clears K (mover) and k (captured)", Rights() == "Qq");
        _position.UnMake();
        Play("b1c3");
        Check("Castle: unmake rook capture restores KQkq", Rights() == "KQkq");

        Setup(CastleSetup);
        Play("b1c3", "a8a1");
        Check("Castle: black Ra8xa1 clears q (mover) and Q (captured)", Rights() == "Kk");

        // Promotion capture on a corner removes the captured rook's right.
        Setup(CastleSetup);
        Play("b7a8q");
        Check("Castle: b7xa8=Q clears q only", Rights() == "KQk");
        _position.UnMake();
        Play("b1c3");
        Check("Castle: unmake promotion capture restores KQkq", Rights() == "KQkq");

        // Non-corner capture or quiet move to a corner does not clear anything.
        Setup("Ke1 Ra1 Rh1 Nb1 ke8 ra8 rh8 pg2");
        Play("h1h2");
        Check("Castle: only the rook's own right cleared by Rh1-h2", Rights() == "Qkq");

        // Authoritative initial rights come from the loader, not an assumption of KQkq.
        Setup(CastleSetup, false, false, false, false);
        Play("b1c3");
        Check("Castle: initial rights none -> none, CanCastle false", Rights() == "" && !_history.CanCastle());

        Setup(CastleSetup, true, false, false, false);
        Play("b1c3");
        Check("Castle: initial only K stays only K", Rights() == "K" && _history.CanCastle());

        Setup(CastleSetup, false, false, false, true);
        Play("b1c3", "e8d8");
        Check("Castle: initial only q, king move clears it", Rights() == "" && !_history.CanCastle());

        // A rook that left and returned must not regain the right.
        Setup("Ke1 Rh1 Nb1 ke8 ra8", true, false, false, false);
        Play("h1h2", "a8a7", "h2h1", "a7a8");
        Check("Castle: rook away and back does not restore right", Rights() == "");

        // Initial rights apply to the first move too: first move by a rook without the right keeps state exact.
        Setup(CastleSetup, true, true, true, true);
        Play("h1h2");
        Check("Castle: first move rook h1 clears K immediately", Rights() == "Qkq");
    }

    #endregion

    #region 50-move rule

    private static void FiftyMoveTests()
    {
        Setup("Ke1 ke8", false, false, false, false);
        Play("e1d1");
        Check("50: first quiet move -> count 1", _history.GetReversibleMovesCount() == 1);
        Play("e8d8");
        Check("50: second quiet move -> count 2", _history.GetReversibleMovesCount() == 2);

        Reset(false, false, false, false);
        Place("Ke1 ke8");
        string[] cycle = ["e1d1", "e8d8", "d1e1", "d8e8"];
        for (int i = 0; i < 96; i++) Play(cycle[i % 4]);
        Check("50: 96 reversible plies -> not yet draw", _history.GetReversibleMovesCount() == 96 && !_history.IsFiftyMoves());
        for (int i = 0; i < 4; i++) Play(cycle[i % 4]);
        Check("50: 100 reversible plies -> fifty-move draw", _history.GetReversibleMovesCount() == 100 && _history.IsFiftyMoves());

        // Pawn move and capture reset the counter; unmake restores it.
        Setup("Ke1 Pa2 Nb1 ke8 nb8 ra8", false, false, false, false);
        Play("e1d1", "e8d8", "d1e1", "d8e8");
        Check("50: four quiet plies -> 4", _history.GetReversibleMovesCount() == 4);
        Play("a2a3");
        Check("50: pawn move resets to 0", _history.GetReversibleMovesCount() == 0);
        Play("e8d8", "b1c3");
        Check("50: quiet plies count again from 0", _history.GetReversibleMovesCount() == 2);
        _position.UnMake();
        _position.UnMake();
        Check("50: unmake restores 0", _history.GetReversibleMovesCount() == 0);
        _position.UnMake();
        Check("50: unmake pawn move restores 4", _history.GetReversibleMovesCount() == 4);

        Setup("Ke1 Rh1 ke8 rh8", false, false, false, false);
        Play("e1d1", "e8d8");
        Play("h1h8");
        Check("50: capture resets to 0", _history.GetReversibleMovesCount() == 0);

        // Native WDL API cannot account for a nonzero clock: the managed layers must refuse it.
        var service = SyzygyService.Instance;
        var clockZero = Parse("4k3/8/8/8/8/8/8/3QK3 w - - 0 1");
        var clockFive = Parse("4k3/8/8/8/8/8/8/3QK3 w - - 5 1");
        Check("50: native WDL probe works at clock 0", service.TryProbeWdl(clockZero, out var zeroResult) && zeroResult == TbResult.Win);
        Check("50: native WDL probe refused at clock 5", !service.TryProbeWdl(clockFive, out _));

        // Root probing uses the real clock: a win becomes a cursed win once the clock plus DTZ exceeds 100.
        var rootZero = service.ProbeRoot(clockZero);
        var rootHigh = service.ProbeRoot(Parse("4k3/8/8/8/8/8/8/3QK3 w - - 99 1"));
        Check("50: root at clock 0 is Win", rootZero.IsValid && rootZero.Wdl == TbResult.Win);
        Check("50: root at clock 99 is CursedWin (DTZ + 99 > 100)",
            rootHigh.IsValid && rootHigh.Wdl == TbResult.CursedWin);
        Check("50: root DTZ identical regardless of clock", rootZero.Dtz == rootHigh.Dtz);
    }

    #endregion

    #region Tablebase service

    private static void TablebaseServiceTests()
    {
        if (!_tablebase.IsEnabled)
        {
            Check("Tablebase service enabled", false);
            return;
        }

        var service = SyzygyService.Instance;

        // KPvKP with an actionable en-passant capture: service must pass the exact target to Fathom.
        Setup("Kb1 Pe5 kh8 pd7", false, false, false, false);
        Play("b1a1", "d7d5");
        int ep = Ep;
        Check("TB: en-passant target d6 available", ep == Sq("d6"));
        Check("TB: no castling rights remain", !_history.CanCastle());

        service.TryProbeWdl(Parse("7k/8/8/3pP3/8/8/8/K7 w - d6 0 1"), out var nativeEp);
        service.TryProbeWdl(Parse("7k/8/8/3pP3/8/8/8/K7 w - - 0 1"), out var nativeNoEp);

        bool withoutEp = _tablebase.TryProbeWdl(_board, true, 0, 0, out var serviceNoEp);
        bool withEp = _tablebase.TryProbeWdl(_board, true, 0, ep, out var serviceEp);
        Check($"TB: service no-EP result matches native (got {serviceNoEp}, native {nativeNoEp})", withoutEp && serviceNoEp == nativeNoEp);
        Check($"TB: service EP result matches native (got {serviceEp}, native {nativeEp})", withEp && serviceEp == nativeEp);

        // Probe again in the opposite order: cache entries for EP and non-EP must not collide.
        bool withEpAgain = _tablebase.TryProbeWdl(_board, true, 0, ep, out var serviceEp2);
        bool withoutEpAgain = _tablebase.TryProbeWdl(_board, true, 0, 0, out var serviceNoEp2);
        Check("TB: cached EP result unchanged", withEpAgain && serviceEp2 == nativeEp);
        Check("TB: cached non-EP result unchanged", withoutEpAgain && serviceNoEp2 == nativeNoEp);

        // Side to move is part of the key.
        service.TryProbeWdl(Parse("7k/8/8/3pP3/8/8/8/K7 b - - 0 1"), out var nativeBlack);
        bool black = _tablebase.TryProbeWdl(_board, false, 0, 0, out var serviceBlack);
        Check("TB: black-to-move result matches native and is not shared with white", black && serviceBlack == nativeBlack);

        // Clock handling: any nonzero clock is refused before the cache is consulted, even after a cached zero-clock hit.
        Check("TB: nonzero rule50 refused (cached zero-clock entry must not leak)",
            !_tablebase.TryProbeWdl(_board, true, 1, ep, out _) && !_tablebase.TryProbeWdl(_board, true, 50, 0, out _));

        // Root probing keeps the real clock and the exact en-passant target.
        bool rootOk = _tablebase.TryProbeRoot(_board, true, 0, ep, out var root);
        var nativeRoot = service.ProbeRoot(Parse("7k/8/8/3pP3/8/8/8/K7 w - d6 0 1"));
        Check("TB: root with EP matches native", rootOk == nativeRoot.IsValid && root.From == nativeRoot.From
            && root.To == nativeRoot.To && root.Wdl == nativeRoot.Wdl && root.IsEnPassant == nativeRoot.IsEnPassant);

        bool rootHighOk = _tablebase.TryProbeRoot(_board, true, 99, ep, out var rootHigh);
        var nativeHigh = service.ProbeRoot(Parse("7k/8/8/3pP3/8/8/8/K7 w - d6 99 1"));
        Check("TB: root with clock 99 matches native (clock not cached across calls)",
            rootHighOk == nativeHigh.IsValid && rootHigh.Wdl == nativeHigh.Wdl && rootHigh.Dtz == nativeHigh.Dtz);

        // Castling rights block probing at the search level (gate lives on MoveHistoryService.CanCastle).
        Setup("Ke1 Rh1 Nb1 ke8", true, false, false, false);
        Play("b1c3");
        Check("TB: untouched king and rook keep the right, which blocks probing", _history.CanCastle());
        Play("e8d8", "h1h2");
        Check("TB: rook move clears the last right, so probing is allowed", !_history.CanCastle());

        Setup("Ke1 Rh1 ke8 rh8", false, false, false, false);
        Play("e1d1", "e8d8", "h1h8");
        Check("TB: after capture clock is 0 and the position is probeable",
            _history.GetReversibleMovesCount() == 0 && _tablebase.TryProbeWdl(_board, false, 0, 0, out _));
        Play("d8c7");
        Check("TB: next quiet move gives clock 1, WDL refused",
            _history.GetReversibleMovesCount() == 1 && !_tablebase.TryProbeWdl(_board, true, 1, 0, out _));
    }
    #endregion

    #region Strategy

    private static StrategyBase NewStrategy() =>
        Boot.GetService<IStrategyFactory>().GetStrategy(4, _position, "lmrd");

    private static void Shuffle(string[] cycle, int rounds)
    {
        for (int i = 0; i < rounds; i++) Play(cycle);
    }

    private static void StrategyTests()
    {
        var service = SyzygyService.Instance;

        // Root: KQvK is a win; the strategy must return a tablebase move with a positive score.
        Setup("Ka1 Qd1 ke8", false, false, false, false);
        Shuffle(["a1b1", "e8f8", "b1a1", "f8e8"], 5);
        Play("a1b1", "e8f8");
        var native = service.ProbeRoot(Parse("5k2/8/8/8/8/8/8/1K1Q4 w - - 22 12"));
        var r = NewStrategy().GetResult();
        Check("Strategy: KQvK root returns a move with a winning score", r.Move != null && r.Value > 0 && native.Wdl == TbResult.Win);
        Check("Strategy: root move equals native DTZ move", r.Move != null && r.Move.From == native.From && r.Move.To == native.To);

        // Root: KBvK is a draw and must be scored 0.
        Setup("Ka1 Bc1 ke8", false, false, false, false);
        Shuffle(["a1b1", "e8f8", "b1a1", "f8e8"], 5);
        Play("a1b1", "e8f8");
        r = NewStrategy().GetResult();
        Check("Strategy: KBvK root scores 0", r.Move != null && r.Value == 0);

        // Root: en-passant capture position must match native root move.
        Setup("Kb1 Pe5 kh8 pd7", false, false, false, false);
        Shuffle(["b1a1", "h8g8", "a1b1", "g8h8"], 5);
        Play("b1a1", "d7d5");
        native = service.ProbeRoot(Parse("7k/8/8/3pP3/8/8/8/K7 w - d6 0 1"));
        r = NewStrategy().GetResult();
        Check("Strategy: EP position root matches native move", r.Move != null && native.IsValid
            && r.Move.From == native.From && r.Move.To == native.To && (r.Move is PawnOverAttack) == native.IsEnPassant);

        // Root with a real 50-move clock: black wins KQvK, but at clock 99 the win is cursed and scores 0.
        Setup("Ka1 kh8 qd5", false, false, false, false);
        string[] cycle = ["a1b1", "h8g8", "b1a1", "g8h8"];
        for (int i = 0; i < 96; i++) Play(cycle[i % 4]);
        Play("a1b1", "h8g8", "b1a1");
        Check("Strategy: clock is 99 with black to move", _history.GetReversibleMovesCount() == 99 && !WhiteToMove);
        r = NewStrategy().GetResult();
        Check("Strategy: cursed win at clock 99 scores 0", r.Move != null && r.Value == 0);

        // Same material at clock 0 is a real win for black.
        Setup("Ka1 kh8 qd5", false, false, false, false);
        Shuffle(cycle, 5);
        Play("a1b1");
        r = NewStrategy().GetResult();
        Check("Strategy: same position at clock 1 is a winning root", r.Move != null && r.Value > 0);

        // Castling rights disable root probing; the search must still return a legal move.
        Setup("Ke1 Rh1 Nb1 ke8", true, false, false, false);
        Play("b1c3", "e8d8", "c3e4", "d8c8", "e4g5", "c8b8", "g5h3", "b8a8", "h3f4", "a8a7", "f4d5", "a7b7", "d5b4", "b7c7", "b4a2", "c7d7", "a2c1", "d7e7", "c1b3", "e7f7");
        Check("Strategy: castling rights present, probing disabled", _history.CanCastle());
        r = NewStrategy().GetResult();
        Check("Strategy: search still returns a move with castling rights", r.Move != null);
    }

    #endregion

    #region Helpers

    private static SyzygyPosition Parse(string fen)
    {
        var parts = fen.Split(' ');
        ulong white = 0, black = 0, kings = 0, queens = 0, rooks = 0, bishops = 0, knights = 0, pawns = 0;

        int rank = 7, file = 0;
        foreach (char c in parts[0])
        {
            if (c == '/') { rank--; file = 0; continue; }
            if (char.IsDigit(c)) { file += c - '0'; continue; }

            ulong bit = 1UL << (rank * 8 + file++);
            if (char.IsUpper(c)) white |= bit; else black |= bit;
            switch (char.ToLower(c))
            {
                case 'k': kings |= bit; break;
                case 'q': queens |= bit; break;
                case 'r': rooks |= bit; break;
                case 'b': bishops |= bit; break;
                case 'n': knights |= bit; break;
                case 'p': pawns |= bit; break;
            }
        }

        uint castling = parts[2] == "-" ? 0u : 1u;
        uint ep = parts[3] == "-" ? 0u : Sq(parts[3]);
        return new SyzygyPosition(white, black, kings, queens, rooks, bishops, knights, pawns,
            uint.Parse(parts[4]), castling, ep, parts[1] == "w");
    }

    #endregion
}
