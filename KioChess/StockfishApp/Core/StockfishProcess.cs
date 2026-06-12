using System.Diagnostics;

namespace StockfishApp.Core
{
    internal class StockfishProcess : IDisposable
    {
        /// <summary>
        /// Default process info for Stockfish process
        /// </summary>
        private ProcessStartInfo _processStartInfo { get; set; }

        /// <summary>
        /// Stockfish process
        /// </summary>
        private Process _process { get; set; }

        private bool _disposed;

        /// <summary>
        /// Stockfish process constructor
        /// </summary>
        /// <param name="path">Path to usable binary file from stockfish site</param>
        public StockfishProcess(string path)
        {
            //TODO: need add method which should be depended on os version
            _processStartInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };
            _process = new Process { StartInfo = _processStartInfo };
            _disposed = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="millisecond"></param>
        public void Wait(int millisecond) => _process.WaitForExit(millisecond);

        /// <summary>
        /// This method is writing in stdin of Stockfish process
        /// </summary>
        /// <param name="command"></param>
        public void WriteLine(string command)
        {
            if (_process.StandardInput == null)
            {
                throw new NullReferenceException();
            }
            _process.StandardInput.WriteLine(command);
            _process.StandardInput.Flush();
        }

        /// <summary>
        /// This method is allowing to read stdout of Stockfish process
        /// </summary>
        /// <returns></returns>
        public string ReadLine()
        {
            if (_process.StandardOutput == null)
            {
                throw new NullReferenceException();
            }
            return _process.StandardOutput.ReadLine();
        }
        /// <summary>
        /// Start stockfish process
        /// </summary>
        public void Start() => _process.Start();

        /// <summary>
        /// This method is allowing to close Stockfish process
        /// </summary>
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