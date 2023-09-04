using Engine.Book.Interfaces;
using Engine.Interfaces.Config;
using Newtonsoft.Json;
using OpeningMentor.Chess.Model;
using System;
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
class Opening
{
    public string Name { get; set; }
    public string Variation { get; set; }
    public List<string> Moves { get; set; }
}

internal class Program
{
    private static int _elo;
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        _elo = Boot.GetService<IConfigurationProvider>().BookConfiguration.Elo;

        //var text = File.ReadAllText("OpeningVariationNames.json");
        //var dictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(text);

        var das = Boot.GetService<IDataAccessService>();
        try
        {
            das.Connect();

            var openings = File.ReadLines("BasicOpenings2.txt")
                .Select(JsonConvert.DeserializeObject<Opening>)
                //.Where(o => o.Moves.Count == 2)
                .ToList();

            foreach (var item in openings)
            {
                das.SaveOpening(item.Name, item.Variation, string.Join(' ', item.Moves));
            }

            //int progress = 0;

            //using (var writer = new StreamWriter("BasicOpenings2.txt"))
            //{
            //    foreach (var opening in openings)
            //    {
            //        var json = JsonConvert.SerializeObject(opening);
            //        Console.WriteLine($"{++progress}   {json}");
            //        writer.WriteLine(json);
            //    }
            //}

            //GetOpeningNames(das);
        }
        finally
        {
            das.Disconnect();
        }

        //ProcessPgnFiles(timer);

        //ProcessPgnOpenings();

        timer.Stop();

        Console.WriteLine(timer.Elapsed);

        Console.WriteLine("PGN DONE !!!");

        Console.ReadLine();
    }

    private static void GetOpeningNames(IDataAccessService das)
    {
        HashSet<string> openingNames = das.GetOpeningNames();

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

                    var mss = GetCommonSequence(moveSequences.Select(w => w.Moves).ToList());

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

            using (var writer = new StreamWriter("OpeningSequences.txt"))
            {
                foreach (var item in openings)
                {
                    string op;
                    string variation;
                    if (item.Key.Contains(":"))
                    {
                        var parts = item.Key.Split(new char[] { ':' }, StringSplitOptions.TrimEntries);
                        op = parts[0];
                        variation = parts[1];
                    }
                    else if (item.Key.Contains("#"))
                    {
                        var parts = item.Key.Split(new char[] { '#' }, StringSplitOptions.TrimEntries);
                        op = parts[0];
                        variation = $"#{parts[1]}";
                    }
                    else if (item.Key.Contains(","))
                    {
                        var parts = item.Key.Split(new char[] { ',' }, StringSplitOptions.TrimEntries);
                        op = parts[0];
                        variation = parts[1];
                    }
                    else
                    {
                        op = item.Key;
                        variation = string.Empty;
                    }

                    HashSet<Sequence> moveSequences = new HashSet<Sequence>();
                    foreach (var sequ in item.Value)
                    {
                        var seque = sequ.Split(new char[] { '.', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .Where(q => !int.TryParse(q, out _)).ToList();

                        Sequence se = new Sequence(seque);

                        moveSequences.Add(se);
                    }

                    var mss = GetCommonSequence(moveSequences.Select(w => w.Moves).ToList());

                    foreach (var ms in mss)
                    {
                        Opening opening = new Opening
                        {
                            Name = op,
                            Variation = variation,
                            Moves = ms
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

    private static List<List<string>> GetCommonSequence(List<List<string>> list)
    {
        if (list == null|| list.Count == 0) return new List<List<string>>();

        if(list.Count == 1)return list;

        List<List<string>> ll = new List<List<string>>();

        var sequence = list.FirstOrDefault();

        for (int i = sequence.Count - 1; i > 1; i--)
        {
            var dic = list.GroupBy(l => new Sequence(l.GetRange(0, i)))
                .OrderByDescending(e=>e.Count())
                .ToDictionary(d => d.Key, v => Math.Round(100.0 * v.Count()/list.Count,2));

            if (dic.Count < 20||i == 2)
            {
                ll.AddRange(dic.TakeWhile(d=>d.Value > 1).Select(f=>f.Key.Moves));
            }
        }

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