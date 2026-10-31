using Engine.Models.Config;
using Newtonsoft.Json;
using System.Diagnostics;
using TorchSharp;
using static TorchSharp.torch;

namespace NnueTrainer;

internal class Program
{
	private static int Main(string[] args)
	{
		var configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(@"Config\Configuration.json"));
		var nnue = configuration.NnueConfiguration;
		var network = nnue.Network;
		var training = nnue.Training;

		if (network.Inputs != RecordBatchReader.Inputs)
		{
			Console.WriteLine($"Network.Inputs must be {RecordBatchReader.Inputs}");
			return 1;
		}

		torch.set_num_threads(training.Threads);
		torch.manual_seed(training.Seed);
		Directory.CreateDirectory(nnue.Data.SourceDirectory);
		Directory.CreateDirectory(nnue.Data.OutputDirectory);
		Directory.CreateDirectory(training.OutputDirectory);

		if (!File.Exists(training.TrainPath))
		{
			Console.WriteLine($"Training data not found: {training.TrainPath}. Run 'NnueTool merge' first.");
			return 1;
		}

		var resume = args.Length > 1 && args[0] == "--resume" ? args[1] : null;
		bool chunk = args.Contains("--chunk");
		int epochs = training.Epochs;
		double learningRate = training.LearningRate;
		int firstEpoch = 1;

		if (chunk)
		{
			// Continuous training: resume from the latest checkpoint, few epochs, lower LR after the first chunk.
			var latest = Directory.GetFiles(training.OutputDirectory, "epoch-*.pt").OrderBy(f => f).LastOrDefault();
			epochs = Math.Max(1, training.ChunkEpochs);
			if (latest != null)
			{
				resume = latest;
				firstEpoch = int.Parse(Path.GetFileNameWithoutExtension(latest)["epoch-".Length..]) + 1;
				learningRate *= training.ChunkLearningRateFactor > 0 ? training.ChunkLearningRateFactor : 1;
			}
		}

		using var train = new RecordBatchReader(training.TrainPath, training.BatchSize);
		using var validation = File.Exists(training.ValidationPath)
			? new RecordBatchReader(training.ValidationPath, training.BatchSize)
			: null;

		Console.WriteLine($"Train={train.Count:N0} Val={validation?.Count ?? 0:N0} Hidden={network.Hidden} Batch={training.BatchSize} Epochs={epochs} LR={learningRate} Threads={training.Threads}");

		var logPath = Path.Combine(training.OutputDirectory, "training.log");
		var statePath = Path.Combine(training.OutputDirectory, "training.state");
		// state: "<distinct positions> <position updates>" of all completed runs
		long totalPositions = 0, totalUpdates = 0;
		if (resume != null && File.Exists(statePath))
		{
			var parts = File.ReadAllText(statePath).Split(' ');
			totalPositions = long.Parse(parts[0]);
			totalUpdates = long.Parse(parts[1]);
		}
		Log(logPath, $"START  train={train.Count:N0} val={validation?.Count ?? 0:N0} epochs={firstEpoch}..{firstEpoch + epochs - 1} lr={learningRate} resume={(resume == null ? "none (new model)" : Path.GetFileName(resume))}");

		var model = new NnueModel(network.Hidden);
		if (resume != null)
		{
			model.load(resume);
			Console.WriteLine($"Resumed from {resume}");
		}

		var optimizer = optim.AdamW(model.parameters(), learningRate, weight_decay: training.WeightDecay);
		var random = new Random(training.Seed + firstEpoch);
		var stopwatch = Stopwatch.StartNew();
		var maxDuration = TimeSpan.FromHours(training.MaxHours);
		long totalSteps = (long)train.Batches * epochs;
		long step = 0;
		var order = Enumerable.Range(0, train.Batches).ToArray();

		for (int epoch = firstEpoch; epoch < firstEpoch + epochs; epoch++)
		{
			random.Shuffle(order);
			double epochLoss = 0;
			model.train();

			foreach (var batchIndex in order)
			{
				double progress = (double)step++ / totalSteps;
				double lr = training.FinalLearningRate + 0.5 * (learningRate - training.FinalLearningRate) * (1 + Math.Cos(Math.PI * progress));
				foreach (var group in optimizer.ParamGroups) group.LearningRate = lr;

				using var scope = NewDisposeScope();
				var loss = Loss(model, train.Read(batchIndex, training.Lambda, network.Scale));

				optimizer.zero_grad();
				loss.backward();
				optimizer.step();
				model.Clip(training.WeightClip);

				epochLoss += loss.item<float>();

				if (stopwatch.Elapsed > maxDuration)
					break;
			}

			double validationLoss = Validate(model, validation, training.Lambda, network.Scale);
			Console.WriteLine($"{stopwatch.Elapsed:hh\\:mm\\:ss} epoch {epoch}: train={epochLoss / train.Batches:F6} val={validationLoss:F6}");

			if (!double.IsFinite(epochLoss) || double.IsInfinity(validationLoss))
			{
				Log(logPath, $"FAILED epoch {epoch}: loss is not finite (train={epochLoss}, val={validationLoss}); checkpoint not saved");
				return 4;
			}

			totalUpdates += train.Count;

			var checkpoint = Path.Combine(training.OutputDirectory, $"epoch-{epoch:D3}.pt");
			model.save(checkpoint);
			NetworkExporter.Export(model, network, Path.Combine(training.OutputDirectory, $"kio-{epoch:D3}.nnue"));
			Log(logPath, $"EPOCH  {epoch:D3} train={epochLoss / train.Batches:F6} val={validationLoss:F6} time={stopwatch.Elapsed:hh\\:mm\\:ss} -> kio-{epoch:D3}.nnue");

			if (stopwatch.Elapsed > maxDuration)
			{
				Log(logPath, $"STOP   time limit {training.MaxHours} h reached");
				Console.WriteLine($"Time limit {training.MaxHours} h reached; resume with --resume {checkpoint}");
				return chunk ? 2 : 0; // pipeline keeps train.bin and retries
			}
		}

		totalPositions += train.Count;
		File.WriteAllText(statePath, $"{totalPositions} {totalUpdates}");
		Log(logPath, $"DONE   total distinct positions={totalPositions:N0} position updates={totalUpdates:N0}");
		return 0;
	}

	private static void Log(string path, string message) =>
		File.AppendAllText(path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");

	private static Tensor Loss(NnueModel model, Batch batch)
	{
		var output = model.Forward(
			tensor(batch.StmIndices.ToArray()), tensor(batch.StmOffsets),
			tensor(batch.NstmIndices.ToArray()), tensor(batch.NstmOffsets));
		return nn.functional.mse_loss(output.sigmoid(), tensor(batch.Targets));
	}

	private static double Validate(NnueModel model, RecordBatchReader validation, double lambda, int scale)
	{
		if (validation == null)
			return double.NaN;

		model.eval();
		double sum = 0;
		using (no_grad())
		{
			for (int i = 0; i < validation.Batches; i++)
			{
				using var scope = NewDisposeScope();
				sum += Loss(model, validation.Read(i, lambda, scale)).item<float>();
			}
		}
		return sum / Math.Max(1, validation.Batches);
	}
}
