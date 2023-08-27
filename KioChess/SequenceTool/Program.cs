using Data.Common;
using Engine.Book.Interfaces;
using Engine.Interfaces;
using Engine.Models.Boards;
using Engine.Models.Moves;
using Engine.Sorting.Comparers;
using Engine.Strategies.Models;
using Newtonsoft.Json;
using SequenceTool;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        var timer = Stopwatch.StartNew();

        Boot.SetUp();

        var das = Boot.GetService<IDataAccessService>();
        var bookService = Boot.GetService<IBookService>();

        string directory = @"C:\Projects\AI\Kio-Chess\KioChess\Engine\Config\Sequences";
        string[] files = new[] { "Sequence_c2-c4.txt", "Sequence_d2_d4.txt", "Sequence_e2_e4.txt", "Sequence_kf3.txt" };
        int movesToTake = 3;

        try
        {
            das.Connect();

            das.LoadAsync(bookService);

            das.WaitToData();

            IPosition position = new Position();

            var moveProvider = Boot.GetService<IMoveProvider>();

            List<MoveSequence> history = new List<MoveSequence>();

            HashSet<short> toExclude = new HashSet<short>();

            int count = 0;

            foreach (var file in files)
            {
                Console.WriteLine();
                Console.WriteLine(file);
                Console.WriteLine();

                var fullPath = Path.Combine(directory, file);
                foreach (var ms in File.ReadLines(fullPath).Select(JsonConvert.DeserializeObject<MoveSequence>))
                {
                    Console.WriteLine(ms);

                    position.MakeFirst(moveProvider.Get(ms.Keys[0]));
                    for (int i = 1; i < ms.Keys.Count; i++)
                    {
                        position.Make(moveProvider.Get(ms.Keys[i]));
                    }

                    toExclude.Clear();

                    for (int i = 0; i < movesToTake; i++)
                    {
                        SequenceStrategy strategy = new SequenceStrategy(position);

                        var result = strategy.GetResult(toExclude);

                        toExclude.Add(result.Move.Key);

                        var mhs = new MoveSequence(ms);

                        mhs.Add(result.Move);

                        history.Add(mhs);

                        Console.WriteLine($"\t{++count}   {mhs}");
                    }

                    for (int i = 0; i < ms.Keys.Count; i++)
                    {
                        position.UnMake();
                    }
                } 
            }

            File.WriteAllLines("Seuquence.txt", history.Select(JsonConvert.SerializeObject));
        }
        finally
        {
            das.Disconnect();
        }

        timer.Stop();

        Console.WriteLine();

        Console.WriteLine(timer.Elapsed);
        Console.WriteLine();

        Console.WriteLine($"Finished !!!");

        Console.ReadLine();
    }
}