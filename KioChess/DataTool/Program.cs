using DataAccess.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Engine.Dal.Interfaces;
using Engine.DataStructures;
using Engine.Models.Boards;
using Engine.Models.Helpers;
using Engine.Services;
using Newtonsoft.Json;
using System.Diagnostics;
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
    private static ILocalDbService _localDbService;

    private static void Main(string[] args)
    {
        Boot.SetUp();
        var timer = Stopwatch.StartNew();

        Initialize();

        _openingDbService = Boot.GetService<IOpeningDbService>();
        _gameDbService = Boot.GetService<IGameDbService>();
        //var inMemory = Boot.GetService<IMemoryDbService>();
        _bulkDbService = Boot.GetService<IBulkDbService>();
        _localDbService = Boot.GetService<ILocalDbService>();

        try
        {
            //inMemory.Connect();
            _openingDbService.Connect();
            _gameDbService.Connect();
            _bulkDbService.Connect();
            _localDbService.Connect();

            //GenerateStockFishToolPairs();

            ProcessPositionTotalDifferences();



            //text = File.ReadAllText(@"C:\Dev\PGN\Openings\codes.json");
            //Dictionary<string, List<OpeningItem>> codes = JsonConvert.DeserializeObject<Dictionary<string, List<OpeningItem>>>(text);

            //ProcessEcoPgn();
            //PopularTest(timer);

            //ParseDebutVariations();

            //var json = JsonConvert.SerializeObject(_openingDbService.GetAllDebuts(), Formatting.Indented);
            //File.WriteAllText(@"C:\Dev\PGN\Openings\AllDebuts.json", json);
        }
        finally
        {
            // inMemory.Disconnect();
            _openingDbService.Disconnect();
            _gameDbService.Disconnect();
            _bulkDbService?.Disconnect();
            _localDbService?.Disconnect();
        }

        timer.Stop();
        Console.WriteLine();
        Console.WriteLine(timer.Elapsed);
        Console.WriteLine();
        Console.WriteLine($"Finished !!!");
        Console.ReadLine();
    }

    private static void ProcessPositionTotalDifferences()
    {
        Console.WriteLine("Clear Position Total Difference");
        _localDbService.ClearPositionTotalDifference();

        _localDbService.Shrink();

        var positions = _gameDbService.LoadPositions();

        var chunks = positions.Chunk(25000);

        int size = 0;
        int count = 0;

        foreach (var chunk in chunks)
        {
            size += chunk.Length;
            count++;
            Console.WriteLine($"{count} - {size}");

            _localDbService.Add(chunk);
        }

        Console.WriteLine($"Total Positions = {_localDbService.GetPositionsCount()}");
    }

    private static void GenerateStockFishToolPairs()
    {
        var history = _gameDbService.Get(new byte[0]);

        var position = new Position();

        var mp = Boot.GetService<MoveProvider>();

        List<Seq> seqs = new List<Seq>();

        foreach (var historyEntry in history)
        {
            MoveKeyList moveKeyList = new MoveKeyList(new short[1]);
            moveKeyList.Add(historyEntry.Key);
            var bytes = moveKeyList.AsByteKey();
            var hh = _gameDbService.Get(bytes);

            var seq = hh.Select(i => new Seq(mp) { White = historyEntry.Key, Black = i.Key, Total = i.Value.GetTotal() });

            seqs.AddRange(seq.Where(s=>s.Total > 50000));
        }

        seqs.Sort();

        Dictionary<short, Dictionary<string, string>> seqMap = seqs.GroupBy(s => s.White).ToDictionary(k => k.Key, v => v.ToDictionary(k => k.ToSequence(), v => v.ToString()));

        var json = JsonConvert.SerializeObject(seqMap, Formatting.Indented);
        File.WriteAllText("seqMap.json", json);
        foreach (var mapItem in seqMap)
        {
            Console.WriteLine(mapItem.Key);
            foreach (var item in mapItem.Value)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine();
        }
    }

    private static void LoadPositionTotalDifferences(int chunkSize)
    {
        var timer = Stopwatch.StartNew();
        IEnumerable<PositionTotalDifference> positions = _gameDbService.LoadPositionTotalDifferences();

        var chunks = positions.Chunk(chunkSize);

        int size = 0;
        int count = 0;

        foreach (var chunk in chunks)
        {
            size += chunk.Length;
            count++;
            Console.WriteLine($"{count} - {size} - {timer.Elapsed}");
        }

        Console.WriteLine($"{nameof(LoadPositionTotalDifferences)} - {timer.Elapsed}");
    }

    private static void ParseDebutVariations()
    {
        var text = File.ReadAllText(@"C:\Dev\PGN\Openings\openingVariations.json");

        var ov = JsonConvert.DeserializeObject<Debuts>(text);

        Dictionary<string, List<Debut>> codes = ov.Codes;

        var debuts = codes.Values.SelectMany(v => v).ToList();

        _localDbService.AddDebuts(debuts);
    }

    private static void CompareDebuts()
    {
        List<Debut> debuts = _localDbService.GetAllDebuts();

        var debutSequences = debuts.Select(d => new DebutSequence { Debut = d, Sequence = ToShorts(d.Sequence) }).ToDictionary(k => k.Sequence);

        var variations = _openingDbService.GetAllVariations().GroupBy(k => k.Sequence).ToDictionary(k => k.Key, v =>
        {
            return v.Select(a =>
            {
                a.OpeningVariation.Name = a.OpeningVariation.Name.Replace(':', ',');
                return a;
            }).ToList();
        });

        List<DebutSequence> debutsOnly = debutSequences.Where(d => !variations.ContainsKey(d.Key)).Select(x => x.Value).ToList();

        List<OpeningSequence> variationsOnly = variations.Where(d => !debutSequences.ContainsKey(d.Key)).SelectMany(x => x.Value).ToList();

        List<DebutVariation> debutVariations = debutSequences.Where(d => variations.ContainsKey(d.Key))
            .SelectMany(x => variations[x.Key]
            .Select(q => new DebutVariation { DebutSequence = x.Value, OpeningSequence = q }))
            .Where(dv => dv.DebutSequence.Debut.Name != dv.OpeningSequence.OpeningVariation.Name)
            .ToList();


        var json = JsonConvert.SerializeObject(debutVariations, Formatting.Indented);
        File.WriteAllText(@"C:\Dev\PGN\Openings\debutVariations.json", json);

        Debuts dbs = new Debuts
        {
            DebutVariations = debutVariations,
            Codes = debuts.GroupBy(d => d.Code).ToDictionary(k => k.Key, v => v.ToList())
        };

        json = JsonConvert.SerializeObject(dbs, Formatting.Indented);
        File.WriteAllText(@"C:\Dev\PGN\Openings\openingVariations.json", json);
    }

    private static string ToShorts(byte[] sequence)
    {
        var shorts = new short[sequence.Length / 2];

        Buffer.BlockCopy(sequence, 0, shorts, 0, sequence.Length);

        return string.Join('-', shorts);
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

        _localDbService.AddDebuts(sequenceItems.Select(si => new Debut { Code = si.Code, Name = si.Name, Sequence = Encoding.Unicode.GetBytes(si.Moves) }));
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
            Moves = seq,
            Sequence = sequenceInfo.Sequence
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

}

public class DebutSequence
{
    public Debut Debut { get; set; }
    public string Sequence { get; set; }
}

public class DebutVariation
{
    public DebutSequence DebutSequence { get; set; }
    public OpeningSequence OpeningSequence { get; set; }
}

public class Debuts
{
    public List<DebutVariation> DebutVariations { get; set; }
    public Dictionary<string, List<Debut>> Codes { get; set; }
}