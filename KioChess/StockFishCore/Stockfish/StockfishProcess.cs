using System.Diagnostics;

namespace StockFishCore.Stockfish
{
    internal class StockfishProcess : IDisposable
    {
        private ProcessStartInfo _processStartInfo { get; set; }
        private Process _process { get; set; }
        private bool _disposed;

        public StockfishProcess(string path)
        {
            _processStartInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            _process = new Process { StartInfo = _processStartInfo };
            _disposed = false;
        }

        public void Wait(int millisecond) => _process.WaitForExit(millisecond);

        public void WriteLine(string command)
        {
            if (_process.StandardInput == null)
            {
                throw new NullReferenceException();
            }
            _process.StandardInput.WriteLine(command);
            _process.StandardInput.Flush();
        }

        public string ReadLine()
        {
            if (_process.StandardOutput == null)
            {
                throw new NullReferenceException();
            }
            return _process.StandardOutput.ReadLine();
        }

        public void Start() => _process.Start();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        if (_process != null)
                        {
                            if (!_process.HasExited)
                            {
                                _process.Kill();
                            }
                            _process.Dispose();
                        }
                    }
                    catch { }
                }
                _disposed = true;
            }
        }

        ~StockfishProcess()
        {
            Dispose(false);
        }
    }
}
