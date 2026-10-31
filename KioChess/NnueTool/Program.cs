using Engine.Interfaces.Config;
using Engine.Models.Config;
using System.Diagnostics;

namespace NnueTool;

internal class Program
{
	private static int Main(string[] args)
	{
		if (args.Length == 0)
			args = ["all"];

		if (args[0] is "-h" or "--help" or "help")
		{
			PrintUsage();
			return 1;
		}

		Boot.SetUp();
		var nnue = Boot.GetService<IConfigurationProvider>().NnueConfiguration;
		var data = nnue.Data;
		Directory.CreateDirectory(data.SourceDirectory);
		Directory.CreateDirectory(data.OutputDirectory);
		Directory.CreateDirectory(nnue.Training.OutputDirectory);
		var filteredPath = Path.Combine(data.OutputDirectory, data.FilteredFile);

		switch (args[0].ToLowerInvariant())
		{
			case "all":
				return Pipeline.Run(data, GetWorkers(args, data.Label), RunWorkers, Path.Combine(nnue.Training.OutputDirectory, "training.log"));
			case "filter":
			{
				// filter <input.pgn> [output.pgn]
				var input = args.Length > 1 ? args[1] : null;
				if (input == null) { PrintUsage(); return 1; }
				Directory.CreateDirectory(data.OutputDirectory);
				FilterCommand.Run(input, GetOption(args, "--out") ?? filteredPath, data.Filter);
				return 0;
			}
			case "label":
			{
				// label --worker k --workers n [--input filtered.pgn]
				int worker = int.Parse(GetOption(args, "--worker") ?? "0");
				int workers = int.Parse(GetOption(args, "--workers") ?? "1");
				var input = GetOption(args, "--input") ?? filteredPath;
				var outputDirectory = GetOption(args, "--out") ?? data.OutputDirectory;
				new LabelCommand(data.Extract, data.Label, worker).Run(input, outputDirectory, worker, workers);
				return 0;
			}
			case "run":
			{
				// run [--input filtered.pgn] [--workers n]: starts label processes and waits
				var input = GetOption(args, "--input") ?? filteredPath;
				return RunWorkers(input, GetWorkers(args, data.Label), data.OutputDirectory);
			}
			case "merge":
			{
				MergeCommand.Run(data.OutputDirectory, data.OutputDirectory, data.Merge, data.Label.MaxAbsScore);
				return 0;
			}
			default:
				PrintUsage();
				return 1;
		}
	}

	private static int GetWorkers(string[] args, NnueLabelConfiguration label)
	{
		var workersOption = GetOption(args, "--workers");
		return workersOption != null ? int.Parse(workersOption)
			: label.Workers > 0 ? label.Workers
			: Math.Max(1, Environment.ProcessorCount * 3 / 4);
	}

	private static int RunWorkers(string input, int workers, string outputDirectory)
	{
		if (!File.Exists(input) || new FileInfo(input).Length == 0)
		{
			Console.WriteLine($"Input is missing or empty: {input}. Run 'filter' first.");
			return 1;
		}

		// Game partitioning depends on the worker count; resuming with a different count would duplicate/skip games.
		var workersFile = Path.Combine(outputDirectory, "labeled.workers");
		if (File.Exists(workersFile) && int.Parse(File.ReadAllText(workersFile)) != workers
			&& Directory.EnumerateFiles(outputDirectory, "labeled-*.progress").Any())
		{
			Console.WriteLine($"Existing run used {File.ReadAllText(workersFile)} workers; delete labeled-* files or use --workers {File.ReadAllText(workersFile)}.");
			return 1;
		}
		File.WriteAllText(workersFile, workers.ToString());

		var exe = Environment.ProcessPath;
		var processes = new List<Process>();

		Console.CancelKeyPress += (_, e) =>
		{
			e.Cancel = true;
			Console.WriteLine("Stopping workers...");
			foreach (var p in processes)
				if (!p.HasExited) p.Kill();
		};

		Console.WriteLine($"Starting {workers} label workers ({Environment.ProcessorCount} logical processors)");

		for (int i = 0; i < workers; i++)
		{
			var info = new ProcessStartInfo(exe, $"label --worker {i} --workers {workers} --input \"{input}\" --out \"{outputDirectory}\"")
			{
				UseShellExecute = false,
				WorkingDirectory = AppContext.BaseDirectory
			};
			processes.Add(Process.Start(info));
		}

		int failed = 0;
		foreach (var process in processes)
		{
			process.WaitForExit();
			if (process.ExitCode != 0) failed++;
		}

		Console.WriteLine($"All workers finished, failed={failed}");
		return failed == 0 ? 0 : 1;
	}

	private static string GetOption(string[] args, string name)
	{
		int index = Array.IndexOf(args, name);
		return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
	}

	private static void PrintUsage()
	{
		Console.WriteLine("NnueTool [all] [--workers n]   one-click resumable pipeline: filter -> label -> merge for every *.pgn in Data.SourceDirectory");
		Console.WriteLine("NnueTool filter <lichess.pgn> [--out filtered.pgn]");
		Console.WriteLine("NnueTool run [--input filtered.pgn] [--workers n]   (default: Label.Workers, or ProcessorCount * 3 / 4 when it is 0)");
		Console.WriteLine("NnueTool label --worker k --workers n [--input filtered.pgn]");
		Console.WriteLine("NnueTool merge");
		Console.WriteLine("All thresholds come from Config\\Configuration.json -> NnueConfiguration.Data");
	}
}
