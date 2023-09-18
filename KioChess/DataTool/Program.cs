using Engine.Book.Interfaces;
using Engine.DataStructures;
using Engine.DataStructures.Moves;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using Tools.Common;

class OpeningInfo
{
    public string Key { get; set; }
    public string Name { get; set; }
    public List<short> Keys { get; set; }
    public List<string> Moves { get; set; }

    public OpeningInfo()
    {
        Keys= new List<short>();
        Moves= new List<string>();
    }
}

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

            var moveHistory = Boot.GetService<IMoveHistoryService>();

            _dataAccessService.Connect();

            string get = @"  SELECT top 1000 [History]
                                  ,[NextMove]
                                  ,[White]
                                  ,[Draw]
                                  ,[Black]
                              FROM [ChessData].[dbo].[Games] WITH (NOLOCK)
                              WHERE Len([History]) > 0
                              order by Len([History])";

            var results = _dataAccessService.Execute(get, reader=> new
            {
                 History = reader.GetString(0),
                 Next = reader.GetInt16(1),
                 White = reader.GetInt32(2),
                 Draw= reader.GetInt32(3),
                 Black= reader.GetInt32(4),
            }).ToList();

            foreach (var item in results)
            {
                try
                {
                    var shorts = item.History.Split("-").Select(short.Parse).ToArray();

                    byte[] bytes = new byte[2 * shorts.Length];

                    Buffer.BlockCopy(shorts, 0, bytes, 0, bytes.Length);

                    var history = Encoding.UTF8.GetString(bytes);

                    Console.WriteLine($"Shorts = {shorts.Length}, Bytes = {bytes.Length}, History = {history.Length}, {history}");

                    string insert = @$"INSERT INTO [dbo].[Histories] ([History] ,[NextMove] ,[White] ,[Draw] ,[Black]) VALUES (@History,{item.Next},{item.White},{item.Draw},{item.Black})";

                    _dataAccessService.Execute(insert, new string[] { "@History" }, new object[] { bytes });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToFormattedString());
                }
            }

            _dataAccessService.Execute(@$"INSERT INTO [dbo].[Histories] ([History] ,[NextMove] ,[White] ,[Draw] ,[Black]) VALUES (@History,{0},{0},{0},{0})", new string[] { "@History" }, new object[] { new byte[0] });
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
    private static void GenerateMoves(IPosition position, IMoveHistoryService moveHistory)
    {
        Dictionary<string, OpeningInfo> openings = new Dictionary<string, OpeningInfo>();
        Dictionary<string, OpeningInfo> unknown = new Dictionary<string, OpeningInfo>();
        var moves1 = position.GetAllMoves();
        foreach (var m1 in moves1)
        {
            position.MakeFirst(m1);
            ProcessMove(moveHistory, openings, m1, unknown);
            var moves2 = position.GetAllMoves();

            foreach (var m2 in moves2)
            {
                position.Make(m2);

                ProcessMove(moveHistory, openings, m2, unknown);

                var moves3 = position.GetAllMoves();

                foreach (var m3 in moves3)
                {
                    position.Make(m3);

                    ProcessMove(moveHistory, openings, m3, unknown);

                    position.UnMake();
                }

                position.UnMake();
            }

            position.UnMake();
        }

        //SaveOpeningMap(openings, "OpeningMap.txt");
        //SaveOpeningMap(unknown, "UnknownMap.txt");

        ProcessUnknown4(unknown.Values);
    }

    private static void ProcessUnknown4(ICollection<OpeningInfo> values)
    {
    }

    private static void ProcessUnknown3(ICollection<OpeningInfo> values)
    {
        Console.WriteLine(values.Count);
        List<KeyValuePair<string,OpeningInfo>> list = new List<KeyValuePair<string, OpeningInfo>>();
        Dictionary<string, int> map = new Dictionary<string, int>();

        var basicOpenings = File.ReadLines(@"C:\Projects\AI\Kio-Chess\KioChess\DataTool\BasicOpenings.csv").ToHashSet();
        var openingTotal = File.ReadLines(@"C:\Projects\AI\Kio-Chess\KioChess\DataTool\OpeningsTotal.csv")
            .Select(l=>l.Split(','))
            .ToDictionary(k => k[0], v => int.Parse(v[1]));
        var basicOpeningTotal = File.ReadLines(@"C:\Projects\AI\Kio-Chess\KioChess\DataTool\BasicOpeningsTotal.csv")
            .Select(l => l.Split(','))
            .ToDictionary(k => k[0], v => int.Parse(v[1]));

        int count = 0;
        foreach (var info in values)
        {
            var key = info.Key;
            var parts = key.Split('-');
            var k1 = $"{parts[0]}-{parts[2]}";
            var k2 = $"{parts[1]}-{parts[2]}";

            int id1 = _dataAccessService.GetOpeningVariationID(k1);
            int id2 = _dataAccessService.GetOpeningVariationID(k2);

            if(id1 > 0)
            {
                if (id2 > 0)
                {
                    string n1 = _dataAccessService.GetOpeningName(k1);
                    string n2 = _dataAccessService.GetOpeningName(k2);
                    if (!basicOpenings.Contains(n1) && !basicOpenings.Contains(n2))
                    {
                        //if (openingTotal[k1] > openingTotal[k2])
                        //{
                        //    _dataAccessService.SaveOpening(info.Key, id1);
                        //}
                        //else
                        //{
                        //    _dataAccessService.SaveOpening(info.Key, id2);
                        //}

                        //list.Add(new KeyValuePair<string, OpeningInfo>(n1,info));
                    }
                    //else if (!basicOpenings.Contains(n1))
                    //{
                    //    //Console.WriteLine($"{n1}");
                    //    _dataAccessService.SaveOpening(info.Key, id1);
                    //}
                    //else if (!basicOpenings.Contains(n2))
                    //{
                    //    //Console.WriteLine($"{n2}");
                    //    _dataAccessService.SaveOpening(info.Key, id2);
                    //}
                    else
                    {
                        if (basicOpeningTotal[parts[0]] > basicOpeningTotal[parts[1]])
                        {
                            _dataAccessService.SaveOpening(info.Key, id1);
                        }
                        else
                        {
                            _dataAccessService.SaveOpening(info.Key, id2);
                        }

                        Console.WriteLine($"{++count} {n1} x {n2} = {(basicOpeningTotal[parts[0]] > basicOpeningTotal[parts[1]] ?n1:n2)}");
                    }
                }
                else
                {
                    _dataAccessService.SaveOpening(info.Key, id1);
                }
            }
            else
            {
                if (id2 > 0)
                {
                    _dataAccessService.SaveOpening(info.Key, id2);
                }
                else
                {

                }

            }
        }        
    }

    private static void ProcessUnknown2(ICollection<OpeningInfo> values)
    {
        foreach (var info in values)
        {
            var key = info.Key.Substring(0, info.Key.IndexOf($"-{info.Keys[0]}"));
            int id = _dataAccessService.GetOpeningVariationID(key);

            if(id > 0)
            {
                _dataAccessService.SaveOpening(info.Key, id);
                Console.WriteLine($"{info.Key} - {_dataAccessService.GetOpeningName(info.Key)}");
            }
            else
            {
                Console.WriteLine(key);
            }
        }
    }

    private static void SaveOpeningMap(Dictionary<string, OpeningInfo> map, string file)
    {
        File.WriteAllLines(file, map.Select(p => JsonConvert.SerializeObject(p.Value)));
    }

    private static void ProcessMove(IMoveHistoryService moveHistory, Dictionary<string, OpeningInfo> openings, MoveBase m, Dictionary<string, OpeningInfo> unknown)
    {
        MoveKeyList moveKeys = new short[16];

        moveHistory.GetSequence(ref moveKeys);

        moveKeys.Order();

        var key = moveKeys.AsKey();

        var o = _dataAccessService.GetOpeningName(key);

        if (string.IsNullOrWhiteSpace(o))
        {
            ProcessOpeningInfo(m, unknown, key, o);
        }
        else
        {

            ProcessOpeningInfo(m, openings, key, o);
        }
    }

    private static void ProcessOpeningInfo(MoveBase m, Dictionary<string, OpeningInfo> map, string key, string o)
    {
        if (map.TryGetValue(key, out var info))
        {
            info.Keys.Add(m.Key);
            info.Moves.Add(m.ToString());
        }
        else
        {
            map[key] = new OpeningInfo { Key = key, Keys = new List<short> { m.Key }, Moves = new List<string> { m.ToString() }, Name = o };
        }
    }

    private static void ProcessSequences(IPosition position)
    {
        List<KeyValuePair<int, string>> sequences = _dataAccessService.GetSequences();

        foreach (var item in sequences)
        {
            int id = item.Key;
            var moves = item.Value.Split(" ");

            bool isValid = true;

            int j = 0;

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

                if (move == null)
                {
                    Console.WriteLine(item);
                    isValid = false;
                    break;

                }

                if (i != 0)
                    position.Make(move);
                else
                    position.MakeFirst(move);
                j++;
            }

            if (isValid)
            {
                SaveOpening(id);
            }

            for (int i = 0; i < j; i++)
            {
                position.UnMake();
            }
        }
    }

    private static void processBasicOpenings(IPosition position)
    {
        foreach (var l in File.ReadLines(@"C:\Dev\Temp\BasicOpenings_1_2.csv").Skip(1))
        {
            var line = "2715,d4";
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

            break;
        }
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
        if (moves == null || moves.Count != 1)
        {
            return null;
        }
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
        if(moves == null || moves.Count != 1)
        {
            return null;
        }
        return moves[0];
    }

    private static void SaveOpening(int id)
    {
        var moveHistory = Boot.GetService<IMoveHistoryService>();

        MoveKeyList moveKeys = new short[16];

        moveHistory.GetSequence(ref moveKeys);

        moveKeys.Order();

        var key = moveKeys.AsKey();

        //Console.WriteLine($"{key} {id}");

        _dataAccessService.SaveOpening(key, id);
    }
}