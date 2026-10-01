using DataAccess.Syzygy;
using Engine.Models.Boards;
using Engine.Models.Enums;
using Engine.Models.Moves;

internal static partial class EngineEdgeCaseTests
{
    private static string FenToPieces(string fen)
    {
        var sb = new System.Text.StringBuilder();
        int rank = 7, file = 0;
        foreach (char c in fen.Split(' ')[0])
        {
            if (c == '/') { rank--; file = 0; continue; }
            if (char.IsDigit(c)) { file += c - '0'; continue; }
            sb.Append(c).Append((char)('a' + file)).Append((char)('1' + rank)).Append(' ');
            file++;
        }
        return sb.ToString();
    }

    private static void MakeAny(MoveBase move)
    {
        if (!_history.Any()) _position.MakeFirst(move);
        else _position.Make(move);
    }

    private static List<MoveBase> KingMoves(bool white)
    {
        byte king = white ? Pieces.WhiteKing : Pieces.BlackKing;
        var list = new List<MoveBase>();
        foreach (MoveBase m in _moves.GetAll())
        {
            if (m.Piece != king || m.IsAttack || m.IsCastle || !_board.GetPieceBits(king).IsSet(m.From)) continue;
            if (m.IsLegal()) list.Add(m);
        }
        return list;
    }

    private static MoveBase Back(bool white, MoveBase m) =>
        KingMoves(white).FirstOrDefault(x => x.From == m.To && x.To == m.From);

    /// <summary>Four reversible king plies that leave the same position with white to move and clock 4.</summary>
    private static bool Shuffle4()
    {
        foreach (MoveBase w1 in KingMoves(true))
        {
            MakeAny(w1);
            foreach (MoveBase b1 in KingMoves(false))
            {
                _position.Make(b1);
                MoveBase w2 = Back(true, w1);
                if (w2 != null)
                {
                    _position.Make(w2);
                    MoveBase b2 = Back(false, b1);
                    if (b2 != null) { _position.Make(b2); return true; }
                    _position.UnMake();
                }
                _position.UnMake();
            }
            _position.UnMake();
        }
        return false;
    }

    private static int PromotionCode(MoveBase move)
    {
        byte piece = move switch
        {
            PromotionMove pm => pm.PromotionPiece,
            PromotionAttack pa => pa.PromotionPiece,
            _ => (byte)255,
        };
        return piece == 255 ? 0 : "QRBN".IndexOf(char.ToUpper(PieceLetters[piece])) + 1;
    }

    private static string Square(int sq) => $"{(char)('a' + sq % 8)}{(char)('1' + sq / 8)}";

    private static void DtzTests()
    {
        var service = SyzygyService.Instance;

        // Promotion roots: strategy must follow the native DTZ move including the promotion piece.
        var rnd = new Random(12345);
        int checkedPromotions = 0;
        bool sawUnderpromotion = false;
        for (int attempt = 0; attempt < 400000 && checkedPromotions < 3; attempt++)
        {
            int file = rnd.Next(8), wk = rnd.Next(64), bk = rnd.Next(64);
            int pawn = 48 + file;
            if (wk == pawn || bk == pawn || wk == bk || pawn + 8 == wk || pawn + 8 == bk) continue;
            if (Math.Abs(wk % 8 - bk % 8) <= 1 && Math.Abs(wk / 8 - bk / 8) <= 1) continue;
            if (bk == pawn + 7 && file > 0 || bk == pawn + 9 && file < 7) continue;

            string board = $"{Square(wk)} {Square(bk)} {Square(pawn)}";
            string fen = BuildFen(wk, bk, pawn, 0);
            var probe = service.ProbeRoot(Parse(fen));
            if (!probe.IsValid || probe.Promotion == 0 || probe.Wdl == TbResult.Loss) continue;
            if (probe.Promotion == 1 && (attempt % 50 != 0 || sawUnderpromotion && checkedPromotions >= 2)) continue;

            Setup(FenToPieces(fen), false, false, false, false);
            if (!Shuffle4()) continue;

            var native = service.ProbeRoot(Parse(BuildFen(wk, bk, pawn, _history.GetReversibleMovesCount())));
            var r = NewStrategy().GetResult();
            bool ok = r.Move != null && native.IsValid && r.Move.From == native.From && r.Move.To == native.To
                && PromotionCode(r.Move) == native.Promotion;
            Check($"DTZ root promotion ({board}, code {native.Promotion}) matches native", ok);
            checkedPromotions++;
            sawUnderpromotion |= native.Promotion > 1;
        }
        Check("DTZ root: promotion positions were exercised", checkedPromotions > 0);
        Console.WriteLine($"INFO: underpromotion root encountered: {sawUnderpromotion}");

        // Play out KQvK with tablebase moves on both sides: mate must arrive within the initial DTZ.
        Setup("Ka1 Qd1 ke8", false, false, false, false);
        Check("DTZ playout: shuffle set-up", Shuffle4());
        var start = service.ProbeRoot(Parse($"4k3/8/8/8/8/8/8/K2Q4 w - - {_history.GetReversibleMovesCount()} 1"));
        var first = NewStrategy().GetResult();
        Check($"DTZ playout: root score is win distance (value {first.Value}, dtz {start.Dtz})",
            start.Wdl == TbResult.Win && first.Value == TablebaseWin - start.Dtz);

        int plies = 0;
        bool mated = false;
        while (plies < start.Dtz + 4)
        {
            var step = NewStrategy().GetResult();
            if (step.Move == null) break;
            _position.Make(step.Move);
            plies++;
            if (_position.GetAllMoves().Count == 0) { mated = true; break; }
        }
        Check($"DTZ playout: checkmate delivered in {plies} plies (dtz {start.Dtz})", mated && plies <= start.Dtz);

        // Losing side at a low clock scores as a loss; at clock 98 the loss is blessed and scores 0.
        string[] cycle = ["a1b1", "h8g8", "b1a1", "g8h8"];
        Setup("Ka1 kh8 qd5", false, false, false, false);
        Shuffle(cycle, 5);
        var r2 = NewStrategy().GetResult();
        Check($"DTZ root: loss at low clock scores negative (value {r2.Value})", r2.Move != null && r2.Value < 0 && WhiteToMove);

        Setup("Ka1 kh8 qd5", false, false, false, false);
        for (int i = 0; i < 96; i++) Play(cycle[i % 4]);
        Play("a1b1", "h8g8");
        var native2 = service.ProbeRoot(Parse("6k1/8/8/3q4/8/8/8/1K6 w - - 98 50"));
        r2 = NewStrategy().GetResult();
        Check($"DTZ root: blessed loss at clock 98 scores 0 (wdl {native2.Wdl}, value {r2.Value})",
            _history.GetReversibleMovesCount() == 98 && WhiteToMove && native2.Wdl == TbResult.BlessedLoss && r2.Move != null && r2.Value == 0);
    }

    private static string BuildFen(int wk, int bk, int pawn, int clock)
    {
        var grid = new char[64];
        Array.Fill(grid, '.');
        grid[wk] = 'K'; grid[bk] = 'k'; grid[pawn] = 'P';
        var sb = new System.Text.StringBuilder();
        for (int rank = 7; rank >= 0; rank--)
        {
            int empty = 0;
            for (int f = 0; f < 8; f++)
            {
                char c = grid[rank * 8 + f];
                if (c == '.') { empty++; continue; }
                if (empty > 0) { sb.Append(empty); empty = 0; }
                sb.Append(c);
            }
            if (empty > 0) sb.Append(empty);
            if (rank > 0) sb.Append('/');
        }
        return $"{sb} w - - {clock} 1";
    }
}
