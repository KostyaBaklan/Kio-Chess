using DataAccess.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Engine.Services;
using GamesServices;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using Tools.Common;

internal class Program
{
    private static int _elo;
    private static int _configElo;
    private static Dictionary<string, int> _suggestedElos;
    private static IOpeningDbService _dataAccessService;
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        _elo = Boot.GetService<IConfigurationProvider>().BookConfiguration.Elo;
        _configElo = _elo;

        //var text = File.ReadAllText("OpeningVariationNames.json");
        //var dictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(text);

        _dataAccessService = Boot.GetService<IOpeningDbService>();
        try
        {
            _dataAccessService.Connect();

            //ParseEcos();

            //ProcessPgnOpenings();

            //AddNewSequenceVariations();

            //AddNewSequences();

            CountElo(timer);

            ProcessPgnFiles(timer);

            //ProcessPgnFilesWithoutElo(timer);

            //ProcessFailures();

        }
        finally
        {
            _dataAccessService.Disconnect();
        }

        timer.Stop();

        Console.WriteLine(timer.Elapsed);

        Console.WriteLine("PGN DONE !!!");

        Console.ReadLine();
    }

    private static void ProcessFailures()
    {
        var dir = @"C:\Projects\AI\Kio-Chess\KioChess\Data\Release\net7.0\PGNs\Failures";

        var directory = new DirectoryInfo(dir);

        var files = directory.GetFiles("*.pgn");

        for (int i = 0; i < files.Length; i++)
        {
            FileInfo file = files[i];

            StringBuilder bui
                = new StringBuilder();

            bui.Append('"').Append(file.FullName).Append('"');

            var par = bui.ToString();

            var process = Process.Start("PgnTool.exe", par);
            process.WaitForExit();
        }
    }

    private static void ParseEcos()
    {
        var lines = File.ReadLines(@"C:\Projects\AI\Kio-Chess\KioChess\PgnManager\Openings\ECOs.csv")
            .Skip(2).ToList();

        List<EcoInformation> ecoInformations = new List<EcoInformation>();

        foreach (var line in lines)
        {
            var columns = line.Split(',', StringSplitOptions.TrimEntries);

            EcoInformation ecoInformation = new EcoInformation(columns);

            ecoInformations.Add(ecoInformation);
        }

        var map = ecoInformations
            .GroupBy(e => e.Opening.Moves.Count)
            .Where(e=>e.Key > 2)
            .OrderBy(e=>e.Key)
            .ToDictionary(k => k.Key, v => v.Select(x=>x.Opening).ToList());

        var dir = "Candidates";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        foreach(var pair in map)
        {
            var file = Path.Combine(dir, $"Opening_{pair.Key}.txt");

            List<Opening> lists = pair.Value;

            File.WriteAllLines(file, lists.Select(JsonConvert.SerializeObject));
        }
    }

    private static void AddNewSequences()
    {
        var sequences = _dataAccessService.GetSequences("[ID] > 3332");

        var parser = new MoveSequenceParser(new Position(), Boot.GetService<MoveHistoryService>());

        int count = 0;

        foreach (var item in sequences)
        {
            var openingVariationID = item.Key;
            var moves = item.Value.Split(" ");
            var sequence = parser.Parse(moves);

            Console.WriteLine($"{++count}   ID = {openingVariationID} {item.Value} {sequence}");

            if (string.IsNullOrWhiteSpace(sequence))
            {

            }
            else
            {
                _dataAccessService.SaveOpening(sequence, openingVariationID);
            }
        }
    }

    private static void AddNewSequenceVariations()
    {
        List<string> list = new List<string>();
        List<string> failure1 = new List<string>();
        List<string> failure2 = new List<string>();
        List<string> failure4 = new List<string>();
        List<string> failure3 = new List<string>();

        int count = 0;

        foreach (var opening in File
            .ReadLines(@"C:\Projects\AI\Kio-Chess\KioChess\Data\Debug\net7.0\CandidateSequenceList_Temp_7.txt")
            .Select(JsonConvert.DeserializeObject<Opening>))
        {
            var openingID = _dataAccessService.GetOpeningID(opening.Name);
            if (openingID < 0)
            {
                _dataAccessService.AddOpening(new[] { opening.Name });
                openingID = _dataAccessService.GetOpeningID(opening.Name);
            }

            var variationID = _dataAccessService.GetVariationID(opening.Variation);
            if (variationID < 0)
            {
                _dataAccessService.AddVariations(new[] { opening.Variation });
                variationID = _dataAccessService.GetVariationID(opening.Variation);
            }

            var name = string.IsNullOrWhiteSpace(opening.Variation)
                ? opening.Name
                : $"{opening.Name}: {opening.Variation}";

            if (openingID > 0 && variationID > 0)
            {
                var parser = new MoveSequenceParser(new Position(), Boot.GetService<MoveHistoryService>());
                if (parser.IsValid(opening.Moves))
                {
                    if (!_dataAccessService.IsOpeningVariationExists(openingID, variationID))
                    {
                        if (_dataAccessService.AddOpeningVariation(name, openingID, variationID, opening.Moves))
                        {
                            list.Add(name);
                        }
                        else
                        {
                            failure1.Add(name);
                        }
                    }
                    else
                    {
                        failure2.Add(name);
                    }
                }
                else
                {
                    failure4.Add(name);
                }
            }
            else
            {
                failure3.Add(name);
            }

            Console.WriteLine($"{++count} {name}");
        }

        //File.WriteAllLines("OpeningsToSave.txt", list);
        File.WriteAllLines("FailuresToCheck1.txt", failure1);
        File.WriteAllLines("FailuresToCheck2.txt", failure2);
        File.WriteAllLines("FailuresToCheck3.txt", failure3);
    }

    private static void SetOpeningVariations()
    {
        var openings = File.ReadLines(@"C:\Projects\AI\Kio-Chess\KioChess\PgnManager\Openings\Openings_3.txt")
            .Select(JsonConvert.DeserializeObject<Opening>)
            .ToList();

        foreach (var openingObj in openings)
        {
            var openingName = openingObj.Name.Trim(' ').Trim('.').Trim('"');
            var variationName = openingObj.Variation.Trim(' ').Trim('.').Trim('"');

            short openingID = _dataAccessService.GetOpeningID(openingName);
            if(openingID < 0)
            {
                _dataAccessService.AddOpening(new[] { openingName });
                openingID = _dataAccessService.GetOpeningID(openingName);
            }

            short variationID = _dataAccessService.GetVariationID(variationName);
            if(variationID < 0)
            {
                _dataAccessService.AddVariations(new[] { variationName });
                variationID = _dataAccessService.GetVariationID(variationName);
            }

            if(openingID > 0 && variationID > 0)
            {
                var name = string.IsNullOrWhiteSpace(variationName) ? openingName:$"{openingName}: {variationName}";

                _dataAccessService.AddOpeningVariation(name, openingID, variationID,openingObj.Moves);
            }
            else
            {
                Console.WriteLine(JsonConvert.SerializeObject(openingObj));
            }
        }

    }

    private static void SetVariations()
    {
        var names = File.ReadLines(@"C:\Projects\AI\Kio-Chess\KioChess\PgnManager\Openings\VariationNames.txt").ToHashSet();

        foreach (var line in File.ReadLines(@"C:\Projects\AI\Kio-Chess\KioChess\PgnManager\Openings\OpeningListTable.csv"))
        {
            var parts = line.Split(',');

            names.Add(parts[2]);
        }

        _dataAccessService.AddVariations(names.Select(n => n.Trim(' ').Trim('.').Trim('"')).OrderBy(n=>n));

        //int i = 1;
        //foreach (var name in names.OrderBy(n => n))
        //{
        //    Console.WriteLine($"{i++} {name}");
        //}
    }

    private static void SetOpenings()
    {
        var names = File.ReadLines(@"C:\Projects\AI\Kio-Chess\KioChess\PgnManager\Openings\OpeningNames.txt").ToHashSet();

        foreach (var line in File.ReadLines(@"C:\Projects\AI\Kio-Chess\KioChess\PgnManager\Openings\OpeningListTable.csv"))
        {
            var parts = line.Split(',');

            names.Add(parts[1]);
        }

        _dataAccessService.AddOpening(names.Select(n => n.Trim(' ').Trim('.').Trim('"')).OrderBy(n => n));

        //int i = 1;
        //foreach (var name in names.OrderBy(n=>n))
        //{
        //    Console.WriteLine($"{i++} {name}");
        //}
    }

    private static void ProcessPgnOpeningsNames()
    {
        object sync = new object();

        int f = 0;

        try
        {
            var files = Directory.GetFiles(@"C:\Dev\PGN", "*.pgn");

            Dictionary<string, List<string>> list = new Dictionary<string, List<string>>();

            foreach (var file in files)
            {
                f++;

                var tasks = new List<Task> { Task.CompletedTask };

                StringBuilder stringBuilder = new StringBuilder();

                using (var reader = new StreamReader(file))
                {
                    var size = 100.0 / reader.BaseStream.Length;

                    string line;
                    string opening = string.Empty;
                    string sequence = string.Empty;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.ToLower().StartsWith("[event"))
                        {
                            if (!string.IsNullOrWhiteSpace(opening) && !string.IsNullOrWhiteSpace(sequence))
                            {
                                if (list.TryGetValue(opening, out var seq))
                                {
                                    seq.Add(sequence);
                                }
                                else
                                {
                                    list[opening] = new List<string> { sequence };
                                    //Console.WriteLine($"{++count}   {opening}  {sequence}");
                                }

                                //var gameAsString = stringBuilder.ToString();

                                //if (!string.IsNullOrWhiteSpace(gameAsString))
                                //{
                                //    //var progress = Math.Round(reader.BaseStream.Position * size, 6);

                                //    //var task = Task.Factory.StartNew(() =>
                                //    //{
                                //    //    var buffer = Encoding.UTF8.GetBytes(gameAsString);

                                //    //    var text = Convert.ToBase64String(buffer);

                                //    //    var t = Stopwatch.StartNew();

                                //    //    var process = Process.Start("PgnTool.exe", text);
                                //    //    process.WaitForExit();

                                //    //    t.Stop();

                                //    //    lock (sync)
                                //    //    {
                                //    //        Console.WriteLine($"{f}/{files.Length}   {++count}   {progress}%   {t.Elapsed}   {timer.Elapsed}");
                                //    //    }
                                //    //});

                                //    //tasks.Add(task);
                                //} 
                            }

                            stringBuilder = new StringBuilder(line);
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("["))
                            {
                                sequence = line;
                            }
                            if (line.ToLower().StartsWith("[opening"))
                            {
                                var parts = line.Split('"');
                                if (string.IsNullOrWhiteSpace(parts[1]))
                                {
                                    opening = string.Empty;
                                }
                                else
                                {
                                    opening = parts[1];
                                }
                            }
                            stringBuilder.Append(line);
                        }
                    }
                }

                Task.WaitAll(tasks.ToArray());

                Console.WriteLine(f);
            }

            var openings = list.Keys.OrderBy(x => x).ToHashSet();

            var ops = new HashSet<string>();
            var variations = new HashSet<string>();

            Console.WriteLine(openings.Count);

            foreach (var item in openings)
            {
                string op;
                string variation;
                ExtractOpeningAndVariation(out op, out variation, item);

                ops.Add(op);
                variations.Add(variation);
            }

            File.WriteAllLines("OpeningNames.txt", ops.OrderBy(x => x));
            File.WriteAllLines("VariationNames.txt", variations.OrderBy(x => x));
            File.WriteAllLines("OpeningVariationNames.txt", openings);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToFormattedString());

            Console.WriteLine("Pizdets !!!");
        }
    }

    private static void GetOpeningNames()
    {
        HashSet<string> openingNames = _dataAccessService.GetOpeningNames();

        object sync = new object();

        int f = 0;

        try
        {
            var files = Directory.GetFiles(@"C:\Dev\PGN", "*.pgn");

            Dictionary<string, List<string>> list = new Dictionary<string, List<string>>();

            foreach (var file in files)
            {
                f++;

                var tasks = new List<Task> { Task.CompletedTask };

                StringBuilder stringBuilder = new StringBuilder();

                using (var reader = new StreamReader(file))
                {
                    var size = 100.0 / reader.BaseStream.Length;

                    string line;
                    string opening = string.Empty;
                    string sequence = string.Empty;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.ToLower().StartsWith("[event"))
                        {
                            if (!string.IsNullOrWhiteSpace(opening) && !string.IsNullOrWhiteSpace(sequence))
                            {
                                if (list.TryGetValue(opening, out var seq))
                                {
                                    seq.Add(sequence);
                                }
                                else if (openingNames.Contains(opening))
                                {
                                    list[opening] = new List<string> { sequence };
                                }
                            }

                            stringBuilder = new StringBuilder(line);
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("["))
                            {
                                sequence = line;
                            }
                            if (line.ToLower().StartsWith("[opening"))
                            {
                                var parts = line.Split('"');
                                if (string.IsNullOrWhiteSpace(parts[1]))
                                {
                                    opening = string.Empty;
                                }
                                else
                                {
                                    opening = parts[1];
                                }
                            }
                            stringBuilder.Append(line);
                        }
                    }
                }

                Task.WaitAll(tasks.ToArray());
            }

            Dictionary<string, List<string>> openings = list.OrderBy(k => k.Key)
                .ToDictionary(k => k.Key, v => v.Value
                                        .Where(x => x.Contains("7.") && x.IndexOf("7.") < 100)
                                        .Select(s =>
                                        {
                                            return s.Substring(0, s.IndexOf("7."));
                                        })
                                        .ToList());

            Console.WriteLine(openings.Count);

            int progress = 0;

            using var writer = new StreamWriter("BasicOpenings.txt");
            foreach (var item in openings)
            {
                HashSet<Sequence> moveSequences = new HashSet<Sequence>();
                foreach (var sequ in item.Value)
                {
                    var seque = sequ.Split(new char[] { '.', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(q => !int.TryParse(q, out _)).ToList();

                    Sequence se = new Sequence(seque);

                    moveSequences.Add(se);
                }

                var mss = GetCommonSequence(moveSequences.Select(w => w.Moves).ToList(), 3);

                //if (mss.Count == 1)
                //{
                //    das.SaveOpening(item.Key, string.Empty, string.Join(' ', mss[0]));
                //}

                if (mss.Count == 1)
                {
                    Opening opening = new Opening
                    {
                        Name = item.Key,
                        Variation = string.Empty,
                        Moves = mss[0]
                    };

                    var json = JsonConvert.SerializeObject(opening);
                    Console.WriteLine($"{++progress}   {json}");
                    writer.WriteLine(json);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToFormattedString());

            Console.WriteLine("Pizdets !!!");
        }
    }

    private static void ProcessPgnOpenings()
    {
        object sync = new object();

        int f = 0;

        int sequenceSize = 7;
        var openingKey = "6.";

        try
        {
            var files = Directory.GetFiles(@"C:\Dev\PGN", "*.pgn", SearchOption.AllDirectories);

            Dictionary<string, HashSet<string>> list = new Dictionary<string, HashSet<string>>();

            foreach (var file in files)
            {
                f++;

                StringBuilder stringBuilder = new StringBuilder();

                using (var reader = new StreamReader(file))
                {
                    var size = 100.0 / reader.BaseStream.Length;

                    string line;
                    string opening = string.Empty;
                    string sequence = string.Empty;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.ToLower().StartsWith("[event"))
                        {
                            if (!string.IsNullOrWhiteSpace(opening) && 
                                !string.IsNullOrWhiteSpace(sequence) 
                                && opening != "?"
                                && sequence.Contains(openingKey) && sequence.IndexOf(openingKey)<100
                                && !sequence.Contains("%eval"))
                            {
                                sequence = sequence.Substring(0, sequence.IndexOf(openingKey)).TrimEnd();

                                if (list.TryGetValue(opening, out var seq))
                                {
                                    seq.Add(sequence);
                                }
                                else
                                {
                                    list[opening] = new HashSet<string> { sequence };
                                    //Console.WriteLine($"{++count}   {opening}  {sequence}");
                                }
                            }

                            stringBuilder = new StringBuilder(line);
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("["))
                            {
                                sequence = line;
                            }
                            if (line.ToLower().StartsWith("[opening"))
                            {
                                var parts = line.Split('"');
                                if (string.IsNullOrWhiteSpace(parts[1]))
                                {
                                    opening = string.Empty;
                                }
                                else
                                {
                                    opening = parts[1];
                                }
                            }
                            stringBuilder.Append(line);
                        }
                    }
                }

                Console.WriteLine(f);
            }

            SortedDictionary<string, HashSet<string>> openings = new SortedDictionary<string, HashSet<string>>(list);

            Console.WriteLine(openings.Count);

            int progress = 0;

            List<Opening> candidate = new List<Opening>();

            HashSet<string> sequenceKeys = _dataAccessService.GetSequenceKeys();

            HashSet<string> sequenceSets = _dataAccessService.GetSequenceSets();

            foreach (var item in openings)
            {
                string op;
                string variation;
                string key = item.Key;

                ExtractOpeningAndVariation(out op, out variation, item.Key);

                short openingID = _dataAccessService.GetOpeningID(op);
                if (openingID < 0)
                {
                    _dataAccessService.AddOpening(new[] { op });
                    openingID = _dataAccessService.GetOpeningID(op);
                }

                short variationID = _dataAccessService.GetVariationID(variation);
                if (variationID < 0)
                {
                    _dataAccessService.AddVariations(new[] { variation });
                    variationID = _dataAccessService.GetVariationID(variation);
                }

                if (_dataAccessService.IsOpeningVariationExists(openingID, variationID))
                {
                    continue;
                }

                HashSet<Sequence> moveSequences = new HashSet<Sequence>();
                foreach (var sequ in item.Value)
                {
                    var seque = sequ.Split(new char[] { '.', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(q => !int.TryParse(q, out _)).ToList();

                    Sequence se = new Sequence(seque);

                    moveSequences.Add(se);
                }

                var mss = GetCommonSequence(moveSequences.Select(w => w.Moves).ToList(), sequenceSize);
                if (mss == null || mss.Count == 0 || mss[0].Count > sequenceSize) continue;

                var msSequences = mss.Select(ms => string.Join(' ', ms)).ToList();
                if (msSequences.Any(sequenceSets.Contains))
                {
                    continue;
                }

                foreach (var ms in mss)
                {
                    Opening opening = new Opening
                    {
                        Name = op,
                        Variation = variation,
                        Moves = ms
                    };

                    //var json = JsonConvert.SerializeObject(opening);

                    Console.WriteLine($"{++progress}");

                    candidate.Add(opening);
                }
            }

            MoveSequenceParser parser = new MoveSequenceParser(new Position(), Boot.GetService<MoveHistoryService>());

            List<Opening> candidateList = new List<Opening>();

            foreach (var item in candidate.Where(c => c.Moves.Count == sequenceSize))
            {
                var set = item.Moves.ToArray();

                bool contains = false;

                try
                {
                    var key = parser.Parse(set);
                    if(!string.IsNullOrWhiteSpace(key))
                        contains = sequenceKeys.Contains(key);
                }
                catch (Exception)
                {
                    contains = false;
                }

                if (!contains)
                {
                    candidateList.Add(item);
                }
            }

            File.WriteAllLines("CandidateSequenceList.txt", candidateList.Select(JsonConvert.SerializeObject));

            //File.WriteAllLines("OpeningSingle.txt", single);
            //File.WriteAllLines("OpeningCandidate.txt", candidate);
            //File.WriteAllLines("OpeningMultiple.txt", multiple);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToFormattedString());

            Console.WriteLine("Pizdets !!!");
        }
    }

    private static bool IsCandidate(List<List<string>> mss)
    {
        var set = mss[0].ToHashSet();

        for (int i = 1; i < mss.Count; i++)
        {
            if (set.Intersect(mss[i]).Count() < set.Count) return false;
        }

        return true;
    }

    private static void ExtractOpeningAndVariation(out string op, out string variation, string key)
    {
        if (key.Contains(":"))
        {
            var parts = key.Split(new char[] { ':' }, StringSplitOptions.TrimEntries);
            op = parts[0];
            variation = parts[1];
        }
        else if (key.Contains("#"))
        {
            var parts = key.Split(new char[] { '#' }, StringSplitOptions.TrimEntries);
            op = parts[0];
            variation = $"#{parts[1]}";
        }
        else if (key.Contains(","))
        {
            var parts = key.Split(new char[] { ',' }, StringSplitOptions.TrimEntries);
            op = parts[0];
            variation = parts[1];
        }
        else
        {
            op = key;
            variation = string.Empty;
        }

        op = op.Trim(' ').Trim('.').Trim('"');
        variation = variation.Trim(' ').Trim('.').Trim('"');
    }

    private static List<List<string>> GetCommonSequence(List<List<string>> list, int size)
    {
        if (list == null || list.Count == 0) return new List<List<string>>();

        if (list.Count == 1) return list;

        List<List<string>> ll = new List<List<string>>();

        var sequence = list.FirstOrDefault();

        for (int i = Math.Min(sequence.Count - 2,size+2); i >= size; i--)
        {
            var dic = list.GroupBy(l => new Sequence(l.GetRange(0, i)))
                .OrderByDescending(e => e.Count())
                .ToDictionary(d => d.Key, v => Math.Round(100.0 * v.Count() / list.Count, 2));

            if (dic.Count <= size)
            {
                ll.AddRange(dic.TakeWhile(d => d.Value > 2.5).Select(f => f.Key.Moves));

                if (dic.Count < size - 1 && i > size)
                {
                    break;
                }
            }
            else
            {

            }
        }

        if (ll.Count == 0) return null;

        Dictionary<int, List<List<string>>> zz = ll.OrderBy(string.Concat)
            .GroupBy(f => f.Count)
            .ToDictionary(k => k.Key, v => v.ToList());

        if (zz.Count == 1)
            return zz.Values.FirstOrDefault();

        List<List<string>> lll = null;

        foreach (var item in zz.Values)
        {
            if (item.Count > 1) break;

            lll = item;
        }

        if (lll == null)
        {
            return zz.Values.LastOrDefault();
        }

        return lll;
    }

    private static void ProcessPgnFiles(Stopwatch timer)
    {
#if DEBUG

        var process = Process.Start(@$"..\..\..\GsServer\bin\Debug\net7.0\GsServer.exe");
        process.WaitForExit(100);
#else
        var process = Process.Start(@$"..\..\..\GsServer\bin\Release\net7.0\GsServer.exe");
        process.WaitForExit(100);
# endif

        SequenceClient client = new SequenceClient();
        var service = client.GetService();
        service.Initialize();

        object sync = new object();

        int count = 0;
        int f = 0;

        try
        {
            var files = Directory.GetFiles(@"C:\Dev\PGN", "*.pgn");

            foreach (var file in files)
            {
                if (_suggestedElos != null)
                {
                    if (_suggestedElos.TryGetValue(file, out var elo) && elo > _configElo)
                    {
                        _elo = elo;
                    } 
                }
                else
                {
                    _elo = 0;
                }

                f++;

                var ff = $"{f}/{files.Length}";

                int white = 0;
                int black = 0;

                var tasks = new List<Task>();

                StringBuilder stringBuilder = new StringBuilder();

                using (var reader = new StreamReader(file))
                {
                    var size = 100.0 / reader.BaseStream.Length;

                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.ToLower().StartsWith("[event "))
                        {
                            if (Math.Min(white, black) > _elo)
                            {
                                var gameAsString = stringBuilder.ToString();

                                if (!string.IsNullOrWhiteSpace(gameAsString))
                                {
                                    var progress = Math.Round(reader.BaseStream.Position * size, 6);
                                    var c = ++count;

                                    var task = Task.Factory.StartNew(() =>
                                    {
                                        var t = Stopwatch.StartNew();

                                        ProcessStartInfo info = new ProcessStartInfo
                                        {
                                            FileName = "PgnTool.exe",
                                            ArgumentList = { gameAsString }
                                        };

                                        var process = Process.Start(info);
                                        process.WaitForExit();

                                        t.Stop(); 
                                        
                                        Console.WriteLine($"{ff}   {c}   {progress}%   {t.Elapsed}   {timer.Elapsed}");
                                    });

                                    tasks.Add(task);
                                }
                            }

                            white = 0;
                            black = 0;

                            stringBuilder = new StringBuilder(line);
                        }
                        else
                        {
                            if (line.ToLower().StartsWith("[whiteelo"))
                            {
                                var parts = line.Split('"');
                                if (int.TryParse(parts[1], out var w))
                                {
                                    white = w;
                                }
                                else
                                {
                                    white = 0;
                                }
                            }
                            else if (line.ToLower().StartsWith("[blackelo"))
                            {
                                var parts = line.Split('"');
                                if (int.TryParse(parts[1], out var b))
                                {
                                    black = b;
                                }
                                else
                                {
                                    black = 0;
                                }
                            }

                            stringBuilder.Append(line);
                        }
                    }
                }

                Task.WaitAll(tasks.ToArray());

                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to delete '{file}'");
                }
            }

            service.Save();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToFormattedString());

            Console.WriteLine("Pizdets !!!");
        }
        finally
        {
            client.Close();

            foreach (var file in _suggestedElos)
            {
                Console.WriteLine($"Finished '{file.Key}' ELO = {file.Value}");
            }
        }
    }

    private static void ProcessPgnFilesWithoutElo(Stopwatch timer)
    {
#if DEBUG

        var process = Process.Start(@$"..\..\..\GsServer\bin\Debug\net7.0\GsServer.exe");
        process.WaitForExit(100);
#else
        var process = Process.Start(@$"..\..\..\GsServer\bin\Release\net7.0\GsServer.exe");
        process.WaitForExit(100);
# endif

        SequenceClient client = new SequenceClient();
        var service = client.GetService();
        service.Initialize();

        object sync = new object();

        int count = 0;
        int f = 0;

        try
        {
            var files = Directory.GetFiles(@"C:\Dev\PGN", "*.pgn");

            foreach (var file in files)
            {
                f++;

                var ff = $"{f}/{files.Length}";

                var tasks = new List<Task>();

                StringBuilder stringBuilder = new StringBuilder();

                using (var reader = new StreamReader(file))
                {
                    var size = 100.0 / reader.BaseStream.Length;

                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.ToLower().StartsWith("[event"))
                        {
                            var gameAsString = stringBuilder.ToString();

                            if (!string.IsNullOrWhiteSpace(gameAsString))
                            {
                                var progress = Math.Round(reader.BaseStream.Position * size, 6);
                                var c = ++count;

                                var task = Task.Factory.StartNew(() =>
                                {
                                    var t = Stopwatch.StartNew();

                                    ProcessStartInfo info = new ProcessStartInfo
                                    {
                                        FileName = "PgnTool.exe",
                                        ArgumentList = { gameAsString }
                                    };

                                    var process = Process.Start(info);
                                    process.WaitForExit();

                                    t.Stop();

                                    Console.WriteLine($"{ff}   {c}   {progress}%   {t.Elapsed}   {timer.Elapsed}");
                                });

                                tasks.Add(task);
                            }

                            stringBuilder = new StringBuilder(line);
                        }
                        else
                        {
                            stringBuilder.Append(line);
                        }
                    }
                }

                Task.WaitAll(tasks.ToArray());

                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to delete '{file}'");
                }
            }

            service.Save();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToFormattedString());

            Console.WriteLine("Pizdets !!!");
        }
        finally
        {
            client.Close();
        }
    }

    private static void CountElo(Stopwatch timer)
    {
        int totalCount = 0;
        int f = 0;
        int totalGames = 0;

        _suggestedElos = new Dictionary<string, int>();

        List<double> elo = new List<double>();

        try
        {
            var files = Directory.GetFiles(@"C:\Dev\PGN", "*.pgn");

            foreach (var file in files)
            {
                f++;

                _suggestedElos[file] = _elo;

                List<int> elos = new List<int>();

                int white = 0;
                int black = 0;
                int count = 0;
                int games = 0;

                var tasks = new List<Task>();

                StringBuilder stringBuilder = new StringBuilder();

                using (var reader = new StreamReader(file))
                {
                    var size = 100.0 / reader.BaseStream.Length;

                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.ToLower().StartsWith("[event"))
                        {
                            var el = Math.Min(white, black);
                            if (el >= _elo)
                            {
                                elos.Add(el);
                                count++;
                                var progress = Math.Round(reader.BaseStream.Position * size, 6);

                                Console.WriteLine($"{f}/{files.Length}   {++totalCount}   {totalGames}   {progress}%   {timer.Elapsed}");
                            }

                            white = 0;
                            black = 0;

                            stringBuilder = new StringBuilder(line);
                            totalGames++;
                            games++;
                        }
                        else
                        {
                            if (line.ToLower().StartsWith("[whiteelo"))
                            {
                                var parts = line.Split('"');
                                if (int.TryParse(parts[1], out var w))
                                {
                                    white = w;
                                }
                                else
                                {
                                    white = 0;
                                }
                            }
                            else if (line.ToLower().StartsWith("[blackelo"))
                            {
                                var parts = line.Split('"');
                                if (int.TryParse(parts[1], out var b))
                                {
                                    black = b;
                                }
                                else
                                {
                                    black = 0;
                                }
                            }

                            stringBuilder.Append(line);
                        }
                    }
                }

                elo.Add(Math.Round(100.0 * count / games, 6));

                int maxElo = _elo;
                for(int i = _elo+5;i < 4000; i += 5)
                {
                    var eloV = elos.Count(a => a >= i);
                    Console.WriteLine($"{i}   {eloV}");
                    if(eloV < 500000)
                    {
                        break;
                    }
                    maxElo = i;
                }

                _suggestedElos[file] = maxElo;

                Console.WriteLine($"Suggested = {_suggestedElos[file]}");
            }

            elo.Add(Math.Round(100.0 * totalCount / totalGames, 6));

            Console.WriteLine("   ------    ");
            Console.WriteLine(_elo);
            
            for (int i = 0; i < elo.Count - 1; i++)
            {
                double e = elo[i];
                Console.WriteLine($"\t{e}%");
            }
            Console.WriteLine($"Total {elo.Last()}%");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToFormattedString());

            Console.WriteLine("Pizdets !!!");
        }
    }
}