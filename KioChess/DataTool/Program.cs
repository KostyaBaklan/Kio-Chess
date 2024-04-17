﻿using DataAccess.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Engine.Dal.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Models.Moves;
using Engine.Services;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Text;
using Tools.Common;

internal class Program
{
    private static Dictionary<string, byte> _squares = new Dictionary<string, byte>();
    private static Dictionary<string, byte> _pieces = new Dictionary<string, byte>();
    private static Dictionary<string, string> _subPieces = new Dictionary<string, string>();
    private static IOpeningDbService _openingDbService;
    private static IGameDbService _gameDbService;
    private static IBulkDbService _bulkDbService;

    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        Initialize();

        //_openingDbService = Boot.GetService<IOpeningDbService>();
        _gameDbService = Boot.GetService<IGameDbService>();
        //var inMemory = Boot.GetService<IMemoryDbService>();
        _bulkDbService = Boot.GetService<IBulkDbService>();

        try
        {
            //inMemory.Connect();
            //_openingDbService.Connect();
            _gameDbService.Connect();
            _bulkDbService.Connect();

            //ProcessDebuts();

            //text = File.ReadAllText(@"C:\Dev\PGN\Openings\codes.json");
            //Dictionary<string, List<OpeningItem>> codes = JsonConvert.DeserializeObject<Dictionary<string, List<OpeningItem>>>(text);

            //ProcessEcoPgn();
            //PopularTest(timer);
        }
        finally
        {
            // inMemory.Disconnect();
            //_openingDbService.Disconnect();
            _gameDbService.Disconnect();
            _bulkDbService?.Disconnect();
        }

