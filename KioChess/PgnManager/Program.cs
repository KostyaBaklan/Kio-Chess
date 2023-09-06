using Engine.Book.Interfaces;
using Engine.Interfaces;
using Engine.Interfaces.Config;
using Engine.Models.Boards;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using OpeningMentor.Chess.Model;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using Tools.Common;

class Sequence:IEquatable<Sequence>
{
    public Sequence()
    {
        Moves = new List<string>();
    }

    public Sequence(List<string> moves)
    {
        Moves = moves;
    }

    public List<string> Moves { get; set; }

    public bool Equals(Sequence other)
    {
        if(Moves.Count!= other.Moves.Count) return false;

        for (int i = 0; i < Moves.Count; i++)
        {
            if (Moves[i] != other.Moves[i]) return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        int code = 0;
        for (int i = 0; i < Moves.Count; i++)
        {
            code ^= Moves[i].GetHashCode();
        }

        return code;
    }

    public override string ToString()
    {
        return string.Join(' ',Moves);
    }
}
class OpeningVariation
{
    public string Name { get; set; }
    public string Variation { get; set; }
}
class Opening
{
    public string Name { get; set; }
    public string Variation { get; set; }
    public List<string> Moves { get; set; }

    public OpeningVariation ToVariation()
    {
        return new OpeningVariation { Name = Name, Variation = Variation };
    }
}

internal class Program
{
    private static int _elo;
    private static IDataAccessService _dataAccessService;
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        _elo = Boot.GetService<IConfigurationProvider>().BookConfiguration.Elo;

        //var text = File.ReadAllText("OpeningVariationNames.json");
        //var dictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(text);

        _dataAccessService = Boot.GetService<IDataAccessService>();
        try
        {
            _dataAccessService.Connect();

            ProcessPgnOpenings();

        }
        finally
        {
            _dataAccessService.Disconnect();
        }

        //ProcessPgnFiles(timer);

        timer.Stop();

        Console.WriteLine(timer.Elapsed);

        Console.WriteLine("PGN DONE !!!");

        Console.ReadLine();
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

        int count = 0;
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

        int count = 0;
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

            using (var writer = new StreamWriter("BasicOpenings.txt"))
            {
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

                    var mss = GetCommonSequence(moveSequences.Select(w => w.Moves).ToList(),3);

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

        int count = 0;
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
                            if (!string.IsNullOrWhiteSpace(opening) && !string.IsNullOrWhiteSpace(sequence) && opening != "?")
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

            var openingKey = "5.";

            Dictionary<string, List<string>> openings = list.OrderBy(k => k.Key)
                .ToDictionary(k => k.Key, v => v.Value
                                        .Where(x => x.Contains(openingKey) && x.IndexOf(openingKey) < 100)
                                        .Select(s =>
                                        {
                                            return s.Substring(0, s.IndexOf(openingKey));
                                        })
                                        .ToList());

            Console.WriteLine(openings.Count);

            int progress = 0;

            List<Opening> candidate = new List<Opening>();

            HashSet<string> sequenceKeys = _dataAccessService.GetSequenceKeys();

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

                if (_dataAccessService.Exists(openingID, variationID))
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

                int size = 3;

                var mss = GetCommonSequence(moveSequences.Select(w => w.Moves).ToList(), size);
                if (mss == null || mss.Count == 0 || mss[0].Count > size) continue;

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

            MoveSequenceParser parser = new MoveSequenceParser(new Position(), Boot.GetService<IMoveHistoryService>());

            SortedDictionary<string, List<OpeningVariation>> openingList = new SortedDictionary<string, List<OpeningVariation>>();

            Dictionary<string, List<OpeningVariation>> candidateDictionary = candidate.GroupBy(l => new Sequence(l.Moves).ToString()).ToDictionary(k => k.Key, v => v.Select(o=>o.ToVariation()).ToList());

            foreach (var item in candidateDictionary.Keys)
            {
                var set = item.Split(' ');

                //List<string[]> subsets = set.Select((s, i) => set.Take(i+1).ToArray()).ToList();

                //bool contains = false;

                //for (int i = subsets.Count - 1; i >= 0; i--)
                //{
                //    var key = parser.Parse(subsets[i]);
                //    if (sequenceKeys.Contains(key))
                //    {
                //        contains = true;
                //        break;
                //    }
                //}

                var key = parser.Parse(set);
                if (!sequenceKeys.Contains(key))
                {
                    openingList[item] = candidateDictionary[item]; 
                }
                else
                {

                }
            }

            File.WriteAllText("OpeningList.json", JsonConvert.SerializeObject(openingList, Formatting.Indented));
            File.WriteAllLines("OpeningSequenceList.txt", openingList.Keys.OrderBy(g=>g));

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
        if (list == null|| list.Count == 0) return new List<List<string>>();

        if(list.Count == 1)return list;

        List<List<string>> ll = new List<List<string>>();

        var sequence = list.FirstOrDefault();

        var dic = list.Where(l => l.Count >= size)
            .GroupBy(l => new Sequence(l.GetRange(0, size)))
                .OrderByDescending(e => e.Count())
                .ToDictionary(d => d.Key, v => Math.Round(100.0 * v.Count() / list.Count, 2));

        ll.AddRange(dic.TakeWhile(d => d.Value > 1).Select(f => f.Key.Moves));

        Dictionary<int, List<List<string>>> zz = ll.OrderBy(string.Concat)
            .GroupBy(f=>f.Count)
            .ToDictionary(k=>k.Key, v=>v.ToList());

        if(zz.Count == 1)
            return zz.Values.FirstOrDefault();

        List<List<string>> lll = null;

        foreach (var item in zz.Values)
        {
            if (item.Count > 1) break;

            lll = item;
        }

        if (lll == null) 
        { 
            //var er = zz.Values.LastOrDefault();
            //var re = zz.Values.FirstOrDefault();
            return zz.Values.LastOrDefault();
        }

        return lll;
    }

    private static void ProcessPgnFiles(Stopwatch timer)
    {
        object sync = new object();

        int count = 0;
        int f = 0;

        try
        {
            var files = Directory.GetFiles(@"C:\Dev\PGN", "*.pgn");

            foreach (var file in files)
            {
                f++;

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
                        if (line.ToLower().StartsWith("[event"))
                        {
                            if (Math.Min(white, black) > _elo)
                            {
                                var gameAsString = stringBuilder.ToString();

                                if (!string.IsNullOrWhiteSpace(gameAsString))
                                {
                                    var progress = Math.Round(reader.BaseStream.Position * size, 6);

                                    var task = Task.Factory.StartNew(() =>
                                    {
                                        var buffer = Encoding.UTF8.GetBytes(gameAsString);

                                        var text = Convert.ToBase64String(buffer);

                                        var t = Stopwatch.StartNew();

                                        var process = Process.Start("PgnTool.exe", text);
                                        process.WaitForExit();

                                        t.Stop();

                                        lock (sync)
                                        {
                                            Console.WriteLine($"{f}/{files.Length}   {++count}   {progress}%   {t.Elapsed}   {timer.Elapsed}");
                                        }
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
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToFormattedString());

            Console.WriteLine("Pizdets !!!");
        }
    }
}