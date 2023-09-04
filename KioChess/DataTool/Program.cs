using Engine.Book.Interfaces;
using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using System.Diagnostics;

internal class Program
{
    private static Dictionary<string, byte> _squares = new Dictionary<string, byte>();
    private static Dictionary<string, byte> _pieces = new Dictionary<string, byte>();
    private static Dictionary<string, string> _subPieces = new Dictionary<string, string>();
    private static IDataAccessService _dataAccessService;

    private static void Main(string[] args)
    {
        for (byte i = 0; i < 64; i++)
        {
            var k = i.AsString().ToLower();
            _squares[k] = i;
        }

        for (byte i = 0; i < 12; i++)
        {
            var p = i.AsEnumString();
            _pieces[p] = i;
        }

        _subPieces = new Dictionary<string, string>
        {
            {"N","Knight" },{"n","Knight" },
            {"B","Bishop" },{"b","Bishop" },
            {"R","Rook" },{"r","Rook" },
            {"Q","Queen" },{"q","Queen" },
            {"K","King" },{"k","King" }
        };

        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        _dataAccessService = Boot.GetService<IDataAccessService>();

        var bookService = Boot.GetService<IBookService>();

        try
        {
            IPosition position = new Position();

            _dataAccessService.Connect();

            foreach(var line in File.ReadLines(@"C:\Dev\Temp\BasicOpenings_1_2.csv").Skip(1))
            {
                var parts = line.Split(',');
                short id = short.Parse(parts[0]);
                var moves = parts[1].Split(" ");

                for (int i = 0; i < moves.Length; i++)
                {
                    string m = moves[i];
                    MoveBase move;

                    if (i % 2 == 0)
                    {
                        move = ParseWhiteMove(m, position);
                    }
                    else
                    {
                        move = ParseBlackMove(m, position);
                    }

                    if (i != 0)
                        position.Make(move);
                    else
                        position.MakeFirst(move);
                }

                SaveOpening(id);

                foreach (var item in moves)
                {
                    position.UnMake();
                }
            }
        }
        finally
        {

            _dataAccessService.Disconnect();
        }
        timer.Stop();

        Console.WriteLine();

        Console.WriteLine(timer.Elapsed);
        Console.WriteLine();

        Console.WriteLine($"Finished !!!");

        Console.ReadLine();
    }

    private static MoveBase ParseWhiteMove(string m, IPosition position)
    {
        string squareString = null;
        string pieceString = null;
        if (m.Length == 2)
        {
            squareString = m;
            pieceString = "WhitePawn";
        }
        else if (m.Length == 3)
        {
            squareString = m.Substring(1);
            var p = m.Substring(0, 1);
            pieceString = $"White{_subPieces[p]}";
        }
        else if (m.Length == 4)
        {
            squareString = m;
            pieceString = $"WhitePawn";
        }
        else if (m.Length == 4)
        {
            squareString = m;
            pieceString = $"WhitePawn";
        }
        else
        {

        }
        var square = _squares[squareString];
        var piece = _pieces[pieceString];
        var moves = position.GetMoves(piece, square);
        return moves[0];
    }

    private static MoveBase ParseBlackMove(string m, IPosition position)
    {
        string squareString = null;
        string pieceString = null;
        if (m.Length == 2)
        {
            squareString = m;
            pieceString = "BlackPawn";
        }
        else if (m.Length == 3)
        {
            squareString = m.Substring(1);
            var p = m.Substring(0, 1);
            pieceString = $"Black{_subPieces[p]}";
        }
        else if (m.Length == 4)
        {
            squareString = m;
            pieceString = $"BlackPawn";
        }
        else if (m.Length == 4)
        {
            squareString = m;
            pieceString = $"BlackPawn";
        }
        else
        {

        }
        var square = _squares[squareString];
        var piece = _pieces[pieceString];
        var moves = position.GetMoves(piece, square);
        return moves[0];
    }

    private static void SaveOpening(short id)
    {
        var moveHistory = Boot.GetService<IMoveHistoryService>();

        MoveKeyList moveKeys = new short[2];

        moveHistory.GetSequence(ref moveKeys);

        moveKeys.Order();

        var key = moveKeys.AsKey();

        _dataAccessService.SaveOpening(key, id);
    }
}