        timer.Stop();
        Console.WriteLine();
        Console.WriteLine(timer.Elapsed);
        Console.WriteLine();
        Console.WriteLine($"Finished !!!");
        Console.ReadLine();
    }

    private static void ProcessDebuts()
    {
        Dictionary<string, string> replaceMap = new Dictionary<string, string>
            {
                {"Benoni","Benoni Defense" },
                {"Bird","Bird's Opening" },
                {"Blackmar-Diemer","Blackmar-Diemer Gambit" },
                {"Budapest","Budapest Defense" },
                {"Caro-Kann","Caro-Kann Defense" },
                {"Catalan","Catalan Opening" },
                {"Czech Benoni","Czech Benoni Defense" },
                {"Dutch","Dutch Defense" },
                {"English","English Opening" },
                {"Four Knights","Four Knights Game" },
                {"French","French Defense" },
                {"Grob","Grob's Attack" },
                {"Gruenfeld","Gruenfeld Defense" },
                {"King's Pawn","King's Pawn Game" },
                {"King's Indian","King's Indian Defense" },
                {"Nimzo-Indian","Nimzo-Indian Defense" },
                {"Old Indian","Old Indian Defense" },
                {"Petrov","Petrov's Defense" },
                {"Philidor","Philidor's Defense" },
                {"Pirc","Pirc Defense" },
                {"Polish","Polish (Sokolsky) Opening" },
                {"Ponziani","Ponziani Opening" },
                {"Queen's Indian","Queen's Indian Defense" },
                {"Queen's Pawn","Queen's Pawn Game" },
                {"Reti","Reti Opening" },
                {"Scandinavian","Scandinavian Defense" },
                {"Sicilian","Sicilian Defense" },
                {"Three Knights","Three Knights Game" },
                {"Two Knights","Two Knights Defense" },
                {"Vienna","Vienna Game" },
                {"Scotch","Scotch Game" },
                {"Scotch Opening","Scotch Game" },
                {"Grob's Attack","Grob's Opening" }
            };

        var text = File.ReadAllText(@"C:\Dev\PGN\Openings\openings.json");
        List<OpeningItem> openings = JsonConvert.DeserializeObject<List<OpeningItem>>(text);

        foreach (var opening in openings)
        {
            opening.Capitalize();

            if (replaceMap.TryGetValue(opening.Name, out var name))
            {
                opening.Name = name;
            }
        }

        var names = openings.Select(o => o.Name).ToHashSet();

        File.WriteAllLines(@"C:\Dev\PGN\Openings\openingsNames.txt", names.OrderBy(x => x));

        var json = JsonConvert.SerializeObject(openings, Formatting.Indented);
        File.WriteAllText(@"C:\Dev\PGN\Openings\openings.json", json);

        var sequenseInfoes = openings.Select(o => o.GetSequenceInfo()).ToList();
        json = JsonConvert.SerializeObject(sequenseInfoes, Formatting.Indented);
        File.WriteAllText(@"C:\Dev\PGN\Openings\sequenseInfoes.json", json);

        Position position = new Position();
        MoveSequenceParser parser = new MoveSequenceParser(position, Boot.GetService<MoveHistoryService>());

        List<SequenceItem> sequenceItems = new List<SequenceItem>();
        foreach (var sequenceInfo in sequenseInfoes)
        {
            var sequenceItem = ParseSequence(sequenceInfo, parser);
            sequenceItems.Add(sequenceItem);
        }

        //var movesMap = sequenceItems.GroupBy(s => s.Moves).ToDictionary(k => k.Key, v => v.ToList());

        //var map = movesMap.Where(v => v.Value.Count > 1).ToDictionary(k => k.Key, v => v.Value);


        json = JsonConvert.SerializeObject(sequenceItems, Formatting.Indented);
        File.WriteAllText(@"C:\Dev\PGN\Openings\sequenceItems.json", json);

        _gameDbService.AddDebuts(sequenceItems.Select(si => new Debut { Code = si.Code, Name = si.Name, Sequence = Encoding.Unicode.GetBytes(si.Moves) }));
    }

    private static SequenceItem ParseSequence(SequenceInfo sequenceInfo,MoveSequenceParser parser)
    {
        var sequence = sequenceInfo.Sequence;

        var parts = sequence.Split(new char[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(p=>!int.TryParse(p,out _))
            .ToArray();

        var seq = parser.Parse(parts);
        
        //foreach (var item in parts)
        //{
        //    MoveBase move = null;
        //    if (isWhite)
        //    {
        //        move = ParseWhiteMove(item, position);
        //    }
        //    else
        //    {
        //        move = ParseBlackMove(item, position);
        //    }

        //    if (move != null)
        //    {
        //        if (position.GetHistory().Any())
        //        {
        //            position.Make(move); 
        //        }
        //        else
        //        {
        //            position.MakeFirst(move);
        //        }
        //        moves.Add(move);
        //        isWhite = !isWhite;
        //    }
        //    else
        //    {
        //        throw new Exception("Parse Error!");
        //    }
        //}

        return new SequenceItem
        {
            Code = sequenceInfo.Code,
            Name = sequenceInfo.Name,
            Moves = seq
        };
    }

    private static void ProcessEcoPgn()
    {
        OpeningItem current = new OpeningItem();
        List<OpeningItem> items = new List<OpeningItem>();

        var lines = File.ReadLines(@"C:\Dev\PGN\Openings\eco.pgn");

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("[Site "))
            {
                var site = line.Replace("[Site ", string.Empty).Trim(']').Trim('"').Trim(' ');
                current.Code = site;
            }
            else if (line.StartsWith("[White "))
            {
                var site = line.Replace("[White ", string.Empty).Trim(']').Trim('"').Trim(' ');
                current.Name = site;
            }
            else if (line.StartsWith("[Black "))
            {
                var site = line.Replace("[Black ", string.Empty).Trim(']').Trim('"').Trim(' ');
                current.Variation = site;
            }
            else if (line.StartsWith("1. "))
            {
                current.Sequence = line.Trim(' ');

                var item = current.Clone();

                items.Add(item);

                current = new OpeningItem();
            }
            else
            {
                if (items.Any())
                {
                    var last = items.Last();
                    var sequence = last.Sequence;

                    last.Sequence = new StringBuilder(sequence.TrimEnd(' ')).Append(' ').Append(line).ToString();
                }

            }
        }

        Dictionary<string, List<OpeningItem>> codes = items.GroupBy(i => i.Code).ToDictionary(k => k.Key, v => v.ToList());
        File.WriteAllText("codes.json", JsonConvert.SerializeObject(codes, Formatting.Indented));

        var json = JsonConvert.SerializeObject(items, Formatting.Indented);
        File.WriteAllText("openings.json", json);
    }

    private static void PopularTest(Stopwatch timer)
    {
        for (int i = 10; i < 101; i += 10)
        {
            IEnumerable<SequenceTotalItem> items = _gameDbService.GetPopular(i);

            var moveMap = items.GroupBy(l => l.Seuquence, v => v.Move)
                .Where(x => x.Count() > 4)
                .ToDictionary(k => k.Key, v => v.OrderByDescending(a => a.Value).Select(b => b.Id).ToArray());

            Console.WriteLine($"{i}   {moveMap.Count}   {timer.Elapsed}");
        }
    }

    private static void Initialize()
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

        Boot.SetUp();
    }

    private static void GenerateMoves(Position position, MoveHistoryService moveHistory)
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
        List<KeyValuePair<string, OpeningInfo>> list = new List<KeyValuePair<string, OpeningInfo>>();
        Dictionary<string, int> map = new Dictionary<string, int>();

        var basicOpenings = File.ReadLines(@"C:\Projects\AI\Kio-Chess\KioChess\DataTool\BasicOpenings.csv").ToHashSet();
        var openingTotal = File.ReadLines(@"C:\Projects\AI\Kio-Chess\KioChess\DataTool\OpeningsTotal.csv")
            .Select(l => l.Split(','))
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

            int id1 = _openingDbService.GetOpeningVariationID(k1);
            int id2 = _openingDbService.GetOpeningVariationID(k2);

            if (id1 > 0)
            {
                if (id2 > 0)
                {
                    string n1 = _openingDbService.GetOpeningName(k1);
                    string n2 = _openingDbService.GetOpeningName(k2);
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
                            _openingDbService.SaveOpening(info.Key, id1);
                        }
                        else
                        {
                            _openingDbService.SaveOpening(info.Key, id2);
                        }

                        Console.WriteLine($"{++count} {n1} x {n2} = {(basicOpeningTotal[parts[0]] > basicOpeningTotal[parts[1]] ? n1 : n2)}");
                    }
                }
                else
                {
                    _openingDbService.SaveOpening(info.Key, id1);
                }
            }
            else
            {
                if (id2 > 0)
                {
                    _openingDbService.SaveOpening(info.Key, id2);
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
            int id = _openingDbService.GetOpeningVariationID(key);

            if (id > 0)
            {
                _openingDbService.SaveOpening(info.Key, id);
                Console.WriteLine($"{info.Key} - {_openingDbService.GetOpeningName(info.Key)}");
            }
            else
            {
                Console.WriteLine(key);
            }
        }
    }

    private static void SaveOpeningMap(Dictionary<string, OpeningInfo> map, string file) => File.WriteAllLines(file, map.Select(p => JsonConvert.SerializeObject(p.Value)));

    private static void ProcessMove(MoveHistoryService moveHistory, Dictionary<string, OpeningInfo> openings, MoveBase m, Dictionary<string, OpeningInfo> unknown)
    {
        var key = moveHistory.GetSequenceKey();

        var o = _openingDbService.GetOpeningName(key);

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

    private static void ProcessSequences(Position position)
    {
        List<KeyValuePair<int, string>> sequences = _openingDbService.GetSequences();

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

    private static void processBasicOpenings(Position position)
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

    private static MoveBase ParseWhiteMove(string m, Position position)
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

    private static MoveBase ParseBlackMove(string m, Position position)
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
        if (moves == null || moves.Count != 1)
        {
            return null;
        }
        return moves[0];
    }

    private static void SaveOpening(int id)
    {
        var moveHistory = Boot.GetService<MoveHistoryService>();

        var key = moveHistory.GetSequenceKey();

        //Console.WriteLine($"{key} {id}");

        _openingDbService.SaveOpening(key, id);
    }
}