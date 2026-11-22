using System.Diagnostics;

namespace SprtTool;

/// <summary>One engine process (baseline or candidate). Talks line by line over stdin/stdout.</summary>
internal sealed class PlayerProc : IDisposable
{
    private readonly Process _process;
    private readonly TimeSpan _timeout;
    private bool _exitSent;
    public string Name { get; }

    public PlayerProc(string name, string dir, string exe, TimeSpan timeout)
    {
        Name = name;
        _timeout = timeout;
        var info = new ProcessStartInfo(Path.Combine(dir, exe), "--player")
        {
            WorkingDirectory = dir,          // the player reads its own Config\ folder from here
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        _process = Process.Start(info) ?? throw new InvalidOperationException($"Cannot start {info.FileName}");
    }

    // Separate from the constructor so two engines can start at the same time.
    public void WaitReady()
    {
        var ready = ReadReply();
        if (ready[0] != "READY") throw new InvalidOperationException($"{Name}: unexpected first reply {ready[0]}");
    }

    public string[] Ask(string command)
    {
        _process.StandardInput.WriteLine(command);
        return ReadReply();
    }

    // Lines that do not start with "@@" are engine console noise and are ignored.
    private string[] ReadReply()
    {
        while (true)
        {
            var task = _process.StandardOutput.ReadLineAsync();
            if (!task.Wait(_timeout)) throw new TimeoutException($"{Name}: no reply within {_timeout}");
            var line = task.Result ?? throw new IOException($"{Name}: process exited");
            if (!line.StartsWith("@@")) continue;
            var parts = line.Substring(2).Split(' ');
            if (parts[0] == "ERR") throw new InvalidOperationException($"{Name}: {line}");
            return parts;
        }
    }

    // Asks the engine to quit without waiting, so several engines can shut down at the same time.
    public void RequestExit()
    {
        if (_exitSent) return;
        _exitSent = true;
        try { _process.StandardInput.WriteLine("EXIT"); } catch { }
    }

    public void Dispose()
    {
        RequestExit();
        try { _process.WaitForExit(5000); } catch { }
        try { if (!_process.HasExited) _process.Kill(true); } catch { }
        _process.Dispose();
    }
}
