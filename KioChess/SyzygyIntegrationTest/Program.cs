using DataAccess.Syzygy;

internal class Program
{
    private static int _failed;

    private static int Main(string[] args)
    {
        string devPath = @"C:\Dev";
        string repositoryPath = Path.Combine(devPath, "Kio-Chess", "KioChess");
        string tablesPath = Path.Combine(devPath, "ChessDB", "TB");
        string rtbw = Path.Combine(tablesPath, "RTBW");
        string rtbz = Path.Combine(tablesPath, "RTBZ");

        string fathomDllPath = Path.Combine(repositoryPath, "Fathom", "Build", "fathomDll.dll");

        var service = SyzygyService.Instance;

        Check("Initialize", service.Initialize(fathomDllPath, string.Join(Path.PathSeparator, rtbw, rtbz)));
        if (!service.IsInitialized)
        {
            Console.WriteLine("Syzygy initialization failed");
            return 1;
        }

        Console.WriteLine($"Largest table: {service.LargestTable}");
        Check("Largest >= 5", service.LargestTable >= 5);

        ExpectWdl("KQvK white to move wins", "4k3/8/8/8/8/8/8/3QK3 w - - 0 1", TbResult.Win);
        ExpectWdl("KQvK black to move loses", "4k3/8/8/8/8/8/8/3QK3 b - - 0 1", TbResult.Loss);
        ExpectWdl("KRvK white wins", "4k3/8/8/8/8/8/8/R3K3 w - - 0 1", TbResult.Win);
        ExpectWdl("KBvK draw", "4k3/8/8/8/8/8/8/4KB2 w - - 0 1", TbResult.Draw);
        ExpectWdl("KNNvK draw", "4k3/8/8/8/8/8/8/3NKN2 w - - 0 1", TbResult.Draw);
        ExpectWdl("KPvK winning (pawn on 7th)", "8/4P3/8/8/8/k7/8/4K3 w - - 0 1", TbResult.Win);

        var root = service.ProbeRoot(Parse("4k3/8/8/8/8/8/8/3QK3 w - - 0 1"));
        Check("Root KQvK valid", root.IsValid);
        Check("Root KQvK win", root.Wdl == TbResult.Win);
        Check("Root KQvK dtz > 0", root.Dtz > 0);
        Check("Root KQvK move from queen or king square", root.From == Sq("d1") || root.From == Sq("e1"));

        var promo = service.ProbeRoot(Parse("8/4P3/8/8/8/k7/8/4K3 w - - 0 1"));
        Check("Root KPvK valid win", promo.IsValid && promo.Wdl == TbResult.Win);

        var stalemate = service.ProbeRoot(Parse("7k/5Q2/6K1/8/8/8/8/8 b - - 0 1"));
        Check("Stalemate reported invalid", !stalemate.IsValid);

        Check("Castling rights not probable", !service.CanProbe(Parse("4k2r/8/8/8/8/8/8/R3K3 w KQkq - 0 1")));
        Check("Too many pieces not probable",
            !service.CanProbe(Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1")));

        RunTableTests(rtbw);

        service.Dispose();

        Console.WriteLine(_failed == 0 ? "All tests passed" : $"{_failed} test(s) failed");
        return _failed == 0 ? 0 : 1;
    }

    private const int PositionsPerTableAndSide = 150;
    private const int MaxReportedFailures = 25;
    private static int _reportedFailures;

    private static void Fail(string message)
    {
        _failed++;
        if (_reportedFailures++ < MaxReportedFailures) Console.WriteLine($"FAIL: {message}");
    }

    /// <summary>
    /// For every 3-5 piece table found in the RTBW folder, probes random legal positions for both sides to move and checks:
    /// WDL probe succeeds; root WDL equals WDL probe; DTZ is 0 only for draws; and the root move leads to the
    /// expected WDL for the opponent (Win -> Loss, Loss -> Win, Draw -> Draw).
    /// </summary>
    private static void RunTableTests(string rtbwPath)
    {
        var service = SyzygyService.Instance;
        var rnd = new Random(12345);
        var wdlCounts = new int[5];
        var rootCounts = new int[5];
        int tables = 0, positions = 0, terminals = 0, maxDtz = 0, tablesWithAllOutcomes = 0;

        foreach (string file in Directory.GetFiles(rtbwPath, "*.rtbw").OrderBy(f => f))
        {
            string name = Path.GetFileNameWithoutExtension(file);
            var sides = name.Split('v');
            if (sides.Length != 2 || name.Length < 3 || name.Length > 6) continue;
            tables++;
            int tableMask = 0;

            foreach (bool whiteToMove in new[] { true, false })
            {
                for (int i = 0; i < PositionsPerTableAndSide; i++)
                {
                    var board = RandomBoard(rnd, sides[0], sides[1], whiteToMove);
                    if (board == null) continue;
                    positions++;
                    string desc = $"{name} {(whiteToMove ? "w" : "b")} {BoardToString(board)}";

                    var pos = ToPosition(board, whiteToMove);
                    if (!service.TryProbeWdl(pos, out var wdl))
                    {
                        Fail($"WDL probe failed: {desc}");
                        continue;
                    }

                    wdlCounts[(int)wdl]++;
                    tableMask |= 1 << (int)wdl;

                    var root = service.ProbeRoot(pos);
                    if (!root.IsValid)
                    {
                        terminals++;
                        int king = Array.IndexOf(board, whiteToMove ? 'K' : 'k');
                        var expectedTerminal = IsAttacked(board, king, !whiteToMove) ? TbResult.Loss : TbResult.Draw;
                        if (wdl != expectedTerminal) Fail($"Terminal position expected {expectedTerminal} got {wdl}: {desc}");
                        continue;
                    }

                    rootCounts[(int)root.Wdl]++;
                    maxDtz = Math.Max(maxDtz, root.Dtz);

                    if (root.Wdl != wdl) Fail($"Root WDL {root.Wdl} != probe WDL {wdl}: {desc}");
                    if (wdl == TbResult.Draw ? root.Dtz != 0 : root.Dtz <= 0) Fail($"Unexpected DTZ {root.Dtz} for {wdl}: {desc}");
                    if (board[root.From] == '\0' || char.IsUpper(board[root.From]) != whiteToMove)
                    {
                        Fail($"Root move {root.From}->{root.To} is not a move of the side to move: {desc}");
                        continue;
                    }

                    var next = (char[])board.Clone();
                    char piece = next[root.From];
                    next[root.From] = '\0';
                    if (root.Promotion != SyzygyRootResult.PromotionNone)
                    {
                        char promoted = root.Promotion switch { 1 => 'Q', 2 => 'R', 3 => 'B', _ => 'N' };
                        piece = whiteToMove ? promoted : char.ToLowerInvariant(promoted);
                    }
                    next[root.To] = piece;

                    if (!service.TryProbeWdl(ToPosition(next, !whiteToMove), out var after))
                    {
                        Fail($"WDL probe failed after root move {root.From}->{root.To}: {desc}");
                        continue;
                    }

                    bool ok = wdl switch
                    {
                        TbResult.Win => after == TbResult.Loss,
                        TbResult.Loss => after == TbResult.Win || after == TbResult.CursedWin,
                        TbResult.Draw => after == TbResult.Draw,
                        _ => true
                    };
                    if (!ok) Fail($"Root move {root.From}->{root.To} from {wdl} leads to {after}: {desc}");
                }
            }

            if ((tableMask & 0b00111) == 0b00111 || tableMask == 0b00100 || (tableMask & 0b10001) == 0b10001) tablesWithAllOutcomes++;
        }

        Console.WriteLine($"Tables: {tables}, positions: {positions}, terminal: {terminals}, max DTZ: {maxDtz}");
        Console.WriteLine($"WDL probe   : Loss={wdlCounts[0]} BlessedLoss={wdlCounts[1]} Draw={wdlCounts[2]} CursedWin={wdlCounts[3]} Win={wdlCounts[4]}");
        Console.WriteLine($"Root results: Loss={rootCounts[0]} BlessedLoss={rootCounts[1]} Draw={rootCounts[2]} CursedWin={rootCounts[3]} Win={rootCounts[4]}");

        Check("Found 3-5 piece tables", tables > 0);
        Check("Probed positions", positions > 0);
        Check("Loss outcome observed", wdlCounts[(int)TbResult.Loss] > 0);
        Check("Draw outcome observed", wdlCounts[(int)TbResult.Draw] > 0);
        Check("Win outcome observed", wdlCounts[(int)TbResult.Win] > 0);
        Check("Cursed win / blessed loss observed", wdlCounts[(int)TbResult.CursedWin] + wdlCounts[(int)TbResult.BlessedLoss] > 0);
        Check("All table consistency checks", _reportedFailures == 0);
    }

    private static char[]? RandomBoard(Random rnd, string white, string black, bool whiteToMove)
    {
        for (int attempt = 0; attempt < 1000; attempt++)
        {
            var board = new char[64];
            bool ok = true;
            foreach (char c in white + black.ToLowerInvariant())
            {
                bool isPawn = char.ToUpperInvariant(c) == 'P';
                int sq = isPawn ? 8 + rnd.Next(48) : rnd.Next(64);
                if (board[sq] != '\0') { ok = false; break; }
                board[sq] = char.IsUpper(c) ? c : c;
            }
            if (!ok) continue;

            // pieces of the black side are lower-case; white upper-case
            int whiteKing = Array.IndexOf(board, 'K');
            int blackKing = Array.IndexOf(board, 'k');
            if (whiteKing < 0 || blackKing < 0) continue;

            // side not to move must not be in check
            int idle = whiteToMove ? blackKing : whiteKing;
            if (IsAttacked(board, idle, whiteToMove)) continue;
            return board;
        }
        return null;
    }

    private static readonly int[] KnightDr = { -2, -2, -1, -1, 1, 1, 2, 2 };
    private static readonly int[] KnightDf = { -1, 1, -2, 2, -2, 2, -1, 1 };
    private static readonly int[] Dr = { -1, -1, -1, 0, 0, 1, 1, 1 };
    private static readonly int[] Df = { -1, 0, 1, -1, 1, -1, 0, 1 };

    private static bool IsAttacked(char[] b, int sq, bool byWhite)
    {
        int r = sq / 8, f = sq % 8;

        int pr = byWhite ? r - 1 : r + 1;
        char pawn = byWhite ? 'P' : 'p';
        if (pr >= 0 && pr < 8)
        {
            if (f > 0 && b[pr * 8 + f - 1] == pawn) return true;
            if (f < 7 && b[pr * 8 + f + 1] == pawn) return true;
        }

        char knight = byWhite ? 'N' : 'n';
        for (int i = 0; i < 8; i++)
        {
            int nr = r + KnightDr[i], nf = f + KnightDf[i];
            if (nr >= 0 && nr < 8 && nf >= 0 && nf < 8 && b[nr * 8 + nf] == knight) return true;
        }

        for (int i = 0; i < 8; i++)
        {
            bool diagonal = Dr[i] != 0 && Df[i] != 0;
            int nr = r + Dr[i], nf = f + Df[i];
            bool adjacent = true;
            while (nr >= 0 && nr < 8 && nf >= 0 && nf < 8)
            {
                char c = b[nr * 8 + nf];
                if (c != '\0')
                {
                    if (char.IsUpper(c) == byWhite)
                    {
                        char u = char.ToUpperInvariant(c);
                        if (u == 'Q' || (diagonal ? u == 'B' : u == 'R') || (adjacent && u == 'K')) return true;
                    }
                    break;
                }
                adjacent = false;
                nr += Dr[i];
                nf += Df[i];
            }
        }

        return false;
    }

    private static SyzygyPosition ToPosition(char[] b, bool whiteToMove)
    {
        ulong white = 0, black = 0, kings = 0, queens = 0, rooks = 0, bishops = 0, knights = 0, pawns = 0;
        for (int sq = 0; sq < 64; sq++)
        {
            char c = b[sq];
            if (c == '\0') continue;
            ulong bit = 1UL << sq;
            if (char.IsUpper(c)) white |= bit; else black |= bit;
            switch (char.ToLowerInvariant(c))
            {
                case 'k': kings |= bit; break;
                case 'q': queens |= bit; break;
                case 'r': rooks |= bit; break;
                case 'b': bishops |= bit; break;
                case 'n': knights |= bit; break;
                case 'p': pawns |= bit; break;
            }
        }
        return new SyzygyPosition(white, black, kings, queens, rooks, bishops, knights, pawns, 0, 0, 0, whiteToMove);
    }

    private static string BoardToString(char[] b)
    {
        var parts = new List<string>();
        for (int sq = 0; sq < 64; sq++)
        {
            if (b[sq] != '\0') parts.Add($"{b[sq]}{(char)('a' + sq % 8)}{sq / 8 + 1}");
        }
        return string.Join(" ", parts);
    }

    private static void ExpectWdl(string name, string fen, TbResult expected)
    {
        bool ok = SyzygyService.Instance.TryProbeWdl(Parse(fen), out var result);
        Check($"WDL {name} (got {result})", ok && result == expected);
    }

    private static void Check(string name, bool condition)
    {
        if (!condition) _failed++;
        Console.WriteLine($"{(condition ? "PASS" : "FAIL")}: {name}");
    }

    private static int Sq(string s) => (s[1] - '1') * 8 + (s[0] - 'a');

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
        uint ep = parts[3] == "-" ? 0u : (uint)Sq(parts[3]);
        return new SyzygyPosition(white, black, kings, queens, rooks, bishops, knights, pawns,
            uint.Parse(parts[4]), castling, ep, parts[1] == "w");
    }
}
