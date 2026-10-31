using Engine.Models.Config;
using System.Buffers.Binary;
using System.Diagnostics;

namespace NnueTool;

/// <summary>Merge labeled worker files: dedupe by position + side to move, shuffle, split into train.bin / val.bin.</summary>
internal static class MergeCommand
{
    private const int ChunkRecords = 1 << 20;

    public static void Run(string inputDirectory, string outputDirectory, NnueMergeConfiguration merge, int maxAbsScore)
    {
        var files = Directory.GetFiles(inputDirectory, "*.bin")
            .Where(f => Path.GetFileName(f) is not ("train.bin" or "val.bin"))
            .OrderBy(f => f).ToArray();
        Run(files, outputDirectory, merge, maxAbsScore);
    }

    /// <summary>Merges the given record files into train.bin / val.bin. Outputs are written to .tmp and renamed at the end.
    /// When <paramref name="keepValidation"/> is true and val.bin exists, it is left untouched and everything goes to train.bin.</summary>
    public static void Run(string[] files, string outputDirectory, NnueMergeConfiguration merge, int maxAbsScore, bool keepValidation = false, string logPath = null, string source = null)
    {
        var stopwatch = Stopwatch.StartNew();
        if (files.Length == 0)
        {
            Console.WriteLine("No labeled files to merge");
            return;
        }

        var seen = new HashSet<ulong>();
        var chunks = new List<byte[]>();
        long count = 0, total = 0;
        byte[] chunk = null;
        var record = new byte[TrainingRecord.Size];

        foreach (var file in files)
        {
            using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20);
            while (count < merge.MaxPoolPositions && stream.ReadAtLeast(record, TrainingRecord.Size, false) == TrainingRecord.Size)
            {
                total++;
                if (!seen.Add(TrainingRecord.PositionKey(record)))
                    continue;

                int slot = (int)(count % ChunkRecords);
                if (slot == 0)
                {
                    chunk = new byte[ChunkRecords * TrainingRecord.Size];
                    chunks.Add(chunk);
                }
                record.CopyTo(chunk, slot * TrainingRecord.Size);
                count++;
            }
            Console.WriteLine($"{Path.GetFileName(file)}: read={total:N0} unique={count:N0}");
        }

        seen = null;

        var random = new Random(merge.Seed);
        var order = new int[count];
        for (int i = 0; i < order.Length; i++) order[i] = i;
        random.Shuffle(order);

        Directory.CreateDirectory(outputDirectory);
        var trainPath = Path.Combine(outputDirectory, "train.bin");
        var validationPath = Path.Combine(outputDirectory, "val.bin");
        var trainTemp = trainPath + ".tmp";
        var validationTemp = validationPath + ".tmp";
        bool writeValidation = !(keepValidation && File.Exists(validationPath));
        double validationRate = writeValidation ? merge.ValidationRate : 0;

        long train = 0, validation = 0;
        var wdl = new long[3];
        var scoreBuckets = new long[8];
        int bucketSize = Math.Max(1, maxAbsScore / scoreBuckets.Length);

        using (var trainStream = new FileStream(trainTemp, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20))
        using (var validationStream = new FileStream(validationTemp, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20))
        {
            foreach (var index in order)
            {
                var span = new ReadOnlySpan<byte>(chunks[index / ChunkRecords], (index % ChunkRecords) * TrainingRecord.Size, TrainingRecord.Size);

                if (random.NextDouble() < validationRate)
                {
                    validationStream.Write(span);
                    validation++;
                }
                else
                {
                    trainStream.Write(span);
                    train++;
                }

                wdl[Math.Min(span[TrainingRecord.WdlOffset], (byte)2)]++;
                int score = Math.Abs((int)BinaryPrimitives.ReadInt16LittleEndian(span[TrainingRecord.ScoreOffset..]));
                scoreBuckets[Math.Min(score / bucketSize, scoreBuckets.Length - 1)]++;
            }

            trainStream.Flush(true);
            validationStream.Flush(true);
        }

        File.Move(trainTemp, trainPath, true);
        if (writeValidation)
            File.Move(validationTemp, validationPath, true);
        else
            File.Delete(validationTemp);

        Console.WriteLine($"Merge done in {stopwatch.Elapsed:hh\\:mm\\:ss}: read={total:N0} unique={count:N0} train={train:N0} val={validation:N0}");
        Console.WriteLine($"WDL (stm): loss={wdl[0]:N0} draw={wdl[1]:N0} win={wdl[2]:N0}");
        for (int i = 0; i < scoreBuckets.Length; i++)
        {
            Console.WriteLine($"|score| {i * bucketSize,5}..{(i + 1) * bucketSize,5}: {scoreBuckets[i]:N0}");
        }

        if (logPath != null)
        {
            var buckets = string.Join(" ", scoreBuckets.Select(b => b.ToString()));
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            File.AppendAllText(logPath,
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} MERGE  source={source} read={total:N0} unique={count:N0} train={train:N0} val={validation:N0}{(writeValidation ? " (new val.bin)" : " (val.bin kept)")} " +
                $"wdl={wdl[0]}/{wdl[1]}/{wdl[2]} scoreBuckets[{bucketSize}]={buckets}{Environment.NewLine}");
        }
    }
}
